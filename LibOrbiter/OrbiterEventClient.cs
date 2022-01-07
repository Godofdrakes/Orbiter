using System.Net.WebSockets;
using System.Text;
using LibOrbiter.Converters;
using LibOrbiter.Converters.PS2V2;
using Newtonsoft.Json;

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
	
	[JsonProperty("characters")]
	public List<string> Characters { get; } = new();
	
	[JsonProperty("eventNames")]
	public List<string> EventNames { get; } = new();
}

[JsonObject]
public class OrbiterPayload
{
	[JsonProperty("event_name")]
	public string EventName { get; set; }
}

[JsonObject]
public class OrbiterEvent
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
	
	public string ServiceId { get; }

	public string Environment => "ps2";

	public event Action<string> OnJsonReceived;

	public OrbiterEventClient(string? serviceId = default)
	{
		ServiceId = serviceId ?? "example";

		_webSocket = new ClientWebSocket();
	}

	public async Task Connect(CancellationToken token = default)
	{
		var uri = new Uri($"wss://push.planetside2.com/streaming?environment={Environment}&service-id=s:{ServiceId}");
		await _webSocket.ConnectAsync(uri, token);
	}

	public async Task Send(OrbiterAction action, CancellationToken token = default)
	{
		var data = JsonConvert.SerializeObject(action);
		await _webSocket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, token);
	}

	public async Task<OrbiterEvent?> Receive(ArraySegment<byte> buffer, CancellationToken token = default)
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

		return JsonConvert.DeserializeObject<OrbiterEvent>(json);
	}

	public void Dispose()
	{
		_webSocket.Dispose();
	}
}