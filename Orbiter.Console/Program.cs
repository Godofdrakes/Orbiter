
using LibOrbiter;
using Newtonsoft.Json;

namespace Orbiter.Console;

static class Program
{
	static async Task Main(string[] args)
	{
		using var tokenSource = new CancellationTokenSource();

		var buffer = new byte[2048];

		System.Console.CancelKeyPress += (sender, eventArgs) => { tokenSource.Cancel(); };

		using var orbiter = new OrbiterEventClient();
		
#if DEBUG
		orbiter.OnJsonReceived += json =>
		{
			using var stringReader = new StringReader(json);
			using var stringWriter = new StringWriter();

			var jsonReader = new JsonTextReader(stringReader);
			var jsonWriter = new JsonTextWriter(stringWriter) {Formatting = Formatting.Indented};

			jsonWriter.WriteToken(jsonReader);

			System.Console.WriteLine(stringWriter.ToString());
		};
#endif

		while (!tokenSource.IsCancellationRequested)
		{
			System.Console.WriteLine("Connecting...");

			await orbiter.Connect(tokenSource.Token);

			var characterDeath = new OrbiterSubscribeAction();
			characterDeath.EventNames.Add("Death");
			characterDeath.Characters.AddRange(args);

			System.Console.WriteLine("Subscribing...");
			
			await orbiter.Send(characterDeath, tokenSource.Token);
			
			System.Console.WriteLine("Listening for death events...");
		
			while (!tokenSource.IsCancellationRequested)
			{
				var response = await orbiter.Receive(buffer, tokenSource.Token);
				if (response == null)
					break;
			
				System.Console.WriteLine($"Boop ({response.Type})");
			}
		}
		
		System.Console.WriteLine("Exiting...");
	}
}