using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Config
{
    public class DownloadSignAttActionConfig : PluginConfiguration
    {
        [ConfigGroupBox(DisplayName = "Authenticate")]
        public Authenticate Auth { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Returned id of document", Description = "Specify a text field on the form where returned document ID was save")]
        public int DocTechnicalFieldID { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Copy Status to field", Description = "Specify a field on the form where current document status will be saved")]
        public int StatusFildId { get; set; }

        [ConfigEditableText(DisplayName = "Category", Description = "Attachment category where the signed documents will be downloaded")]
        public string SaveCategory { get; set; }
    }
}