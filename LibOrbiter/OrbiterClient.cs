
using RestSharp;

namespace LibOrbiter
{
    public class OrbiterClient
    {
        private readonly RestClient restClient;

        public OrbiterClient(string? serviceId = default)
        {
            restClient = new($"http://census.daybreakgames.com/s:{serviceId ?? "example"}/json");
            restClient.UseSerializer<RestSharp.Serializers.NewtonsoftJson.JsonNetSerializer>();
        }
    }
}
