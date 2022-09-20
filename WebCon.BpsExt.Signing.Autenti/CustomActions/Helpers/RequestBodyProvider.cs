using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Config;
using WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Models.Auth;
using WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Models.Document;
using WebCon.WorkFlow.SDK.Documents.Model;
using WebCon.WorkFlow.SDK.Documents.Model.ItemLists;
using WebCon.WorkFlow.SDK.Tools.Data.Model;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers
{
    internal static class RequestBodyProvider
    {
        internal static string CreateAuthBody(WebServiceConnection connection, string grant, string scope)
        {
            var requestBody = new RequestBody();
            requestBody.client_id = connection.ClientID;
            requestBody.client_secret = connection.ClientSecret;
            requestBody.username = connection.WebServiceUser;
            requestBody.password = connection.WebServicePassword;
            requestBody.grant_type = grant;
            requestBody.scope = scope;

            return JsonConvert.SerializeObject(requestBody, Formatting.Indented);
        }

        internal static string CreateDocumentBody(ItemsList itemList, Body configurationOfBody)
        {
            var body = new APIv2.Models.Document.ResponseBody();
            body.title = configurationOfBody.Title;
            body.description = configurationOfBody.Description;
            body.processLanguage = "pl";
            body.parties = AddParties(itemList, configurationOfBody);

            return JsonConvert.SerializeObject(body, Formatting.Indented);
        }

        private static Party[] AddParties(ItemsList itemLis, Body configurationOfBody)
        {
            var parties = new List<Party>();          
            foreach (var row in itemLis.Rows)
            {
                var userName = row.GetCellValue(configurationOfBody.Users.Name, EntityValueFormat.PairName).ToString();
                var item = new Party();
                item.party = new Party1();
                item.party.firstName = userName.Split(new string[] { " " }, StringSplitOptions.None).First();
                item.party.lastName = userName.Split(new string[] { " " }, StringSplitOptions.None).Last();
                item.party.name = userName;
                item.party.contacts = new Contact[]
                {
                    new Contact()
                    {
                        type = "CONTACT-TYPE:EMAIL",
                        attributes = new Attributes()
                        {
                        email = row.GetCellValue(configurationOfBody.Users.Email)?.ToString()
                        }
                    }
                };
                item.role = row.GetCellValue(configurationOfBody.Users.Role, EntityValueFormat.PairID)?.ToString();

                var constraints = new List<Constraint>();
                constraints.Add(new Constraint()
                {
                    classifiers = new string[] { "CONSTRAINT-UNIQUE_TYPE:SIGNATURE_TYPE" },
                    attributes = new Attributes2() { requiredClassifiers = new string[] { $"SIGNATURE_PROVIDER-SIGNATURE_TYPE:{configurationOfBody.Type}" } }
                });
                if (!string.IsNullOrEmpty(row.Cells.GetByID(configurationOfBody.Users.PhoneNumber)?.GetValue()?.ToString()))
                {
                    constraints.Add(new Constraint()
                    {
                        constrainedActions = new string[] { "ACTION:SIGNATURE_APPLICATION" },
                        classifiers = new string[] { "CONSTRAINT-UNIQUE_TYPE:PHONE_NUMBER_VERIFICATION_REQUIRED" },
                        attributes = new Attributes2() { phoneNumber = $"{row.GetCellValue(configurationOfBody.Users.PhoneNumber)}" }
                    });
                }
                item.constraints = constraints.ToArray();

                parties.Add(item);
            }

            return parties.ToArray();
        }
    }
}
