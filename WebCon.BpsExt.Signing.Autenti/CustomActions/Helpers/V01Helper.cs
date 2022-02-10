using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using WebCon.BpsExt.Signing.Autenti.CustomActions.APIv1.SendEnvelope;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
using WebCon.WorkFlow.SDK.Tools.Data;
using WebCon.WorkFlow.SDK.Tools.Data.Model;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers
{
    internal class V01Helper
    {
        const string BaseTestUrl = "https://api.accept.autenti.net/api/v0.1";
        string _configUrl;
        StringBuilder _log;
       

        public V01Helper(string autentiUrl, StringBuilder log)
        {
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol |
                                                   SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            _log = log; ;
            _configUrl = autentiUrl ?? BaseTestUrl;
        }

        public string SendEnvelope(SendEnvelopeActionConfig config, List<APIv1.Models.Signer> signers, AttachmentData att)
        {
            var json = new APIv1.Models.EnvelopeRequest();
            json.title = config.MessageContent.Title;
            json.message = config.MessageContent.Msg;
            json.sender = new APIv1.Models.Sender() { type = config.MessageContent.Type.ToString() };
            json.signers = signers.ToArray();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(config.ApiConfig.TokenValue);
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_configUrl}/documents")
            {
                Content = new StringContent(JsonConvert.SerializeObject(json, Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }), Encoding.UTF8, "application/json")               
            };

            var response = client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsStringAsync().Result;
            _log.AppendLine("Response: " + result);

            var docId = JsonConvert.DeserializeObject<APIv1.Models.EnvelopeResponse>(result).documentId;

            AddDocument(config.ApiConfig.TokenValue, docId, att.Content, att.FileName);
            StartProcess(config.ApiConfig.TokenValue, docId);

            return docId;
        }

        internal byte[] GetDocument(string token, string docId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/pdf, application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_configUrl}/documents/{docId}/signed");

            var response = client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsByteArrayAsync().Result;            
        }

        public void AddDocument(string token, string docId, byte[] content, string filename)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");           
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);         
            var multiForm = new MultipartFormDataContent();

            var imageContent = new ByteArrayContent(content);         
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");

            multiForm.Add(imageContent, "files", filename);

            var response = client.PostAsync($"{_configUrl}/documents/{docId}/files", multiForm).Result;
            response.EnsureSuccessStatusCode();           
        }

        public void StartProcess(string token, string docId)
        {
            var json = new APIv1.Models.EnvelopeResponse();
            json.documentId = docId; 

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_configUrl}/signing-process")
            {
                Content = new StringContent(JsonConvert.SerializeObject(json, Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }), Encoding.UTF8, "application/json")
            };

            var response = client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();           
        }

        public string GetDocumentStatus(string token, string docId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_configUrl}/signing-process/{docId}");

            var response = client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsStringAsync().Result;
            _log.AppendLine("Response:").AppendLine(result);

            return JsonConvert.DeserializeObject<APIv1.Models.StatusResponse>(result).status;
        }       
    }
}
