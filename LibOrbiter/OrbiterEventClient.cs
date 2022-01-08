using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using LibOrbiter.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace LibOrbiter;

[JsonObject]
public class OrbiterAction
{
	public string Action { get; protected set; } = string.Empty;

	public string Service { get; } = "event";
}

[JsonObject]
public abstract class OrbiterPayload
{
	public string EventName { get; set; } = string.Empty;

	public abstract void WriteMessage(TextWriter writer, NameCache nameCache);
}

[JsonObject]
public class OrbiterResponse
{
	[JsonConverter(typeof(PayloadConverter))]
	public OrbiterPayload? Payload { get; set; }

	public string Service { get; set; } = string.Empty;

	public string Type { get; set; } = string.Empty;
}

public class OrbiterClient : IDisposable
{
	private readonly RestClient _restClient;
	private readonly ClientWebSocket _webSocket;
	private readonly JsonSerializerSettings _jsonSettings;
	private readonly BlockingCollection<OrbiterAction> _actionQueue = new(8);
	private readonly BlockingCollection<OrbiterResponse> _responseQueue = new(8);

	public string ServiceId { get; }

	public string RestEnvironment => "ps2:v2";

	public string EventEnvironment => "ps2";

	public event Action<string>? OnEventReceived;

	public OrbiterClient(string? serviceId = default)
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

		_restClient = new RestClient($"http://census.daybreakgames.com/s:{ServiceId}");
		_restClient.UseSerializer(() => new JsonNetSerializer(_jsonSettings));
	}

	public void SendAction(OrbiterAction action, CancellationToken token = default)
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

	public async void OpenEventConnection(CancellationToken token = default)
	{
		var uri = new Uri($"wss://push.planetside2.com/streaming?environment={EventEnvironment}&service-id=s:{ServiceId}");

		await _webSocket.ConnectAsync(uri, token);

		var responseBuffer = new byte[2046];

		while (true)
		{
			if (_actionQueue.TryTake(out var action, 0, token))
			{
				var data = JsonConvert.SerializeObject(action);
				var bytes = Encoding.UTF8.GetBytes(data);
				await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, token);
			}

			var json = await Receive(responseBuffer, token);
			if (!string.IsNullOrEmpty(json))
			{
				OnEventReceived?.Invoke(json);

				var response = JsonConvert.DeserializeObject<OrbiterResponse>(json, _jsonSettings);

				if (response == null) throw new InvalidOperationException();
				
				_responseQueue.Add(response, token);
			}

			if (token.IsCancellationRequested)
			{
				break;
			}
		}
	}
	
	private string ReadJson(Stream stream)
	{
		using var streamReader = new StreamReader(stream, Encoding.UTF8);

		stream.Seek(0, SeekOrigin.Begin);

		return streamReader.ReadToEnd();
	}
	
	private async Task<string?> Receive(ArraySegment<byte> buffer, CancellationToken token = default)
	{
		if (buffer.Array == null) throw new ArgumentNullException(nameof(buffer.Array));

		await using var memoryStream = new MemoryStream();

		try
		{
			WebSocketReceiveResult result;

			do
			{
				result = await _webSocket.ReceiveAsync(buffer, token);
				
				memoryStream.Write(buffer.Array, buffer.Offset, result.Count);

				if (token.IsCancellationRequested)
				{
					return null;
				}
			}
			while (!result.EndOfMessage);
		}
		catch (OperationCanceledException)
		{
			return null;
		}

		return ReadJson(memoryStream);
	}
	
	public async Task<string> GetJson(string resource, KeyValuePair<string, string>[] queryParams, CancellationToken token = default)
	{
		var uri = new Uri($"get/{RestEnvironment}/{resource}", UriKind.Relative);
		var request = new RestRequest(uri, Method.GET, DataFormat.Json);
		foreach (var (name, value) in queryParams) request.AddQueryParameter(name, value);
		var response = await _restClient.ExecuteGetAsync(request, token);
		ThrowIfError(response);
		return response.Content;
	}

	public T Get<T>(string resource, IEnumerable<(string, string)> queryParams)
	{
		var uri = new Uri($"get/{RestEnvironment}/{resource}", UriKind.Relative);
		var request = new RestRequest(uri, Method.GET, DataFormat.Json);
		foreach (var (name, value) in queryParams) request.AddQueryParameter(name, value);
		var response = _restClient.Execute<T>(request);
		ThrowIfError(response);
		return response.Data;
	}

	public T Get<T>(string resource, List<(string, string)> queryParams) => Get<T>(resource, queryParams.AsEnumerable());
	public T Get<T>(string resource, params (string, string)[] queryParams) => Get<T>(resource, queryParams.AsEnumerable());

	public async Task<T> GetAsync<T>(string resource, CancellationToken token = default, params KeyValuePair<string, string>[] queryParams)
	{
		var uri = new Uri($"get/{RestEnvironment}/{resource}", UriKind.Relative);
		var request = new RestRequest(uri, Method.GET, DataFormat.Json);
		foreach (var (name, value) in queryParams) request.AddQueryParameter(name, value);
		return await _restClient.GetAsync<T>(request, token);
	}

	private void ThrowIfError(IRestResponse response)
	{
		var exception = response.ResponseStatus switch
		{
			ResponseStatus.Aborted   => new WebException("Request aborted", response.ErrorException),
			ResponseStatus.Error     => response.ErrorException,
			ResponseStatus.TimedOut  => new TimeoutException("Request timed out", response.ErrorException),
			ResponseStatus.None      => null,
			ResponseStatus.Completed => null,
			_                        => throw response.ErrorException ?? new ArgumentOutOfRangeException()
		};

		if (exception != null)
			throw exception;
	}

	public void Dispose()
	{
		_actionQueue.CompleteAdding();
		_responseQueue.CompleteAdding();
		_webSocket.Dispose();
	}
}