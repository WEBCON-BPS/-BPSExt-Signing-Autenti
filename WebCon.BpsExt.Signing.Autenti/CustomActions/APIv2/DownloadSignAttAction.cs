using System;
using System.Threading.Tasks;
using WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Config;
using WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2
{
    public class DownloadSignAttAction : CustomAction<DownloadSignAttActionConfig>
    {
        public override async Task RunAsync(RunCustomActionParams args)
        {
            try
            {
                var docGuid = args.Context.CurrentDocument.GetFieldValue(Configuration.DocTechnicalFieldID).ToString();
                var apiHelper = await V2Helper.CreateAsync(args.Context, Configuration.Auth);
                await apiHelper.GetFileAndSaveStatusAsync(docGuid, Configuration.SaveCategory, Configuration.StatusFildId);
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