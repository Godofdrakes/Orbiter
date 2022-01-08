
using LibOrbiter.Exceptions;
using LibOrbiter.Model.PS2V2;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace LibOrbiter
{
    public class OrbiterClientOld
    {
        private readonly RestClient _restClient;
        
        public string ApiVersion => "ps2:v2";

        public string ServiceId { get; }

        public OrbiterClientOld(string? serviceId = default)
        {
            ServiceId = serviceId ?? "example";

            _restClient = new RestClient($"http://census.daybreakgames.com/s:{ServiceId}");
            _restClient.UseSerializer<JsonNetSerializer>();
        }

        public async Task<CharacterList> GetCharactersByIdAsync(string[] characterIds, CancellationToken token = default)
        {
            var uri = new Uri($"get/{ApiVersion}/character/", UriKind.Relative);
            var request = new RestRequest(uri, Method.GET, DataFormat.Json);
            request.AddQueryParameter("character_id", string.Join(',', characterIds));
            return await _restClient.GetAsync<CharacterList>(request, token);
        }

        public async Task<T> GetAsync<T>(string resource, CancellationToken token = default, params KeyValuePair<string, string>[] queryParams)
        {
            var uri = new Uri($"get/{ApiVersion}/{resource}", UriKind.Relative);
            var request = new RestRequest(uri, Method.GET, DataFormat.Json);
            foreach (var (name, value) in queryParams) request.AddQueryParameter(name, value);
            return await _restClient.GetAsync<T>(request, token);
        }
    }
}
