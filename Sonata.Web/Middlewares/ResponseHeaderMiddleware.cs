#region Namespace Sonata.Web.Middlewares
//	The Sonata.Web.Middlewares namespace contains custom middlewares.
#endregion

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sonata.Web.Middlewares
{
    public static class ResponseHeaderMiddlewareExtensions
    {
        #region Methods

        public static void ConfigureResponseHeaderMiddleware(this IApplicationBuilder app, ResponseHeaderMiddlewareOptions options)
        {
            app.UseMiddleware<ResponseHeaderMiddleware>(options);
        }

        #endregion
    }

    /// <summary>
    /// Reprsents a custom middleware allowing to add custome headers to the HTTP Response.
    /// </summary>
    public class ResponseHeaderMiddleware
    {
        #region Members

        private readonly RequestDelegate _next;
        private readonly ResponseHeaderMiddlewareOptions _options;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseHeaderMiddleware"/> class.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"><see cref="ResponseHeaderMiddlewareOptions"/> to use to customize the current <see cref="ResponseHeaderMiddleware"/> behavior.</param>
        public ResponseHeaderMiddleware(RequestDelegate next, ResponseHeaderMiddlewareOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? new ResponseHeaderMiddlewareOptions();
        }

        #endregion

        #region Methods

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var customHeaders = _options.Headers == null ? null : _options.Headers(httpContext);
            if (customHeaders.Any())
            {
                httpContext.Response.OnStarting(() =>
                {
                    foreach (var customHeader in customHeaders)
                    {
                        httpContext.Response.Headers[customHeader.Key] = customHeader.Value;
                    }
                    
                    return Task.CompletedTask;
                });
            }

            await _next(httpContext);
        }

        #endregion
    }

    public class ResponseHeaderMiddlewareOptions
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value allowing to set custom headers to add to the current HTTP Response.
        /// </summary>
        /// <remarks>The <see cref="Headers"/> default value is set to <c>null</c>.</remarks>
        public Func<HttpContext, Dictionary<string, string>> Headers { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseHeaderMiddlewareOptions"/> class.
        /// </summary>
        /// <remarks>
        /// By default:
        ///     - The <see cref="Headers"/> default value is set to <c>null</c>.
        /// </remarks>
        public ResponseHeaderMiddlewareOptions()
        {
            Headers = null;
        }

        #endregion
    }
}
