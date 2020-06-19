using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args) 
        {
            // discover endpoints from metadata

           var t = Task<int>.Run(() => MainAsync());
            t.Wait();
        }

        static async Task<int> MainAsync()
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:44332");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
            }
            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return 1;
            }

            Console.WriteLine(tokenResponse.Json);

            // call api
            var client2 = new HttpClient();
            client2.SetBearerToken(tokenResponse.AccessToken);

            var response = await client2.GetAsync("https://localhost:44383/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }

            return 1;
        }
    }
}
