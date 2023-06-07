using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
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

        internal async Task<string> GetAuthTokenAsync(WebServiceConnection _connection, string grant, string scope)
        {
            var json = RequestBodyProvider.CreateAuthBody(_connection, grant, scope);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new HttpRequestMessage(HttpMethod.Post, _connection.AuthorizationServiceUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            _context.PluginLogger?.AppendDebug("Response: " + result);
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<APIv2.Models.Auth.ResponseBody>(result)?.access_token;
        }
    }
}
