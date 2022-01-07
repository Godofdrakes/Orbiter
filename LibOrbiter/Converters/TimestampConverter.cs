using Newtonsoft.Json;

namespace LibOrbiter.Converters;

public class TimestampConverter : JsonConverter<DateTime>
{
	public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
	{
		throw new NotSupportedException();
	}

	public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		var value = reader.Value as string ?? throw new InvalidOperationException();
		var timestamp = long.Parse(value);
		return DateTime.FromFileTimeUtc(timestamp);
	}
}