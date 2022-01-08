using Newtonsoft.Json;

namespace LibOrbiter.Model.PS2;

[JsonObject]
public class MapRegionList : ResponseList
{
	[JsonProperty("map_region_list")]
	public List<MapRegion> List { get; } = new();
}