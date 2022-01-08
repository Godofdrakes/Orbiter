using System.Collections;
using Newtonsoft.Json;

namespace LibOrbiter.Model.PS2;

[JsonObject]
public abstract class ResponseList
{
	public int Returned { get; set; }
}