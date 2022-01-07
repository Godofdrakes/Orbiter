using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using LibOrbiter.Converters;
using LibOrbiter.Converters.PS2V2;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LibOrbiter;

public class OrbiterAction
{
	[JsonProperty("action")]
	public string Action { get; protected set; } = string.Empty;

	[JsonProperty("service")]
	public string Service { get; } = "event";
}

[JsonObject]
public abstract class OrbiterPayload
{
	[JsonProperty("event_name")]
	public string EventName { get; set; } = string.Empty;

	public abstract string GetMessage();
}

[JsonObject]
public class OrbiterResponse
{
	[JsonProperty("payload")]
	[JsonConverter(typeof(PayloadConverter))]
	public OrbiterPayload? Payload { get; set; }

	[JsonProperty("service")]
	public string Service { get; set; } = string.Empty;

	[JsonProperty("type")]
	public string Type { get; set; } = string.Empty;
}

public class OrbiterEventClient : IDisposable
{
	private readonly ClientWebSocket _webSocket;
	private readonly JsonSerializerSettings _jsonSettings;
	private readonly BlockingCollection<OrbiterAction> _actionQueue = new();
	private readonly BlockingCollection<OrbiterResponse> _responseQueue = new();

	public string ServiceId { get; }

	public string Environment => "ps2";

	public event Action<string>? OnJsonReceived;

	public OrbiterEventClient(string? serviceId = default)
	{
		ServiceId = serviceId ?? "example";

		_webSocket = new ClientWebSocket();

		_jsonSettings = new JsonSerializerSettings()
		{
			ContractResolver = new CompositeContractResolver()
			{
				new ShouldSerializeContractResolver()
				{
					NamingStrategy = new SnakeCaseNamingStrategy()
				}
			}
		};
	}

	public void Send(OrbiterAction action, CancellationToken token = default)
	{
		try
		{
			_actionQueue.Add(action, token);
		}
		catch (OperationCanceledException)
		{
			// Ignore
		}
	}

	public void Send(IEnumerable<OrbiterAction> actions, CancellationToken token = default)
	{
		try
		{
			foreach (var action in actions)
			{
				_actionQueue.Add(action, token);
			}
		}
		catch (OperationCanceledException)
		{
			// Ignore
		}
	}

	public bool Pump([MaybeNullWhen(false)] out OrbiterResponse response, CancellationToken token = default)
	{
		try
		{
			return _responseQueue.TryTake(out response, 0, token);
		}
		catch (OperationCanceledException)
		{
			response = null;
		}

		return false;
	}

	public async Task BackgroundTask(CancellationToken token = default)
	{
		var uri = new Uri($"wss://push.planetside2.com/streaming?environment={Environment}&service-id=s:{ServiceId}");

		await _webSocket.ConnectAsync(uri, token);

		var buffer = new byte[2046];

		while (!token.IsCancellationRequested)
		{
			if (_actionQueue.TryTake(out var action, 0, token))
			{
				var data = JsonConvert.SerializeObject(action);
				var bytes = Encoding.UTF8.GetBytes(data);
				await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, token);
			}

			var response = await Receive(buffer, token);
			if (response != null)
			{
				_responseQueue.Add(response, token);
			}
		}
	}

	private async Task<OrbiterResponse?> Receive(ArraySegment<byte> buffer, CancellationToken token = default)
	{
		if (buffer.Array == null) throw new ArgumentNullException(nameof(buffer.Array));

		await using var memoryStream = new MemoryStream();

		WebSocketReceiveResult result;

		do
		{
			result = await _webSocket.ReceiveAsync(buffer, token);

			memoryStream.Write(buffer.Array, buffer.Offset, result.Count);
		}
		while (!result.EndOfMessage);

		if (result.MessageType == WebSocketMessageType.Close)
		{
			return null;
		}

		memoryStream.Seek(0, SeekOrigin.Begin);

		using var reader = new StreamReader(memoryStream, Encoding.UTF8);

		var json = await reader.ReadToEndAsync();

		OnJsonReceived?.Invoke(json);

		return JsonConvert.DeserializeObject<OrbiterResponse>(json, _jsonSettings);
	}

	public void Dispose()
	{
		_actionQueue.CompleteAdding();
		_responseQueue.CompleteAdding();
		_webSocket.Dispose();
	}
}