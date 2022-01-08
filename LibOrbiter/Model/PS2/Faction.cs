using Newtonsoft.Json;

namespace LibOrbiter.Model.PS2V2;

[JsonObject]
public class Faction
{
	public int FactionId { get; set; }

	public string CodeTag { get; set; } = string.Empty;

	public string ImageSetId { get; set; } = string.Empty;

	public string ImageId { get; set; } = string.Empty;

	public string ImagePath { get; set; } = string.Empty;

	public Dictionary<string, string> Name { get; } = new();
}