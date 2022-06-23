using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Tools.Data.Model;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers
{
    internal class AutentiTokenProvider
    {
        ActionContextInfo _context;
        internal AutentiTokenProvider(ActionContextInfo context)
        {
            _context = context;
        }

        internal string GetAuthToken(WebServiceConnection _connection, string grant, string scope)
        {
            var json = RequestBodyProvider.CreateAuthBody(_connection, grant, scope);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_connection.Url}/auth/token")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = client.SendAsync(request).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            _context.PluginLogger?.AppendDebug("Response: " + result);
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<APIv2.Models.Auth.ResponseBody>(result).access_token;
        }
    }
}
