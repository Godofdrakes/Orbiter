using LibOrbiter.Model.PS2V2;
using Newtonsoft.Json;

namespace LibOrbiter.Model.PS2;

[JsonObject]
public class FactionList : ResponseList
{
	[JsonProperty("faction_list")]
	public List<Faction> List { get; } = new();
}