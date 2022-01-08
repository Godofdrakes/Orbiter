using Newtonsoft.Json;

namespace LibOrbiter.Model.CharacterEvents;

[JsonObject]
public class VehicleDestroyPayload : OrbiterPayload
{
	public long AttackerCharacterId { get; set; }
	public long AttackerVehicleId { get; set; }
	public long AttackerWeaponId { get; set; }
	public long CharacterId { get; set; }
	public long VehicleId { get; set; }
	public long FacilityId { get; set; }
	public long FactionId { get; set; }

	public long Timestamp { get; set; }

	[JsonIgnore]
	public DateTime TimestampUtc => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Timestamp);

	[JsonIgnore]
	public DateTime TimestampLocal => TimestampUtc.ToLocalTime();

	public override void WriteMessage(TextWriter writer, NameCache nameCache)
	{
		writer.Write($"[{TimestampLocal}] ");
		writer.Write(nameCache.GetCharacterName(AttackerCharacterId));
		writer.Write(" destroyed ");
		writer.Write(nameCache.GetCharacterName(CharacterId));
		writer.Write("'s vehicle");
		writer.WriteLine();
	}
}