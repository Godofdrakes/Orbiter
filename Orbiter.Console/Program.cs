
using LibOrbiter;

namespace Orbiter.Console;

static class Program
{
	static async Task Main(string[] args)
	{
		var orbiter = new OrbiterClient();

		var characters = await orbiter.GetCharactersById(args);

		foreach (var character in characters.List)
		{
			System.Console.WriteLine(character.Name.First);
		}
	}
}