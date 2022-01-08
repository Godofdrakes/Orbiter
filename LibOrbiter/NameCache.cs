using System.Collections.Concurrent;
using LibOrbiter.Converters;
using LibOrbiter.Model.PS2;
using LibOrbiter.Model.PS2V2;
using Newtonsoft.Json;

namespace LibOrbiter;

public class NameCache
{
	private readonly Dictionary<long, string> _factionNames = new();

	private readonly Dictionary<long, string> _characterNames = new();

	private readonly Dictionary<long, string> _zoneNames = new();

	private readonly Dictionary<long, string> _facilityNames = new();

	public readonly string LanguageCode;

	public NameCache(string? languageCode = default)
	{
		LanguageCode = languageCode ?? "en";
	}

	public void LoadFactionNames(string filePath, CancellationToken token = default)
	{
		using var file = File.OpenText(filePath);
		using var reader = new JsonTextReader(file);
		var serializer = new JsonSerializer();
		var factions = serializer.Deserialize<FactionList>(reader);

		if (factions == null)
			throw new InvalidOperationException();

		foreach (var faction in factions.List)
		{
			_factionNames[faction.FactionId] = faction.Name[LanguageCode];
		}
	}

	public int CacheFactionNames(OrbiterClient orbiterClient) => IterativeGet<FactionList>(orbiterClient, "faction",
		factions =>
		{
			foreach (var faction in factions.List)
				_factionNames.TryAdd(faction.FactionId, faction.Name[LanguageCode]);
		});

	public int CacheCharacterNames(OrbiterClient orbiterClient, params string[] characterIds)
	{
		var characters =
			orbiterClient.Get<CharacterList>("character", ("character_id", string.Join(',', characterIds)));
		foreach (var character in characters.List)
			_characterNames.TryAdd(character.CharacterId, character.Name["first"]);

		return _characterNames.Count;
	}

	public int CacheZoneNames(OrbiterClient orbiterClient) => IterativeGet<ZoneList>(orbiterClient, "zone", zones =>
	{
		foreach (var zone in zones.List)
			_zoneNames[zone.ZoneId] = zone.Name[LanguageCode];
	});

	public int CacheRegionNames(OrbiterClient orbiterClient) => IterativeGet<MapRegionList>(orbiterClient, "map_region",
		mapRegions =>
		{
			foreach (var region in mapRegions.List)
				_facilityNames[region.FacilityId] = region.FacilityTypeId switch
				{
					2 => $"{region.FacilityName} {region.FacilityType}",
					3 => $"{region.FacilityName} {region.FacilityType}",
					4 => $"{region.FacilityName} {region.FacilityType}",
					_ => region.FacilityName
				};
		});

	public string GetFactionName(long factionId)
	{
		if (_factionNames.TryGetValue(factionId, out var factionName))
		{
			return factionName;
		}

		return $"Unknown ({factionId})";
	}

	public string GetCharacterName(long characterId)
	{
		if (_characterNames.TryGetValue(characterId, out var characterName))
		{
			return characterName;
		}

		return $"Unknown ({characterId})";
	}

	public string GetZoneName(long zoneId)
	{
		if (_zoneNames.TryGetValue(zoneId, out var zoneName))
		{
			return zoneName;
		}

		return $"Unknown ({zoneId})";
	}

	public string GetFacilityName(long facilityId)
	{
		if (_facilityNames.TryGetValue(facilityId, out var facilityName))
		{
			return facilityName;
		}

		return $"Unknown ({facilityId})";
	}

	private int IterativeGet<T>(OrbiterClient orbiterClient, string resource, Action<T> handler,
		params (string, string)[] queryParams)
		where T : ResponseList
	{
		var count = 0;
		var index = 0;
		var stride = 100;

		T responseList;

		do
		{
			var rangeParams = new List<(string, string)>
			{
				("c:start", index.ToString()),
				("c:limit", stride.ToString()),
			};

			responseList = orbiterClient.Get<T>(resource, queryParams.Concat(rangeParams));

			if (responseList.Returned > 0)
			{
				handler(responseList);
			}

			index += stride;
			count += responseList.Returned;
		}
		while (responseList.Returned >= stride);

		return count;
	}
}