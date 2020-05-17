using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using NettedAzure.Config;
using Newtonsoft.Json.Linq;

namespace NettedAzure
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var confLoader = new ConfigLoader();
            var config = confLoader.LoadConfig();
            var token = await GetAccessToken(config.TenantId, config.PrincipalId, config.PrincipalKey);

            RegisterProviderIfNotRegistered(config.SubscriptionId, token);
            Console.ReadLine();
        }

        private static void RegisterProviderIfNotRegistered(string subscriptionId, string token)
        {
            var response = JObject.Parse(GetSubscriptionResourceProvider(subscriptionId, token).Result.Content
                .ReadAsStringAsync().Result);
            var registrationState = response.SelectToken(Constants.RegistrationState);
            if (registrationState.Value<string>() != Constants.NotRegistered &&
                registrationState.Value<string>() != Constants.UnRegistered)
            {
                return;
            }

            response = JObject.Parse(RegisterResourceProviderAsync(subscriptionId, token).Result.Content
                .ReadAsStringAsync().Result);
            registrationState = response.SelectToken(Constants.RegistrationState);
        }

        private static async Task<string> GetAccessToken(string tenantId, string clientId, string clientSecret)
        {
            var authContextURL = "https://login.windows.net/" + tenantId;
            var authenticationContext = new AuthenticationContext(authContextURL);
            var credential = new ClientCredential(clientId, clientSecret);
            var result = await authenticationContext.AcquireTokenAsync(resource: "https://management.azure.com/", clientCredential: credential);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            var token = result.AccessToken;
            return token;
        }

        private static async Task<HttpResponseMessage> GetSubscriptionResourceProvider(string subscriptionId, string token)
        {
            var uri =
                $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.OperationalInsights?api-version=2019-10-01";

            return await GetWebResponseAsync(uri, token, "GET");
        }
        private static async Task<HttpResponseMessage> RegisterResourceProviderAsync(string subscriptionId, string token)
        {
            var uri =
                $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.OperationalInsights/register?api-version=2019-10-01";

            return await GetWebResponseAsync(uri, token, "POST");
        }

        private static async  Task<HttpResponseMessage> GetWebResponseAsync(string uri, string token, string method)
        {
            var req = new HttpClient();
            req.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            switch (method)
            {
                case "POST":
                    return await req.PostAsync(uri, new StringContent("", Encoding.UTF8, "application/json"));
                case "GET":
                    return await req.GetAsync(uri);
                default:
                    throw new Exception("Unknow web method");
            }
        }
    }
}
