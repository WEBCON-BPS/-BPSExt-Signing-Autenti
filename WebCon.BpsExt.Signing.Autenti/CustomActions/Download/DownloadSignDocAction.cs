using System;
using System.IO;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents;
using WebCon.WorkFlow.SDK.Documents.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.Download
{
    public class DownloadSignDocAction : CustomAction<DownloadSignDocActionConfig>
    {
        private const string SuccessStatus = "SUCCESS";
        public override void Run(RunCustomActionParams args)
        {
            var log = new StringBuilder();
            try
            {
                var status = args.Context.CurrentDocument.GetFieldValue(Configuration.StatusFildId)?.ToString();
                if (!string.IsNullOrEmpty(status) && status.Equals(SuccessStatus, StringComparison.InvariantCultureIgnoreCase))
                {
                    var docId = args.Context.CurrentDocument.GetFieldValue(Configuration.DokFildId)?.ToString();
                    var api = new AutentiHelper(log);
                    var responseContent = api.GetDocument(Configuration.ApiConfig.TokenValue, docId);
                    SaveAtt(args.Context.CurrentDocument, responseContent);
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
                args.Context.PluginLogger.AppendInfo(log.ToString());
            }
        }

        private void SaveAtt(CurrentDocumentData currentDocument, byte[] newAttContent)
        {
            var sourceAttData = currentDocument.GetFieldValue(Configuration.AttConfig.AttTechnicalFieldID).ToString();
            var sourceAtt = currentDocument.Attachments.GetByID(Convert.ToInt32(sourceAttData));
            sourceAtt.Content = newAttContent;
            sourceAtt.FileName = $"{Path.GetFileNameWithoutExtension(sourceAtt.FileName)}{Configuration.AttConfig.AttSufix}{sourceAtt.FileExtension}";

            if (!string.IsNullOrEmpty(Configuration.AttConfig.SaveCategory))
            {
                sourceAtt.FileGroup = new AttachmentsGroup(Configuration.AttConfig.SaveCategory, null);
            }

            DocumentAttachmentsManager.UpdateAttachment(new UpdateAttachmentParams()
            {
                Attachment = sourceAtt
            });
        }
    }
}