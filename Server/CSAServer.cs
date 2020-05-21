using CSACore.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace CSACore.Server {

    public class CSAServer {
        //================================================================================
        public const int                        TIMEOUT_SECONDS = 30;
        public const string                     SERVER_URL = "https://localhost:44352/";


        // LICENCING ================================================================================
        //--------------------------------------------------------------------------------
        internal async Task<bool> ValidateLicenceAsync(string licenceID) {
            // HTTP client
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(SERVER_URL);
            httpClient.Timeout = new TimeSpan(0, 0, TIMEOUT_SECONDS);

            // Content
            FormUrlEncodedContent content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("id", licenceID)
            });

            // Request
            try {
                // Parameters
                string parameters = $"?id={licenceID}&ipAddress={UNetwork.WebIP}&deviceName={Environment.MachineName}&systemUsername={Environment.UserName}";

                // Post
                HttpResponseMessage response = await httpClient.PostAsync($"api/Licence/Validate{parameters}", content);

                // Response
                if (response.IsSuccessStatusCode) {
                    string responseString = await response.Content.ReadAsStringAsync();
                    return UConvert.FromString<bool>(responseString, true);
                }
                else
                    Debug.WriteLine($"Failed to validate licence: {(int)response.StatusCode} - {response.StatusCode} ({response.ReasonPhrase})");
            }
            catch (Exception e) {
                Debug.WriteLine($"Failed to connect to CSA server: {e.Message}");
            }

            // No response
            return true; // Assume licence is valid
        }
    }

}
