using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv01.Config
{
    public class ApiConfiguration
    {
        [ConfigEditableText(DisplayName = "Integration Key", IsRequired = true)]
        public string TokenValue { get; set; }

        [ConfigEditableText(DisplayName = "API URL", DefaultText = "https://api.accept.autenti.net/api/v0.1", Description = "BaseTestUrl: https://api.accept.autenti.net/api/v0.1", IsRequired = true)]
        public string Url { get; set; }
    }
}
