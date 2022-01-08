using LibOrbiter;
using LibOrbiter.Model;
using LibOrbiter.Model.PS2;
using LibOrbiter.Model.WorldEvents;

#if DEBUG
using Newtonsoft.Json;
#endif

namespace Orbiter.Console;

static class Program
{
	static void Main(string[] args)
	{
		var tokenSource = new CancellationTokenSource();

		var token = tokenSource.Token;

		System.Console.CancelKeyPress += (_, eventArgs) =>
		{
			eventArgs.Cancel = true;

			if (!tokenSource.IsCancellationRequested)
			{
				tokenSource.Cancel();
				tokenSource.Dispose();
			}
		};

		var orbiter = new OrbiterClient();
		
#if DEBUG
		orbiter.OnEventReceived += json =>
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

		{
			var numZones = nameCache.CacheZoneNames(orbiter);
			var numRegions = nameCache.CacheRegionNames(orbiter);
			var numFactions = nameCache.CacheFactionNames(orbiter);
			var numCharacters = nameCache.CacheCharacterNames(orbiter, args);

			System.Console.WriteLine($"zones     : {numZones}");
			System.Console.WriteLine($"regions   : {numRegions}");
			System.Console.WriteLine($"factions  : {numFactions}");
			System.Console.WriteLine($"characters: {numCharacters}");
		}
		
		System.Console.WriteLine("Starting Orbiter...");

		Task.Run(() => orbiter.OpenEventConnection(token), token);

		var subscribeAction = new SubscribeAction();
		subscribeAction.Worlds.Add("all");
		subscribeAction.AddEvent<ContinentLockPayload>();
		subscribeAction.AddEvent<ContinentUnlockPayload>();
		subscribeAction.AddEvent<FacilityControlPayload>();
		subscribeAction.AddEvent<MetagameEventPayload>();

		orbiter.SendAction(subscribeAction, token);

		System.Console.WriteLine("Listening for events...");
		
		while (true)
		{
			if (orbiter.Pump(out var response, token))
			{
				switch (response.Type)
				{
					case "serviceMessage":
						response.Payload?.WriteMessage(System.Console.Out, nameCache);
						break;
				}
			}
			
			Thread.Sleep(100);

			if (token.IsCancellationRequested)
			{
				break;
			}
		}
	}
}