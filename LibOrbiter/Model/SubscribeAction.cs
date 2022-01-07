using Newtonsoft.Json;

namespace LibOrbiter.Model;

[JsonObject]
public class SubscribeAction : OrbiterAction
{
	public SubscribeAction() => Action = "subscribe";

	[JsonProperty("characters", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public List<string> Characters { get; } = new();

	[JsonProperty("eventNames", DefaultValueHandling = DefaultValueHandling.Ignore)]
	private List<string> EventNames { get; } = new();

	[JsonProperty("worlds", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public List<string> Worlds { get; } = new();

	[JsonProperty("logicalAndCharactersWithWorlds", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool LogicalAndCharactersWithWorlds { get; set; }

	public void AddEvent<T>() where T : OrbiterPayload => AddEvent(typeof(T));

	public void AddEventRange(params Type[] eventTypes)
	{
		foreach (var eventType in eventTypes)
		{
			AddEvent(eventType);
		}
	}

	public void AddEvent(Type eventType)
	{
		if (!eventType.IsSubclassOf(typeof(OrbiterPayload)))
			throw new ArgumentException($"Type must be subclass of {nameof(OrbiterPayload)}", nameof(eventType));
		
		var eventName = eventType.Name.Substring(0, eventType.Name.IndexOf("Payload", StringComparison.Ordinal));

		EventNames.Add(eventName);
	}
}