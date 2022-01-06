
using LibOrbiter.Exceptions;
using LibOrbiter.Model.PS2V2;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace LibOrbiter
{
    public class OrbiterClient
    {
        private readonly RestClient _restClient;

        public string ApiVersion => "ps2:v2";

        public string ServiceId { get; }

        public OrbiterClient(string? serviceId = default)
        {
            ServiceId = serviceId ?? "example";

            _restClient = new RestClient($"http://census.daybreakgames.com");
            _restClient.UseSerializer<JsonNetSerializer>();
        }

        public async Task<CharacterList> GetCharactersById(string[] characterIds, CancellationToken token = default)
        {
            try
            {
                var uri = new Uri($"get/{ApiVersion}/character/", UriKind.Relative);
                var request = new RestRequest(uri, Method.GET, DataFormat.Json);
                request.AddQueryParameter("character_id", string.Join(',', characterIds));
                return await _restClient.GetAsync<CharacterList>(request, token);
            }
            catch (Exception e)
            {
                throw new OrbiterException(nameof(GetCharactersById), e);
            }
        }
    }
}
