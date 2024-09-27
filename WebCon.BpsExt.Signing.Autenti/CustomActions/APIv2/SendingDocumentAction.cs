using System;
using System.Threading.Tasks;
using WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Config;
using WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2
{
    public class SendingDocumentAction : CustomAction<SendingDocumentActionConfig>
    {
        public override async Task RunAsync(RunCustomActionParams args)
        {
            try
            {
                var autenti = await V2Helper.CreateAsync(args.Context, Configuration.Auth);
                var docGuid = await autenti.CreateDocumentAsync();
                await autenti.ModyfiDocumentAsync(Configuration.RequestBody, docGuid);
                await autenti.AddFileAsync(args.Context, Configuration.AttConfig.AttQuery, docGuid);
                await autenti.SendToSignAsync(Configuration.ASSERTION, docGuid);
                await args.Context.CurrentDocument.SetFieldValueAsync(Configuration.DokumentIdFild, docGuid);
            }
            catch (Exception e)
            {
                args.HasErrors = true;
                args.Message = e.Message;
                args.LogMessage = e.ToString();
                args.Context.PluginLogger?.AppendInfo(e.ToString());
            }
        }        
    }
}
