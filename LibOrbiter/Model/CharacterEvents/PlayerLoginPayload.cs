using Newtonsoft.Json;

namespace LibOrbiter.Model.CharacterEvents;

[JsonObject]
public class PlayerLoginPayload : OrbiterPayload
{
	public string CharacterId { get; set; } = string.Empty;
	public string WorldId { get; set; } = string.Empty;

	public long Timestamp { get; set; }

	[JsonIgnore]
	public DateTime TimestampUtc => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Timestamp);

	[JsonIgnore]
	public DateTime TimestampLocal => TimestampUtc.ToLocalTime();

	public override string GetMessage(NameCache? nameCache = default)
	{
		return $"[{TimestampLocal}] {CharacterId} logged in";
	}
}