using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
using WebCon.WorkFlow.SDK.Documents.Model.ItemLists;
using WebCon.WorkFlow.SDK.Tools.Data;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv1.SendEnvelope
{
    public class SendEnvelopeAction : CustomAction<SendEnvelopeActionConfig>
    {
        StringBuilder _log = new StringBuilder();

        public override async Task RunAsync(RunCustomActionParams args)
        {
            try
            {
                var att = await GetAttachmentAsync(args.Context);
                var users = PrepareUsersToSign(args.Context.CurrentDocument.ItemsLists.GetByID(Configuration.Users.SignersList.ItemListId));
                var api = new V01Helper(Configuration.ApiConfig.Url, _log);
                var docId = await api.SendEnvelopeAsync(Configuration, users, att);

                await args.Context.CurrentDocument.SetFieldValueAsync(Configuration.AttConfig.DokumentIdFild, docId);
                await args.Context.CurrentDocument.SetFieldValueAsync(Configuration.AttConfig.AttTechnicalFieldID, att.ID);
            }
            catch (Exception e)
            {
                args.HasErrors = true;
                args.Message = e.Message;
                _log.AppendLine(e.ToString());
            }
            finally
            {
                args.LogMessage = _log.ToString();
                args.Context.PluginLogger?.AppendInfo(_log.ToString());
            }
        }

        private async Task<AttachmentData> GetAttachmentAsync(ActionContextInfo context)
        {
            if (Configuration.AttConfig.InputAttType == InputType.Category)
            {
                _log.AppendLine("Downloading attachments by category");

                var allAttachments = await new DocumentAttachmentsManager(context).GetAttachmentsAsync(new GetAttachmentsParams()
                {
                    DocumentId = context.CurrentDocument.ID,
                    IncludeContent = true
                });

                if (Configuration.AttConfig.CatType == CategoryType.ID)
                {
                    return allAttachments.FirstOrDefault(x =>
                    x.FileGroup.ID == Configuration.AttConfig.InputCategoryId.ToString()
                    && (string.IsNullOrEmpty(Configuration.AttConfig.AttRegularExpression) || Regex.IsMatch(x.FileName, Configuration.AttConfig.AttRegularExpression)));
                }
                else if (Configuration.AttConfig.CatType == CategoryType.None)
                {
                    return allAttachments.FirstOrDefault(x =>
                    x.FileGroup == null
                    && (string.IsNullOrEmpty(Configuration.AttConfig.AttRegularExpression) || Regex.IsMatch(x.FileName, Configuration.AttConfig.AttRegularExpression)));
                }
                else
                {
                    return allAttachments.First();
                }
            }
            else
            {
                _log.AppendLine("Downloading attachments by SQL query");

                var attId = await new SqlExecutionHelper(context).ExecSqlCommandScalarAsync(Configuration.AttConfig.AttQuery);
                if (attId == null)
                    throw new Exception("Sql query not returning result");

                return await new DocumentAttachmentsManager(context).GetAttachmentAsync(Convert.ToInt32(attId));
            }
        }

        private List<Models.Signer> PrepareUsersToSign(ItemsList itemsList)
        {
            if (itemsList.Rows?.Count <= 0)
                throw new Exception("Empty signers list");

            var users = new List<Models.Signer>();
            foreach(var row in itemsList.Rows)
            {
                var user = new Models.Signer();
                user.email = row.GetCellValue(Configuration.Users.SignersList.SignerMailColumnID).ToString();
                user.signatureType = Configuration.Users.Type.ToString();
                user.personalData = new Models.Personaldata1()
                {
                    givenName = row.GetCellValue(Configuration.Users.SignersList.SignerNameColumnID).ToString().Split(' ').First(),
                    surname = row.GetCellValue(Configuration.Users.SignersList.SignerNameColumnID).ToString().Split(' ').Last(),
                };
                if (Configuration.Users.PhoneAutorization)
                    user.smsAuthorization = new Models.Smsauthorization() { phoneNumber = row.GetCellValue(Configuration.Users.SignersList.SignerPhoneNumberColumnID).ToString() };

                users.Add(user);
            }

            return users;
        }
    }
}