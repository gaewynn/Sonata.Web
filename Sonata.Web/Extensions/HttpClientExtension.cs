#region Namespace Sonata.Web.Extensions
//	TODO
#endregion

using System;
using System.Net.Http;

namespace Sonata.Web.Extensions
{
    public static class HttpClientExtension
    {
        /// <summary>
        /// Updates a default request header value on the specified <paramref name="instance"/>.
        /// First, remove the header matching the specified <paramref name="headerName"/> and add it again with the new specified <paramref name="headerValue"/>
        /// </summary>
        /// <param name="instance">The instance of the <see cref="HttpClient"/> on which update the header value</param>
        /// <param name="headerName">The name of the header to update.</param>
        /// <param name="headerValue">The value of the header to update.</param>
        public static void UpdateDefaultRequestHeader(this HttpClient instance, string headerName, string headerValue)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            instance.DefaultRequestHeaders.Remove(headerName);
            instance.DefaultRequestHeaders.Add(headerName, headerValue);
        }
    }
}
