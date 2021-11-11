using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LoL_Summoner_Spells
{
    class LCUEndpoints
    {
        private static readonly LCUConnector lcu = new LCUConnector();
        private readonly string URL;
        private readonly string LOGIN;
        private readonly string PASS;

        public string Region { get; }

        public LCUEndpoints()
        {
            LOGIN = lcu.LOGIN;
            PASS = lcu.PASS;
            URL = lcu.URL;
            Region = lcu.Region;
        }

        static readonly HttpClient client = new HttpClient();

        public async Task<String> Request(HttpMethod method, string endpoint, string query = "", string data = "")
        {
            HttpRequestMessage request;
            HttpResponseMessage response = new HttpResponseMessage();

            // If it has query
            if (query.Length > 0)
                request = new HttpRequestMessage(method, URL + endpoint + "?" + query);
            else
                request = new HttpRequestMessage(method, URL + endpoint);

            // Encode USER:PASS for the HTTP Basic
            string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(LOGIN + ":" + PASS));

            // Skip SSL
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            // Headers
            request.Headers.Add("Authorization", "Basic " + svcCredentials);

            if (data.Length > 0)
                request.Content = new StringContent(data, Encoding.UTF8, "application/json");

            // Get response and return
            response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> getSession() => await Request(HttpMethod.Get, "/lol-summoner/v1/current-summoner");
    }
}
