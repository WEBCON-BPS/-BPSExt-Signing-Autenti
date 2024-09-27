using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv1.Download
{
    public class DownloadSignDocAction : CustomAction<DownloadSignDocActionConfig>
    {
        private const string SuccessStatus = "SUCCESS";

        public override async Task RunAsync(RunCustomActionParams args)
        {
            var log = new StringBuilder();
            try
            {
                var status = args.Context.CurrentDocument.GetFieldValue(Configuration.StatusFildId)?.ToString();
                if (!string.IsNullOrEmpty(status) && status.Equals(SuccessStatus, StringComparison.InvariantCultureIgnoreCase))
                {
                    var docId = args.Context.CurrentDocument.GetFieldValue(Configuration.DokFildId)?.ToString();
                    var api = new V01Helper(Configuration.ApiConfig.Url, log);
                    var responseContent = await api.GetDocumentAsync(Configuration.ApiConfig.TokenValue, docId);
                    await SaveAttAsync(args.Context, responseContent);
                }
                else
                {
                    args.HasErrors = true;
                    args.Message = "Document cannot be a saved if it is not signed";
                }
            }
            catch (Exception e)
            {
                args.HasErrors = true;
                args.Message = e.Message;
                log.AppendLine(e.ToString());
            }
            finally
            {
                args.LogMessage = log.ToString();
                args.Context.PluginLogger?.AppendInfo(log.ToString());
            }
        }        

        private async Task SaveAttAsync(ActionContextInfo context, byte[] newAttContent)
        {
            var currentDocument = context.CurrentDocument;
            var sourceAttData = currentDocument.GetFieldValue(Configuration.AttConfig.AttTechnicalFieldID).ToString();
            var sourceAtt = await currentDocument.Attachments.GetByIDAsync(Convert.ToInt32(sourceAttData));
            sourceAtt.SetContent(newAttContent);
            sourceAtt.FileName = $"{Path.GetFileNameWithoutExtension(sourceAtt.FileName)}{Configuration.AttConfig.AttSufix}{sourceAtt.FileExtension}";

            if (!string.IsNullOrEmpty(Configuration.AttConfig.SaveCategory))
            {
                await sourceAtt.SetFileGroupAsync(Configuration.AttConfig.SaveCategory);
            }

            await new DocumentAttachmentsManager(context).UpdateAttachmentAsync(new UpdateAttachmentParams()
            {
                Attachment = sourceAtt
            });
        }
    }
}