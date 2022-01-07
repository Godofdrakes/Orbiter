using System.Collections.Concurrent;
using LibOrbiter;
using LibOrbiter.Model;
using Newtonsoft.Json;

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

		System.Console.WriteLine("Starting Orbiter...");
		
		var orbiter = new OrbiterEventClient();
		
#if DEBUG
		orbiter.OnJsonReceived += json =>
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

		var backgroundTask = Task.Run(async () => await orbiter.BackgroundTask(token), token);

		var characterDeath = new SubscribeAction();
		characterDeath.AddEventRange(
			typeof(DeathPayload),
			typeof(VehicleDestroyPayload),
			typeof(PlayerLoginPayload),
			typeof(PlayerLogoutPayload),
			typeof(PlayerFacilityCapturePayload),
			typeof(PlayerFacilityDefendPayload),
			typeof(BattleRankUpPayload));
		characterDeath.Characters.AddRange(args);
		orbiter.Send(characterDeath, token);

		System.Console.WriteLine("Listening for events...");

		while (!token.IsCancellationRequested)
		{
			if (orbiter.Pump(out var response, token))
			{
				switch (response.Type)
				{
					case "serviceMessage":
					{
						if (response.Payload != null)
						{
							System.Console.WriteLine(response.Payload.GetMessage());
						}

						break;
					}
					
				}
			}
		}
	}
}