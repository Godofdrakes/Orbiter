using System.Collections.Concurrent;
using LibOrbiter.Model.PS2V2;

namespace LibOrbiter;

public class NameCache
{
	private readonly ConcurrentDictionary<int, string> _factionNames = new();

	private readonly string _languageId;

	public NameCache(string? languageId = default)
	{
		_languageId = languageId ?? "en";
	}

	public async Task CacheFactionNames(OrbiterClient client, CancellationToken token = default)
	{
		var factions = await client.GetAsync<FactionList>("faction", token,
			new KeyValuePair<string, string>("faction_id", "0,1,2,3,4"));

		foreach (var faction in factions.List)
		{
			_factionNames.TryAdd(faction.FactionId, faction.Name[_languageId]);
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