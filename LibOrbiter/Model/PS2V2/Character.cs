using LibOrbiter.Converters.PS2V2;
using Newtonsoft.Json;

namespace LibOrbiter.Model.PS2V2;

[JsonObject]
public class Character
{
	[JsonObject]
	public struct CharacterName
	{
		[JsonProperty("first")]
		public string First = string.Empty;

		[JsonProperty("first_lower")]
		public string FirstLower = string.Empty;
	}

	[JsonObject]
	public struct CharacterTimes
	{
		[JsonProperty("creation_date")]
		public DateTime Creation;

		[JsonProperty("last_login_date")]
		public DateTime LastLogin;

		[JsonProperty("minutes_played")]
		[JsonConverter(typeof(TimeSpanMinutes))]
		public TimeSpan MinutesPlayed;
	}

	[JsonObject]
	public struct CharacterCerts
	{
		[JsonProperty("earned_points")]
		public int Earned;

		[JsonProperty("gifted_points")]
		public int Gifted;

		[JsonProperty("spent_points")]
		public int Spent;

		[JsonProperty("available_points")]
		public int Available;

		[JsonProperty("percent_to_next")]
		public float Progress;
	}

	[JsonObject]
	public struct CharacterRank
	{
		[JsonProperty("percent_to_next")]
		public int Progress;

		[JsonProperty("value")]
		public int Value;
	}
	
	[JsonObject]
	public struct CharacterRibbons
	{
		[JsonProperty("count")]
		public int Count;

		[JsonProperty("date")]
		public DateTime Date;
	}
	
	[JsonProperty("character_id")]
	public string Id = string.Empty;

	[JsonProperty("name")]
	public CharacterName Name;

	[JsonProperty("faction_id")]
	[JsonConverter(typeof(FactionIdConverter))]
	public FactionId FactionId;

	[JsonProperty("head_id")]
	public string HeadId = string.Empty;

	[JsonProperty("title_id")]
	public string TitleId = string.Empty;

	[JsonProperty("times")]
	public CharacterTimes Times;

	[JsonProperty("certs")]
	public CharacterCerts Certs;

	[JsonProperty("battle_rank")]
	public CharacterRank Rank;

	[JsonProperty("profile_id")]
	public string ProfileId = string.Empty;

	[JsonProperty("daily_ribbon")]
	public CharacterRibbons Ribbons;

	[JsonProperty("prestige_level")]
	public int PrestigeLevel;
}