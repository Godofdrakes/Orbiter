using Newtonsoft.Json;

namespace LibOrbiter.Model.WorldEvents;

[JsonObject]
public class FacilityControlPayload : OrbiterPayload
{
	public string WorldId { get; set; } = string.Empty;
	public string ZoneId { get; set; } = string.Empty;
	public string OutfitId { get; set; } = string.Empty;
	public string FacilityId { get; set; } = string.Empty;
	public string DurationHeld { get; set; } = string.Empty;

	public int OldFactionId { get; set; }
	public int NewFactionId { get; set; }
	
	public long Timestamp { get; set; }

	[JsonIgnore]
	public DateTime TimestampUtc => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Timestamp);

	[JsonIgnore]
	public DateTime TimestampLocal => TimestampUtc.ToLocalTime();

	public override string GetMessage(NameCache? nameCache = default)
	{
		return $"{OutfitId} captured {FacilityId} for the {nameCache?.GetFactionName(NewFactionId) ?? NewFactionId.ToString()}";
	}
}