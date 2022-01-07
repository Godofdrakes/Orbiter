using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibOrbiter.Converters;

public class PayloadConverter : JsonConverter
{
	private static readonly Dictionary<string, Type> EventNameLookup = new();

	static PayloadConverter()
	{
		var payloadTypes = Assembly.GetExecutingAssembly().GetTypes()
			.Where(type => type.IsSubclassOf(typeof(OrbiterPayload)));
		foreach (var type in payloadTypes)
		{
			var eventName = type.Name.Substring(0, type.Name.IndexOf("Payload", StringComparison.Ordinal));
			EventNameLookup.Add(eventName, type);
		}
	}

	public override bool CanRead => true;
	
	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		throw new NotSupportedException();
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
			return null;

		var obj = JObject.Load(reader);

		var eventNameProperty = obj.GetValue("event_name");

		if (eventNameProperty is not {Type: JTokenType.String})
			return null;

		var eventName = eventNameProperty.Value<string>()!;

		if (!EventNameLookup.TryGetValue(eventName, out var type)) 
			return null;
		
		using var subReader = obj.CreateReader();

		var payload = Activator.CreateInstance(type) ?? throw new InvalidOperationException();
		
		serializer.Populate(subReader, payload);

		return payload;

	}

	public override bool CanConvert(Type objectType) => objectType.IsAssignableTo(typeof(OrbiterPayload));
}