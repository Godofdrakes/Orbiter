using LibOrbiter.Converters;
using Newtonsoft.Json;

namespace LibOrbiter.Model.PS2;

[JsonObject]
public class Character
{
	[JsonObject]
	public struct CharacterTimes
	{
		public DateTime CreationDate;

		public DateTime LastLoginDate;

		[JsonConverter(typeof(TimeSpanMinutes))]
		public TimeSpan MinutesPlayed;
	}

	[JsonObject]
	public struct CharacterCerts
	{
		public int EarnedPoints;

		public int GiftedPoints;

		public int SpentPoints;

		public int AvailablePoints;

		public float ProgressToNext;
	}

	[JsonObject]
	public struct CharacterRank
	{
		public int ProgressToNext;

		public int Value;
	}
	
	[JsonObject]
	public struct CharacterRibbons
	{
		public int Count;

		public DateTime Date;
	}
	
	public Dictionary<string, string> Name { get; } = new();

	public long CharacterId { get; set; }

	public long ProfileId { get; set; }

	public int FactionId { get; set; }

	public long TitleId { get; set; }

	public long HeadId { get; set; }

	public int PrestigeLevel { get; set; }

	public CharacterTimes Times { get; set; }

	public CharacterCerts Certs { get; set; }

	public CharacterRank BattleRank { get; set; }

	public CharacterRibbons DailyRibbon { get; set; }
}