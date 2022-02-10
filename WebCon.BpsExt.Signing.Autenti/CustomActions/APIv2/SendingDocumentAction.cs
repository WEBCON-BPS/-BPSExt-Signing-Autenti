using System;
using WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Config;
using WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2
{
    public class SendingDocumentAction : CustomAction<SendingDocumentActionConfig>
    {
        public override void Run(RunCustomActionParams args)
        {
            try
            {
                var autenti = new V2Helper(args.Context, Configuration.Auth);
                var docGuid = autenti.CreateDocument();
                autenti.ModyfiDocument(Configuration.RequestBody, docGuid);
                autenti.AddFile(args.Context, Configuration.AttConfig.AttQuery, docGuid);
                autenti.SendToSign(Configuration.ASSERTION, docGuid);
                args.Context.CurrentDocument.SetFieldValue(Configuration.DokumentIdFild, docGuid);
            }
            catch (Exception e)
            {
                args.HasErrors = true;
                args.Message = e.Message;
                args.LogMessage = e.ToString();
                args.Context.PluginLogger.AppendInfo(e.ToString());
            }
        }
    }
}
