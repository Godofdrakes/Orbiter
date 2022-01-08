using Newtonsoft.Json;

namespace LibOrbiter.Model.CharacterEvents;

[JsonObject]
public class DeathPayload : OrbiterPayload
{
	public string AttackerCharacterId { get; set; } = string.Empty;
	public string CharacterId { get; set; } = string.Empty;
	public string VehicleId { get; set; } = string.Empty;
	public string WorldId { get; set; } = string.Empty;
	public string ZoneId { get; set; } = string.Empty;

	public int IsHeadshot { get; set; }
	public int IsCritical { get; set; }

	public long Timestamp { get; set; }

	[JsonIgnore]
	public DateTime TimestampUtc => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Timestamp);

	[JsonIgnore]
	public DateTime TimestampLocal => TimestampUtc.ToLocalTime();

	public override string GetMessage(NameCache? nameCache = default)
	{
		return $"[{TimestampLocal}] {AttackerCharacterId} killed {CharacterId}{(IsHeadshot == 1 ? " (headshot)" : "")}";
	}
}