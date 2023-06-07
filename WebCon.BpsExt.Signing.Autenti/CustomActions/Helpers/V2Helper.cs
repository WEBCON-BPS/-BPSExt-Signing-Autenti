using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Config;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
using WebCon.WorkFlow.SDK.Tools.Data;
using WebCon.WorkFlow.SDK.Tools.Data.Model;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers
{
    internal class V2Helper
    {
        const string BaseTestUrl = "https://api.accept.autenti.net/api/v2";
        const string Accept = "application/json";
        private ActionContextInfo _context;
        private WebServiceConnection _connection;
        string _token;

        public V2Helper(ActionContextInfo context, Authenticate auth)
        {
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol |
                                                  SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            _context = context;
            _connection = ConnectionsHelper.GetConnectionToWebService(new GetByConnectionParams(auth.WsConId, _context));

            _token = new AutentiTokenProvider(context).GetAuthToken(_connection, auth.GrantType, auth.Scope);
        }        

        internal string CreateDocument()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", Accept);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_connection.Url}/document-processes");

            var response = client.SendAsync(request).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            _context.PluginLogger?.AppendDebug("Response: " + result);
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<APIv2.Models.Document.ResponseBody>(result)?.id;
        }

        internal void ModyfiDocument(Body requestBody, string docGuid)
        {
            var userList = _context.CurrentDocument.ItemsLists.GetByID(requestBody.Users.ItemListId);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", Accept);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");

            var request = new HttpRequestMessage(HttpMethod.Put, $"{_connection.Url}/document-processes/{docGuid}")
            {
                Content = new StringContent(RequestBodyProvider.CreateDocumentBody(userList, requestBody), Encoding.UTF8, "application/json")
            };

            var response = client.SendAsync(request).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            _context.PluginLogger?.AppendDebug("Response: " + result);
            response.EnsureSuccessStatusCode();
        }      

        internal void AddFile(ActionContextInfo context, string attQuery, string docGuid)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", Accept);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");

            var dt = SqlExecutionHelper.GetDataTableForSqlCommand(attQuery, _context);
            if (dt.Rows.Count == 0)
                throw new Exception("Empty attachments list. Please attach at least one file.");

            foreach (System.Data.DataRow row in dt.Rows)
            {
                var att = new DocumentAttachmentsManager(context).GetAttachment(Convert.ToInt32(row[0]));

                var imageContent = new ByteArrayContent(att.Content);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

                var multiForm = new MultipartFormDataContent();
                multiForm.Add(imageContent, "file", att.FileName);

                var response = client.PostAsync($"{_connection.Url}/document-processes/{docGuid}/files", multiForm).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                _context.PluginLogger?.AppendDebug("Response: " + result);
                response.EnsureSuccessStatusCode();
            }
        }

        internal void SendToSign(string xAssertion, string docGuid)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", Accept);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
            client.DefaultRequestHeaders.Add("X-ASSERTION", Convert.ToBase64String(Encoding.UTF8.GetBytes(xAssertion)));

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_connection.Url}/document-processes/{docGuid}/actions");

            var response = client.SendAsync(request).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            _context.PluginLogger?.AppendDebug("Response: " + result);
            response.EnsureSuccessStatusCode();
        }       

        internal void GetFileAndSaveStatus(string docGuid, string attachmentCategory, int statusFieldId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/pdf, application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_connection.Url}/document-processes/{docGuid}");

            var response = client.SendAsync(request).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            _context.PluginLogger?.AppendDebug("Response: " + result);
            response.EnsureSuccessStatusCode();

            var document = JsonConvert.DeserializeObject<APIv2.Models.Document.ResponseBody>(result);
            if (document.status == "COMPLETED")
            {               
                var file = document.files.AsEnumerable().FirstOrDefault(x => x.filePurpose == "SIGNED_CONTENT_FILE");
                if (file == null)
                {
                    throw new Exception("The document does not have a file!");
                }
                else
                {
                    client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Accept", "*/*");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_token);

                    var request2 = new HttpRequestMessage(HttpMethod.Get, $"{_connection.Url}/document-processes/{docGuid}/files/{HttpUtility.UrlEncode(file.id)}/content");

                    var response2 = client.SendAsync(request2).Result;
                    response2.EnsureSuccessStatusCode();
                    var content = response2.Content.ReadAsByteArrayAsync().Result;
                    AddAttToDoc(file.filename, content, attachmentCategory);
                }
            }

            _context.CurrentDocument.SetFieldValue(statusFieldId, document.status);        
        }

        private void AddAttToDoc(string fileName, byte[] content, string category)
        {
            _context.CurrentDocument.Attachments.AddNew(new NewAttachmentData(fileName, content)
            {
                FileGroup = new AttachmentsGroup(category, null)
            });
        }
    }
}
