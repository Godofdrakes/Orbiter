using Newtonsoft.Json;

namespace LibOrbiter.Model.PS2V2;

[JsonObject]
public class FactionList
{
	[JsonProperty("faction_list")]
	public List<Faction> List { get; } = new();
}