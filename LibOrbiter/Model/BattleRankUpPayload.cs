using Newtonsoft.Json;

namespace LibOrbiter.Model;

[JsonObject]
public class BattleRankUpPayload : OrbiterPayload
{
	[JsonProperty("character_id")]
	public string CharacterId { get; set; } = string.Empty;
	
	[JsonProperty("battle_rank")]
	public int BattleRank { get; set; }

	[JsonProperty("timestamp")]
	private long _timestamp;

	[JsonProperty("world_id")]
	public string WorldId { get; set; } = string.Empty;

	[JsonProperty("zone_id")]
	public string ZoneId { get; set; } = string.Empty;

	[JsonIgnore]
	public DateTime TimestampUtc => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(_timestamp);

	[JsonIgnore]
	public DateTime TimestampLocal => TimestampUtc.ToLocalTime();

	public override string GetMessage()
	{
		return $"[{TimestampLocal}] {CharacterId} ranked up ({BattleRank})";
	}
}