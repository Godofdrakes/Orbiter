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
public class OrbiterEchoAction : OrbiterAction
{
	public OrbiterEchoAction() => Action = "echo";
	
	[JsonProperty("payload")]
	public object Payload { get; set; } = new {};
}

[JsonObject]
public class OrbiterSubscribeAction : OrbiterAction
{
	public OrbiterSubscribeAction() => Action = "subscribe";
	
	[JsonProperty("characters", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public List<string> Characters { get; } = new();
	
	[JsonProperty("eventNames", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public List<string> EventNames { get; } = new();

	[JsonProperty("worlds", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public List<string> Worlds { get; } = new();
	
	[JsonProperty("logicalAndCharactersWithWorlds", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool LogicalAndCharactersWithWorlds { get; set; }
}

[JsonObject]
public class OrbiterPayload
{
	[JsonProperty("event_name")]
	public string EventName { get; set; } = string.Empty;
}

[JsonObject]
public class OrbiterResponse
{
	[JsonProperty("payload")]
	[JsonConverter(typeof(OrbiterPayloadConverter))]
	public OrbiterPayload? Payload { get; set; }

	[JsonProperty("service")]
	public string Service { get; set; } = string.Empty;

	[JsonProperty("type")]
	public string Type { get; set; } = string.Empty;
}

[JsonObject]
public class OrbiterDeathPayload : OrbiterPayload
{
	[JsonProperty("attacker_character_id")]
	public string AttackerCharacterId { get; set; } = string.Empty;

	[JsonProperty("character_id")]
	public string CharacterId { get; set; } = string.Empty;

	[JsonProperty("is_headshot")]
	private int _isHeadshot;

	[JsonProperty("timestamp")]
	private long _timestamp;

	[JsonIgnore]
	public bool IsHeadshot => _isHeadshot == 1;

	[JsonIgnore]
	public DateTime Timestamp => DateTime.FromFileTimeUtc(_timestamp);
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
				new CamelCasePropertyNamesContractResolver(),
				new ShouldSerializeContractResolver()
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