using System.Text;
using System.Text.Json;

namespace KeihinLive
{
    class BackendClient
    {

        private string apiBaseUrl;
        private string machineFingerprint;

        public BackendClient(string apiBaseUrl, string machineFingerprint) {
            this.apiBaseUrl = apiBaseUrl;
            this.machineFingerprint = machineFingerprint;
        }

        public async Task<GenerateSeedKeyResponse> GenerateSeedKeyAsync(GenerateSeedKeyRequest request)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Auth-Token", machineFingerprint);
                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiBaseUrl}/api/GenerateSeedKey", jsonContent);
                string responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                GenerateSeedKeyResponse responseObject = JsonSerializer.Deserialize<GenerateSeedKeyResponse>(responseBody);
                if (responseObject == null)
                {
                    throw new InvalidOperationException("Failed to deserialize the response into GenerateSeedKeyResponse.");
                }
                return responseObject;
            }
        }
    }
}
