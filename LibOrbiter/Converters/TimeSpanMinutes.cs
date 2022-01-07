using Newtonsoft.Json;

namespace LibOrbiter.Converters;

public class TimeSpanMinutes : JsonConverter<TimeSpan>
{
	public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
	{
		writer.WriteValue((int)value.TotalMinutes);
	}

	public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		var value = reader.Value as string ?? throw new InvalidOperationException();
		var minutes = int.Parse(value);
		return TimeSpan.FromMinutes(minutes);
	}
}