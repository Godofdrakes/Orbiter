using Newtonsoft.Json;

namespace LibOrbiter.Model.PS2;

public class MetagameEventPayload : OrbiterPayload
{
	public string FactionNC { get; set; }
	public string FactionVS { get; set; }
	public string FactionTR { get; set; }
	public string MetagameEventState { get; set; }
	public long MetagameEventId { get; set; }
	public long WorldId { get; set; }
	public long ZoneId { get; set; }
	public long Timestamp { get; set; }

	[JsonIgnore]
	public DateTime TimestampUtc => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Timestamp);

	[JsonIgnore]
	public DateTime TimestampLocal => TimestampUtc.ToLocalTime();
	
	public override void WriteMessage(TextWriter writer, NameCache nameCache)
	{
		writer.Write($"[{TimestampLocal}] ");
		writer.Write(nameCache.GetZoneName(ZoneId));
		writer.Write(" alert started");
		writer.WriteLine();
	}
}