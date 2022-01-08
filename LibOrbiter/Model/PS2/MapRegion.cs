using Newtonsoft.Json;

namespace LibOrbiter.Model.PS2;

[JsonObject]
public struct MapRegion
{
	public long MapRegionId;
	public long FacilityId;
	public long FacilityTypeId;

	public string FacilityName;
	public string FacilityType;
}