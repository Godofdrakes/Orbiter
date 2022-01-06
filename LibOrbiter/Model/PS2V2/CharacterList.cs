using Newtonsoft.Json;

namespace LibOrbiter.Model.PS2V2;

[JsonObject]
public class CharacterList
{
	[JsonProperty("character_list")]
	public List<Character> List = new();
}