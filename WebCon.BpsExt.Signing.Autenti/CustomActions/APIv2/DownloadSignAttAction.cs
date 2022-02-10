using System;
using WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Config;
using WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2
{
    public class DownloadSignAttAction : CustomAction<DownloadSignAttActionConfig>
    {
        public override void Run(RunCustomActionParams args)
        {
            try
            {
                var docGuid = args.Context.CurrentDocument.GetFieldValue(Configuration.DocTechnicalFieldID).ToString();
                new V2Helper(args.Context, Configuration.Auth).GetFileAndSaveStatus(docGuid, Configuration.SaveCategory, Configuration.StatusFildId);                           
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