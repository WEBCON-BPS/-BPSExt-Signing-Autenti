using System;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.Status
{
    public class CheckDocStatusAction : CustomAction<CheckDocStatusActionConfig>
    {
        public override void Run(RunCustomActionParams args)
        {
            var log = new StringBuilder();
            try
            {
                var docId = args.Context.CurrentDocument.GetFieldValue(Configuration.DokFildId)?.ToString();
                var status = new AutentiHelper(log).GetDocumentStatus(Configuration.ApiConfig.TokenValue, docId);
               
                args.Context.CurrentDocument.SetFieldValue(Configuration.StatusFildId, status);            
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
    }
}