using LibOrbiter;
using LibOrbiter.Model;
using LibOrbiter.Model.CharacterEvents;

namespace Orbiter.Console;

static class Program
{
	static void Main(string[] args)
	{
		var tokenSource = new CancellationTokenSource();

		var token = tokenSource.Token;

		System.Console.CancelKeyPress += (sender, eventArgs) =>
		{
			eventArgs.Cancel = true;

			if (!tokenSource.IsCancellationRequested)
			{
				tokenSource.Cancel();
				tokenSource.Dispose();
			}
		};

		var eventClient = new OrbiterClient();
		
#if DEBUG
		eventClient.OnEventReceived += json =>
		{
			using var stringReader = new StringReader(json);
			using var stringWriter = new StringWriter();

			var jsonReader = new JsonTextReader(stringReader);
			var jsonWriter = new JsonTextWriter(stringWriter)
			{
				Formatting = Formatting.Indented
			};

			jsonWriter.WriteToken(jsonReader);

			System.Console.WriteLine(stringWriter.ToString());
		};
#endif

		System.Console.WriteLine("Precaching names...");

		var nameCache = new NameCache();

		var cacheFactionNames = Task.Run(() => nameCache.CacheFactionNames(new[] {0, 1, 2, 3, 4}, eventClient, token), token);
		var cacheCharacterNames = Task.Run(() => nameCache.CacheCharacterNames(args, eventClient, token), token);
		
		Task.WaitAll(cacheFactionNames, cacheCharacterNames);

		System.Console.WriteLine("Starting Orbiter...");

		Task.Run(() => eventClient.OpenEventConnection(token), token);

		// var facilityControl = new SubscribeAction();
		// facilityControl.AddEvent<FacilityControlPayload>();
		// facilityControl.Worlds.Add("1");
		// eventClient.SendAction(facilityControl, token);

		var characterEvents = new SubscribeAction();
		characterEvents.Characters.AddRange(args);
		characterEvents.AddEvent<DeathPayload>();
		characterEvents.AddEvent<VehicleDestroyPayload>();
		characterEvents.AddEvent<PlayerFacilityCapturePayload>();
		characterEvents.AddEvent<PlayerFacilityDefendPayload>();
		characterEvents.AddEvent<BattleRankUpPayload>();
		eventClient.SendAction(characterEvents, token);

		System.Console.WriteLine("Listening for events...");
		
		while (true)
		{
			if (eventClient.Pump(out var response, token))
			{
				switch (response.Type)
				{
					case "serviceMessage":
					{
						if (response.Payload != null)
						{
							System.Console.WriteLine(response.Payload.GetMessage(nameCache));
						}

						break;
					}
					
				}
			}

			if (token.IsCancellationRequested)
			{
				break;
			}
		}
	}
}