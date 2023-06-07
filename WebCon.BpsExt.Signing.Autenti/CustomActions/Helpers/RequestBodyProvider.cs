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
        private const string Qualified = "QUALIFIED";
        private const string Basic = "BASIC";
        private const string SignerRole = "SIGNER";

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
            body.parties = AddParties(itemList, configurationOfBody.Users, configurationOfBody.SetUserPriority);

            return JsonConvert.SerializeObject(body, Formatting.Indented);
        }

        private static Party[] AddParties(ItemsList itemLis, UserColumns userData, bool setUserPriority)
        {
            var parties = new List<Party>();
            int rowIndex = 1;
            foreach (var row in itemLis.Rows)
            {
                var userName = row.GetCellValue(userData.Name, EntityValueFormat.PairName).ToString();
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
                        email = row.GetCellValue(userData.Email)?.ToString()
                        }
                    }
                };
                item.role = row.GetCellValue(userData.Role, EntityValueFormat.PairID)?.ToString();

                var constraints = new List<Constraint>();
                if (item.role == SignerRole)
                {
                    var signType = (APIv1.SendEnvelope.SignType)Convert.ToInt32(row.GetCellValue(userData.SigType, EntityValueFormat.PairID));
                    AddSignTypeConstraints(constraints, signType);
                    
                    if(signType == APIv1.SendEnvelope.SignType.ESIGNATURE)
                        AddSmsAuthenticationnConstraints(constraints, row, userData);
                }
                AddSmsAccessConstraints(constraints, row, userData);

                if (setUserPriority)
                    AddPriorityConstraints(constraints ,rowIndex++);                   

                item.constraints = constraints.ToArray();

                parties.Add(item);
            }

            return parties.ToArray();
        }

        private static void AddPriorityConstraints(List<Constraint> constraints, int rowIndex)
        {
            constraints.Add(new Constraint()
            {
                classifiers = new string[] { "CONSTRAINT-UNIQUE_TYPE:PARTICIPATION_PRIORITY" },
                attributes = new Attributes2() { priority = rowIndex }
            });
        }

        private static void AddSignTypeConstraints(List<Constraint> constraints, APIv1.SendEnvelope.SignType signType)
        {
            constraints.Add(new Constraint()
            {
                classifiers = new string[] { "CONSTRAINT-UNIQUE_TYPE:SIGNATURE_TYPE" },
                attributes = new Attributes2() { requiredClassifiers = new string[] { $"SIGNATURE_PROVIDER-SIGNATURE_TYPE:{(signType == APIv1.SendEnvelope.SignType.ESIGNATURE ? Basic : Qualified)}" } }
            });
        }

        private static void AddSmsAuthenticationnConstraints(List<Constraint> constraints, ItemRowData row, UserColumns userData)
        {
            string phoneNumber;
            bool? smsAuth = null;

            if (userData.SmsAuthentication.HasValue)
                smsAuth = row.BooleanCells.GetByID(userData.SmsAuthentication.Value).Value;

            if (smsAuth.HasValue && smsAuth.Value)
            {
                phoneNumber = row.Cells.GetByID(userData.PhoneNumber)?.GetValue()?.ToString();
                if (string.IsNullOrEmpty(phoneNumber))
                    throw new Exception("Phone number is empty. You have to provide value for required field.");
            }

            if (smsAuth.HasValue && smsAuth.Value)
            {
                constraints.Add(new Constraint()
                {
                    constrainedActions = new string[] { "ACTION:SIGNATURE_APPLICATION" },
                    classifiers = new string[] { "CONSTRAINT-UNIQUE_TYPE:PHONE_NUMBER_VERIFICATION_REQUIRED" },
                    attributes = new Attributes2() { phoneNumber = $"{row.GetCellValue(userData.PhoneNumber)}" }
                });
            }
        }

        private static void AddSmsAccessConstraints(List<Constraint> constraints, ItemRowData row, UserColumns userData)
        {
            string phoneNumber;
            bool? smsAccess = null;           

            if (userData.SmsAccess.HasValue)
                smsAccess = row.BooleanCells.GetByID(userData.SmsAccess.Value).Value;

            if (smsAccess.HasValue && smsAccess.Value)
            {
                phoneNumber = row.Cells.GetByID(userData.PhoneNumber)?.GetValue()?.ToString();
                if (string.IsNullOrEmpty(phoneNumber))
                    throw new Exception("Phone number is empty. You have to provide value for required field.");
            }           

            if (smsAccess.HasValue && smsAccess.Value)
            {
                constraints.Add(new Constraint()
                {
                    constrainedActions = new string[] { "ACTION:READ_SENSITIVE_METADATA" },
                    classifiers = new string[] { "CONSTRAINT-UNIQUE_TYPE:PHONE_NUMBER_VERIFICATION_REQUIRED" },
                    attributes = new Attributes2() { phoneNumber = $"{row.GetCellValue(userData.PhoneNumber)}" }
                });
            }
        }
    }
}
