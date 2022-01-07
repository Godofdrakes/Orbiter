using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace LibOrbiter.Converters;

public class OrbiterPayloadConverter : JsonConverter<OrbiterPayload>
{
	public static Dictionary<string, Type> EventTypeLookup { get; } = new();

	public override void WriteJson(JsonWriter writer, OrbiterPayload? value, JsonSerializer serializer)
	{
		throw new NotSupportedException();
	}

	public override OrbiterPayload? ReadJson(JsonReader reader, Type objectType, OrbiterPayload? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
			return null;

		var obj = JObject.Load(reader);

		var eventType = obj.GetValue("event_type");

		return null;
	}
}