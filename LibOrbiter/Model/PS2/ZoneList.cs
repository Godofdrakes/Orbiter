using LibOrbiter.Model;
using LibOrbiter.Model.PS2;
using Newtonsoft.Json;

namespace LibOrbiter.Converters;

[JsonObject]
public class ZoneList : ResponseList
{
	[JsonProperty("zone_list")]
	public List<Zone> List { get; } = new();
}