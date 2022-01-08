using Newtonsoft.Json;

namespace LibOrbiter.Model.WorldEvents;

public class ContinentLockPayload : OrbiterPayload
{
	public long ZoneId { get; set; }
	public long WorldId { get; set; }

	public long MetagameEventId { get; set; }

	public long TriggeringFaction { get; set; }
	public long PreviousFaction { get; set; }

	public long Timestamp { get; set; }

	public string VSPopulation { get; set; }
	public string NCPopulation { get; set; }
	public string TRPopulation { get; set; }

	[JsonIgnore]
	public DateTime TimestampUtc => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Timestamp);

	[JsonIgnore]
	public DateTime TimestampLocal => TimestampUtc.ToLocalTime();

	public override void WriteMessage(TextWriter writer, NameCache nameCache)
	{
		writer.Write($"[{TimestampLocal}] ");
		writer.Write(nameCache.GetZoneName(ZoneId));
		writer.Write(" locked by ");
		writer.Write(nameCache.GetFactionName(TriggeringFaction));
		writer.WriteLine();
	}
}