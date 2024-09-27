using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Config;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
using WebCon.WorkFlow.SDK.Tools.Data;
using WebCon.WorkFlow.SDK.Tools.Data.Model;
using WebCon.WorkFlow.SDK.Tools.Other;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers
{
    internal class V2Helper
    {
        const string Accept = "application/json";
        private ActionContextInfo _context;
        string _apiUrl;
        string _token;

        public static async Task<V2Helper> CreateAsync(ActionContextInfo context, Authenticate auth)
        {
            var connection = new ConnectionsHelper(context).GetConnectionToWebService(new GetByConnectionParams(auth.WsConId));
            var token = await new AutentiTokenProvider(context).GetAuthTokenAsync(connection, auth.GrantType, auth.Scope);
            return new V2Helper(context, connection.Url, token);
        }

        private V2Helper(ActionContextInfo context,string apiUrl, string token)
        {
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol |
                                                  SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            _context = context;
            _apiUrl= apiUrl?.TrimEnd('/');
            _token = token;
                     
        }

        internal async Task<string> CreateDocumentAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", Accept);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/document-processes");

            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            _context.PluginLogger?.AppendDebug("CreateDocumentAsync Response: " + result);
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<APIv2.Models.Document.ResponseBody>(result)?.id;
        }

        internal async Task ModyfiDocumentAsync(Body requestBody, string docGuid)
        {
            var userList = _context.CurrentDocument.ItemsLists.GetByID(requestBody.Users.ItemListId);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", Accept);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
            var content = RequestBodyProvider.CreateDocumentBody(userList, requestBody);

            var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiUrl}/document-processes/{docGuid}")
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            _context.PluginLogger?.AppendDebug("ModyfiDocumentAsync Request body: " + content);
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            _context.PluginLogger?.AppendDebug("ModyfiDocumentAsync Response: " + result);
            response.EnsureSuccessStatusCode();
        }      

        internal async Task AddFileAsync(ActionContextInfo context, string attQuery, string docGuid)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", Accept);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");

            var dt = await new SqlExecutionHelper(context).GetDataTableForSqlCommandAsync(attQuery);
            if (dt.Rows.Count == 0)
                throw new Exception("Empty attachments list. Please attach at least one file.");

            foreach (System.Data.DataRow row in dt.Rows)
            {
                var att = await new DocumentAttachmentsManager(context).GetAttachmentAsync(Convert.ToInt32(row[0]));
                var content = await att.GetContentAsync();
                var imageContent = new ByteArrayContent(content);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

                var multiForm = new MultipartFormDataContent();
                multiForm.Add(imageContent, "file", att.FileName);

                var response = await client.PostAsync($"{_apiUrl}/document-processes/{docGuid}/files", multiForm);
                var result = response.Content.ReadAsStringAsync();
                _context.PluginLogger?.AppendDebug("AddFileAsync Response: " + result);
                response.EnsureSuccessStatusCode();
            }
        }

        internal async Task SendToSignAsync(string xAssertion, string docGuid)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", Accept);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
            client.DefaultRequestHeaders.Add("X-ASSERTION", Convert.ToBase64String(Encoding.UTF8.GetBytes(xAssertion)));

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}/document-processes/{docGuid}/actions");

            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            _context.PluginLogger?.AppendDebug("SendToSignAsync Response: " + result);
            response.EnsureSuccessStatusCode();
        }       

        internal async Task GetFileAndSaveStatusAsync(string docGuid, string attachmentCategory, int statusFieldId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/pdf, application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/document-processes/{docGuid}");

            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            _context.PluginLogger?.AppendDebug("GetFileAndSaveStatusAsync Response: " + result);
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

                    var request2 = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}/document-processes/{docGuid}/files/{HttpUtility.UrlEncode(file.id)}/content");

                    var response2 = await client.SendAsync(request2);
                    response2.EnsureSuccessStatusCode();
                    var content = await response2.Content.ReadAsByteArrayAsync();
                    await AddAttToDocAsync(file.filename, content, attachmentCategory);
                }
            }

            await _context.CurrentDocument.SetFieldValueAsync(statusFieldId, document.status);        
        }

        private async Task AddAttToDocAsync(string fileName, byte[] content, string category)
        {
            var newAtt = await new DocumentAttachmentsManager(_context).GetNewAttachmentAsync(fileName, content);
            if (!string.IsNullOrEmpty(category))
                await SetFileGroup(newAtt, category);
            
            await _context.CurrentDocument.Attachments.AddNewAsync(newAtt);
        }

        private async Task SetFileGroup(NewAttachmentData newAtt, string category)
        {
            if (category.Contains("#"))
            {
                newAtt.FileGroup = new AttachmentsGroup(TextHelper.GetPairId(category), TextHelper.GetPairName(category));
                return;
            }

            var fileGroup = await newAtt.ResolveAsync(category);
            newAtt.FileGroup = fileGroup ?? new AttachmentsGroup(category);
        }
    }
}
