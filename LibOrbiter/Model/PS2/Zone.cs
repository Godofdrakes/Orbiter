using Newtonsoft.Json;

namespace LibOrbiter.Model.PS2;

[JsonObject]
public class Zone
{
	public int ZoneId { get; set; }

	public int HexSize { get; set; }

	public string Code { get; set; } = string.Empty;

	public Dictionary<string, string> Name { get; } = new();

	public Dictionary<string, string> Description { get; } = new();

	public List<MapRegion> Regions { get; } = new();
}