using LibOrbiter.Model.PS2V2;
using Newtonsoft.Json;

namespace LibOrbiter.Converters.PS2V2;

public class FactionIdConverter : JsonConverter<FactionId>
{
	public override void WriteJson(JsonWriter writer, FactionId value, JsonSerializer serializer)
	{
		writer.WriteValue((int) value);
	}

	public override FactionId ReadJson(JsonReader reader, Type objectType, FactionId existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		var value = reader.Value as string ?? throw new InvalidOperationException();
		var index = int.Parse(value);
		return Enum.GetValues<FactionId>()[index];
	}
}
