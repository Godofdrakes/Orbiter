using Newtonsoft.Json;

namespace LibOrbiter.Model.CharacterEvents;

[JsonObject]
public class VehicleDestroyPayload : OrbiterPayload
{
	public string AttackerCharacterId { get; set; } = string.Empty;
	public string AttackerVehicleId { get; set; } = string.Empty;
	public string AttackerWeaponId { get; set; } = string.Empty;
	public string CharacterId { get; set; } = string.Empty;
	public string VehicleId { get; set; } = string.Empty;
	public string FacilityId { get; set; } = string.Empty;
	public string FactionId { get; set; } = string.Empty;

	public long Timestamp { get; set; }

	[JsonIgnore]
	public DateTime TimestampUtc => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Timestamp);

	[JsonIgnore]
	public DateTime TimestampLocal => TimestampUtc.ToLocalTime();

	public override string GetMessage(NameCache? nameCache = default)
	{
		return $"[{TimestampLocal}] {AttackerCharacterId} destroyed {CharacterId}'s vehicle";
	}
}