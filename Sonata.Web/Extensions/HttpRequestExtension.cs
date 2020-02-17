#region Namespace Sonata.Web.Extensions
//	TODO
#endregion

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sonata.Web.Extensions
{
    public static class HttpRequestExtension
    {
        public static string GetBearer(this HttpRequest instance)
        {
            WebProvider.Trace($"Call to {nameof(GetBearer)}.");

            if (instance == null)
                return null;

            if (instance.Headers == null)
            {
                WebProvider.Trace("		No headers found in the HTTP request.");
                return null;
            }

            if (String.IsNullOrEmpty(instance.Headers["Authorization"]))
            {
                WebProvider.Trace("		No Authorization header provided.");
                return null;
            }

            if (!instance.Headers["Authorization"].ToString().Trim().StartsWith("Bearer"))
            {
                WebProvider.Trace("		No Bearer token defined.");
                return null;
            }

            return instance.Headers["Authorization"].ToString().Replace("Bearer", String.Empty).Trim();
        }

        public static string GetClaimFromBearerToken(this HttpRequest instance, string claimsType)
        {
            if (claimsType == null)
            {
                throw new ArgumentNullException(nameof(claimsType));
            }

            var encodedToken = instance.GetBearer();
            if (encodedToken == null)
            {
                return null;
            }

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            if (!jwtSecurityTokenHandler.CanReadToken(encodedToken))
            {
                return null;
            }

            var decodedToken = new JwtSecurityToken(encodedToken);
            var claimsValue = decodedToken.Claims.Where(c => c.Type == claimsType).FirstOrDefault()?.Value;

            return claimsValue;
        }

        public static async Task<string> ReadBodyAsStringAsync(this HttpRequest instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            instance.EnableBuffering();
            instance.Body.Position = 0;
            instance.Body.Seek(0, SeekOrigin.Begin);

            var streamReader = new StreamReader(instance.Body);
            var bodyAsText = await streamReader.ReadToEndAsync();

            instance.EnableBuffering();
            instance.Body.Position = 0;
            instance.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }

        public static string GetFirstOrDefaultHeaderValue(this HttpRequest instance, string headerKey)
        {
            if (instance.Headers == null
                || !instance.Headers.ContainsKey(headerKey)
                || (StringValues?)instance.Headers[headerKey] == (StringValues?)null)
            {
                return null;
            }

            return instance.Headers[headerKey].FirstOrDefault();
        }
    }
}
