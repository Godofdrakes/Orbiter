using System.Collections.Concurrent;
using LibOrbiter;
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

		var characterDeath = new OrbiterSubscribeAction();
		characterDeath.EventNames.Add("Death");
		characterDeath.Characters.Add("all");
		characterDeath.Worlds.Add("1");
		orbiter.Send(characterDeath, token);

		System.Console.WriteLine("Listening for events...");

		while (!token.IsCancellationRequested)
		{
			if (orbiter.Pump(out var response, token))
			{
				System.Console.WriteLine($"Event received ({response.Type})");

				switch (response.Type)
				{
					case "serviceMessage":
					{
						switch (response.Payload)
						{
							case OrbiterDeathPayload deathPayload:
								System.Console.WriteLine($"[{deathPayload.Timestamp}] {deathPayload.AttackerCharacterId} killed {deathPayload.CharacterId}{(deathPayload.IsHeadshot ? " (headshot)" : "")}");
								break;
						}

						break;
					}
				}
			}
		}
	}
}