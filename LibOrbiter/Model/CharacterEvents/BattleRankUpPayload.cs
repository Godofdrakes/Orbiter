using Newtonsoft.Json;

namespace LibOrbiter.Model.CharacterEvents;

[JsonObject]
public class BattleRankUpPayload : OrbiterPayload
{
	public long CharacterId { get; set; }
	public long WorldId { get; set; }
	public long ZoneId { get; set; }

	public long Timestamp { get; set; }

	public int BattleRank { get; set; }

	[JsonIgnore]
	public DateTime TimestampUtc => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Timestamp);

	[JsonIgnore]
	public DateTime TimestampLocal => TimestampUtc.ToLocalTime();

	public override void WriteMessage(TextWriter writer, NameCache nameCache)
	{
		writer.Write($"[{TimestampLocal}] ");
		writer.Write(nameCache.GetCharacterName(CharacterId));
		writer.Write(" ranked up ");
		writer.Write($"({BattleRank})");
		writer.WriteLine();
	}
}