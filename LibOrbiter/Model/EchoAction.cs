using Newtonsoft.Json;

namespace LibOrbiter.Model;

[JsonObject]
public class EchoAction : OrbiterAction
{
	public EchoAction() => Action = "echo";

	[JsonProperty("payload")]
	public object Payload { get; set; } = new { };
}