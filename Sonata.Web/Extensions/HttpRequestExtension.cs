#region Namespace Sonata.Web.Extensions
//	TODO
#endregion

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System;

namespace Sonata.Web.Extensions
{
    public static class HttpRequestExtension
    {
        public static string GetBearer(this HttpRequest instance)
        {
            WebProvider.Trace($"Call to {nameof(GetBearer)}.");

            if (instance == null)
                return null;

            if (!(instance is DefaultHttpRequest httpRequest))
            {
                WebProvider.Trace("		Invalid HTTP request.");
                return null;
            }

            if (httpRequest.Headers == null)
            {
                WebProvider.Trace("		No headers found in the HTTP request.");
                return null;
            }

            if (String.IsNullOrEmpty(httpRequest.Headers["Authorization"]))
            {
                WebProvider.Trace("		No Authorization header provided.");
                return null;
            }

            if (!httpRequest.Headers["Authorization"].ToString().Trim().StartsWith("Bearer"))
            {
                WebProvider.Trace("		No Bearer token defined.");
                return null;
            }

            return httpRequest.Headers["Authorization"].ToString().Replace("Bearer", String.Empty).Trim();
        }
    }
}
