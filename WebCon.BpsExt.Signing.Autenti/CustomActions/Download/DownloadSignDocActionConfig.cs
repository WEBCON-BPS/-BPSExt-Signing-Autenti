using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.Download
{
    public class DownloadSignDocActionConfig : PluginConfiguration
    {
        [ConfigGroupBox(DisplayName = "Autenti API Settings")]
        public ApiConfiguration ApiConfig { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Dokument ID", Description = "Select the text field where the Document ID was saved")]
        public int DokFildId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Status", Description = "Select the text field where the Document status was saved")]
        public int StatusFildId { get; set; }

        [ConfigGroupBox(DisplayName = "Attachment selection")]
        public AttConfig AttConfig { get; set; }      
    }

    public class ApiConfiguration
    {
        [ConfigEditableText(DisplayName = "Integration Key", IsRequired = true)]
        public string TokenValue { get; set; }
    }

    public class AttConfig
    {
        [ConfigEditableText(DisplayName = "Suffix", Description = "Suffix that will be added to the name of the downloaded file. When this field is empty then the attachment will be overwritten (if the attachment with the selected Document ID exists on the form).", DefaultText = "_sign")]
        public string AttSufix { get; set; }

        [ConfigEditableText(DisplayName = "Category", Description = "Attachment category where the signed documents will be downloaded")]
        public string SaveCategory { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Source attachment ID", Description = "Select the technical field where the source attachment ID was saved.")]
        public int AttTechnicalFieldID { get; set; }
    }
}