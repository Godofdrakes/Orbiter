using LibOrbiter.Converters;
using Newtonsoft.Json;

namespace LibOrbiter.Model.WorldEvents;

[JsonObject]
public class FacilityControlPayload : OrbiterPayload
{
	public long FacilityId { get; set; }

	public long ZoneId { get; set; }
	
	public long WorldId { get; set; }
	
	public int OldFactionId { get; set; }
	public int NewFactionId { get; set; }
	
	public long OutfitId { get; set; }

	public long DurationHeld { get; set; }
	public long Timestamp { get; set; }

	[JsonIgnore]
	public DateTime TimestampUtc => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Timestamp);

	[JsonIgnore]
	public DateTime TimestampLocal => TimestampUtc.ToLocalTime();

	public override void WriteMessage(TextWriter writer, NameCache nameCache)
	{
		writer.Write($"[{TimestampLocal}] ");
		writer.Write(OutfitId);
		writer.Write(" captured ");
		writer.Write(nameCache.GetFacilityName(FacilityId));
		writer.Write(" for the ");
		writer.Write(nameCache.GetFactionName(NewFactionId));
		writer.Write($" ({nameCache.GetZoneName(ZoneId)})");
		writer.WriteLine();
	}
}