using System.Net.Http;
using WebCon.WorkFlow.SDK.Common.Model;
using WebCon.WorkFlow.SDK.Tools.Data;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.Helpers
{
    internal static class HttpClientHandlerProvider
    {
        internal static HttpClientHandler GetClientHandler(string url, bool useProxy, BaseContext context)
        {
            if (!useProxy)
                return new HttpClientHandler();

            var proxy = new ConnectionsHelper(context).GetProxy(url);
            return new HttpClientHandler()
            {
                Proxy = proxy,
            };
        }
    }
}
