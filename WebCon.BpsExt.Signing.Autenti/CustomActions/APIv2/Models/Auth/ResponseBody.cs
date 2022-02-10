namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Models.Auth
{
    public class ResponseBody
    {
        public string scope { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }

}
