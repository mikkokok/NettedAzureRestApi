﻿using System;
using System.IO;
using System.Net;
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
            var response = JObject.Parse(GetSubscriptionResourceProvider(config.SubscriptionId, token));
            var registrationState = response.SelectToken(Constants.RegistrationState);
            if (registrationState.Value<string>() == Constants.NotRegistered || registrationState.Value<string>() == Constants.UnRegistered)
            {
                response = JObject.Parse(RegisterResourceProvider(config.SubscriptionId, token));
                registrationState = response.SelectToken(Constants.RegistrationState);
            }
            Console.ReadLine();
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

        //GET https://management.azure.com/subscriptions/{subscriptionId}/providers/{resourceProviderNamespace}?api-version=2019-10-01
        //Microsoft.OperationalInsights
        //POST https://management.azure.com/subscriptions/{subscriptionId}/providers/{resourceProviderNamespace}/register?api-version=2019-10-01
        private static string GetSubscriptionResourceProvider(string subscriptionId, string token)
        {
            var uri =
                $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.OperationalInsights?api-version=2019-10-01";


            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            return GetWebResponse(httpWebRequest);
        }
        private static string RegisterResourceProvider(string subscriptionId, string token)
        {
            var uri =
                $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.OperationalInsights/register?api-version=2019-10-01";

            // Create the request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            //using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            //{
            //    streamWriter.Write(body);
            //    streamWriter.Flush();
            //    streamWriter.Close();
            //}

            return GetWebResponse(httpWebRequest);
        }

        private static string GetWebResponse(HttpWebRequest httpWebRequest)
        {
            var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}