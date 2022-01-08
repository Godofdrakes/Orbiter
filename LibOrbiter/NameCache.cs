using System.Collections.Concurrent;
using LibOrbiter.Model.PS2V2;

namespace LibOrbiter;

public class NameCache
{
	private readonly ConcurrentDictionary<int, string> _factionNames = new();

	private readonly ConcurrentDictionary<string, string> _characterNames = new();

	private readonly string _languageId;

	public NameCache(string? languageId = default)
	{
		_languageId = languageId ?? "en";
	}

	public async Task CacheFactionNames(int[] factionIds, OrbiterClient client, CancellationToken token = default)
	{
		var factions = await client.GetAsync<FactionList>("faction", token,
			new KeyValuePair<string, string>("faction_id", string.Join(',', factionIds)));

		foreach (var faction in factions.List)
		{
			_factionNames.TryAdd(faction.FactionId, faction.Name[_languageId]);
		}
	}

	public async Task CacheCharacterNames(string[] characterIds, OrbiterClient client, CancellationToken token = default)
	{
		var characters = await client.GetAsync<CharacterList>("character", token,
			new KeyValuePair<string, string>("character_id", string.Join(',', characterIds)));

		foreach (var character in characters.List)
		{
			_characterNames.TryAdd(character.Id, character.Name.First);
		}
	}

	public string GetFactionName(int factionId)
	{
		if (_factionNames.TryGetValue(factionId, out var factionName))
		{
			return factionName;
		}

		return $"Unknown ({factionId})";
	}
}