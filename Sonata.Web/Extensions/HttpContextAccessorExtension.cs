#region Namespace Sonata.Web.Extensions
//	TODO
#endregion

using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Sonata.Web.Extensions
{
    public static class HttpContextAccessorExtension
    {
        public static string GetBearer(this IHttpContextAccessor instance)
        {
            if (instance.HttpContext?.Request == null)
            {
                throw new InvalidOperationException($"Either {nameof(instance.HttpContext)} or {instance.HttpContext.Request} is not defined");
            }

            return instance.HttpContext.Request.GetBearer();
        }

        public static string GetClaimFromBearerToken(this IHttpContextAccessor instance, string claimsType)
        {
            if (instance.HttpContext?.Request == null)
            {
                throw new InvalidOperationException($"Either {nameof(instance.HttpContext)} or {instance.HttpContext.Request} is not defined");
            }

            return instance.HttpContext.Request.GetClaimFromBearerToken(claimsType);
        }

        public static async Task<string> ReadBodyAsStringAsync(this IHttpContextAccessor instance)
        {
            if (instance.HttpContext?.Request == null)
            {
                throw new InvalidOperationException($"Either {nameof(instance.HttpContext)} or {instance.HttpContext.Request} is not defined");
            }

            return await instance.HttpContext.Request.ReadBodyAsStringAsync();
        }

        public static string GetFirstOrDefaultHeaderValue(this IHttpContextAccessor instance, string headerKey)
        {
            if (instance.HttpContext?.Request == null)
            {
                throw new InvalidOperationException($"Either {nameof(instance.HttpContext)} or {instance.HttpContext.Request} is not defined");
            }

            return instance.HttpContext.Request.GetFirstOrDefaultHeaderValue(headerKey);
        }
    }
}
