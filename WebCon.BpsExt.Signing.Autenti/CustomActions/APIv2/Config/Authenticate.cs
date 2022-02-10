using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Config
{
    public class Authenticate
    {
        [ConfigEditableConnectionID(DisplayName = "API connection", Description = "Salesforce type", ConnectionsType = DataConnectionType.WebServiceREST)]
        public int WsConId { get; set; }

        [ConfigEditableText(DisplayName = "grant_type", DefaultText = "password")]
        public string GrantType { get; set; }

        [ConfigEditableText(DisplayName = "scope", DefaultText = "full")]
        public string Scope { get; set; }
    }
}
