using Newtonsoft.Json;

namespace LibOrbiter.Model.CharacterEvents;

[JsonObject]
public class DeathPayload : OrbiterPayload
{
	public long AttackerCharacterId { get; set; }
	public long CharacterId { get; set; }
	public long VehicleId { get; set; }
	public long WorldId { get; set; }
	public long ZoneId { get; set; }

	public long Timestamp { get; set; }

	public int IsHeadshot { get; set; }
	public int IsCritical { get; set; }

	[JsonIgnore]
	public DateTime TimestampUtc => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Timestamp);

	[JsonIgnore]
	public DateTime TimestampLocal => TimestampUtc.ToLocalTime();
	
	public override void WriteMessage(TextWriter writer, NameCache nameCache)
	{
		writer.Write($"[{TimestampLocal}] ");
		writer.Write(nameCache.GetCharacterName(CharacterId));
		writer.Write(IsHeadshot == 1 ? " headshot " : " killed ");
		writer.Write(nameCache.GetCharacterName(CharacterId));
		writer.WriteLine();
	}
}