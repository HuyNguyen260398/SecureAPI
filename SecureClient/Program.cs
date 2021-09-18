using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace SecureClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Calling the api");
            RunAsyn().GetAwaiter().GetResult();
        }

        private static async Task RunAsyn()
        {
            AuthConfig config = AuthConfig.ReadJsonFromFile("appsettings.json");
            IConfidentialClientApplication app;

            app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                    .WithClientSecret(config.ClientSecret)
                    .WithAuthority(new Uri(config.Authority))
                    .Build();

            string[] ResouceIds = new string[] {config.ResourceId};
            AuthenticationResult result = null;

            try
            {
                 result = await app.AcquireTokenForClient(ResouceIds).ExecuteAsync();
                 Console.ForegroundColor = ConsoleColor.Green;
                 Console.WriteLine("Token acquired \n");
                 Console.WriteLine(result.AccessToken);
                 Console.ResetColor();
            }
            catch (MsalClientException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }

            if (!String.IsNullOrEmpty(result.AccessToken))
            {
                // By pass SSL certificate
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                var httpClient = new HttpClient(clientHandler);
                var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

                if (defaultRequestHeaders.Accept == null || 
                !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json")
                    );
                }

                defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);

                HttpResponseMessage response = await httpClient.GetAsync(config.BaseAddress);

                if (response.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to call api: {response.StatusCode}");
                    string content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Content: {content}");
                }
                Console.ResetColor();
            }
        }
    }
}
