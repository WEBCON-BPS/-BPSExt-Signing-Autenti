using System;
using System.Text;
using System.Threading.Tasks;
using WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv1.Status
{
    public class CheckDocStatusAction : CustomAction<CheckDocStatusActionConfig>
    {
        public override async Task RunAsync(RunCustomActionParams args)
        {
            var log = new StringBuilder();
            try
            {
                var docId = args.Context.CurrentDocument.GetFieldValue(Configuration.DokFildId)?.ToString();
                var status = await new V01Helper(Configuration.ApiConfig.Url, log).GetDocumentStatusAsync(Configuration.ApiConfig.TokenValue, docId);

                await args.Context.CurrentDocument.SetFieldValueAsync(Configuration.StatusFildId, status);
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
    }
}