#region Namespace Sonata.Web.Middlewares
//	The Sonata.Web.Middlewares namespace contains custom middlewares.
#endregion

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Sonata.Web.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
namespace Sonata.Web.Middlewares
{
    public static class LoggingMiddlewareExtensions
    {
        #region Methods

        public static void ConfigureLoggingMiddleware(this IApplicationBuilder app, LoggingMiddlewareOptions options)
        {
            app.UseMiddleware<LoggingMiddleware>(options);
        }

        #endregion
    }

    /// <summary>
    /// Reprsents a custom middleware allowing to trigger events to log <see cref="Microsoft.AspNetCore.Mvc.ControllerBase"/> actions.
    /// </summary>
    public class LoggingMiddleware
    {
        #region Members

        private readonly RequestDelegate _next;
        private readonly LoggingMiddlewareOptions _options;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"><see cref="LoggingMiddlewareOptions"/> to use to customize the current <see cref="LoggingMiddleware"/> behavior.</param>
        public LoggingMiddleware(RequestDelegate next, LoggingMiddlewareOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? new LoggingMiddlewareOptions();
        }

        #endregion

        #region Methods

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (_options.DoNotLog != null && _options.DoNotLog(httpContext))
            {
                await _next(httpContext);
            }
            else
            {
                var serializedRequest = await _options.SerializeRequestAsync(httpContext);
                await _options.OnLogRequestAsync?.Invoke(httpContext, serializedRequest);

                var responseBodyStream = httpContext?.Response?.Body;
                if (responseBodyStream != null)
                {
                    using (var responseBody = new MemoryStream())
                    {
                        httpContext.Response.Body = responseBody;
                        await _next(httpContext);

                        var serializedResponse = await _options.SerializeResponseAsync(httpContext);
                        await _options.OnLogResponseAsync?.Invoke(httpContext, serializedResponse);

                        await responseBody.CopyToAsync(responseBodyStream);
                    }
                }
                else
                {
                    await _next(httpContext);
                }
            }
        }

        #endregion
    }

    public class LoggingMiddlewareOptions
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value allowing to prevent the current <see cref="LoggingMiddleware"/> to log.
        /// </summary>
        /// <remarks>The <see cref="DoNotLog"/> default value is set to <c>null</c>.</remarks>
        public Func<HttpContext, bool> DoNotLog { get; set; }

        /// <summary>
        /// Gets or sets a value allowing to serialize the current <see cref="HttpContext.Request"/>.
        /// </summary>
        /// <remarks>The <see cref="SerializeRequestAsync"/> default value is set to return the request in the following format: "<see cref="HttpRequest.Method"/> <see cref="HttpRequest.Scheme"/>://<see cref="HttpRequest.Host"/><see cref="HttpRequest.Path"/><see cref="HttpRequest.QueryString"/> [Body: <see cref="HttpRequest.Body"/>]".</remarks>
        public Func<HttpContext, Task<string>> SerializeRequestAsync { get; set; }

        /// <summary>
        /// Gets or sets a value allowing to serialize the current <see cref="HttpContext.Response"/>.
        /// </summary>
        /// <remarks>The <see cref="SerializeResponseAsync"/> default value is set to return the request in the following format: "<see cref="HttpResponse.StatusCode"/> <see cref="HttpRequest.Method"/> <see cref="HttpRequest.Scheme"/>://<see cref="HttpRequest.Host"/><see cref="HttpRequest.Path"/> [Body: <see cref="HttpResponse.Body"/>]".</remarks>
        public Func<HttpContext, Task<string>> SerializeResponseAsync { get; set; }

        /// <summary>
        /// Gets or sets a value allowing to run a custom logging process of the <see cref="HttpRequest"/>.
        /// </summary>
        /// <remarks>The <see cref="OnLogRequestAsync"/> default value is set to <c>null</c>.</remarks>
        public Func<HttpContext, string, Task> OnLogRequestAsync { get; set; }

        /// <summary>
        /// Gets or sets a value allowing to run a custom logging process of the <see cref="HttpResponse"/>.
        /// </summary>
        /// <remarks>The <see cref="OnLogResponseAsync"/> default value is set to <c>null</c>.</remarks>
        public Func<HttpContext, string, Task> OnLogResponseAsync { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingMiddlewareOptions"/> class.
        /// </summary>
        /// <remarks>
        /// By default:
        ///     - The <see cref="DoNotLog"/> default value is set to <c>null</c>.
        ///     - The <see cref="SerializeRequestAsync"/> default value is set to return the request in the following format: "<see cref="HttpRequest.Method"/> <see cref="HttpRequest.Scheme"/>://<see cref="HttpRequest.Host"/><see cref="HttpRequest.Path"/><see cref="HttpRequest.QueryString"/> [Body: <see cref="HttpRequest.Body"/>]".
        ///     - The <see cref="SerializeResponseAsync"/> default value is set to return the request in the following format: "<see cref="HttpResponse.StatusCode"/> <see cref="HttpRequest.Method"/> <see cref="HttpRequest.Scheme"/>://<see cref="HttpRequest.Host"/><see cref="HttpRequest.Path"/> [Body: <see cref="HttpResponse.Body"/>]".
        ///     - The <see cref="OnLogRequest"/> default value is set to <c>null</c>.
        ///     - The <see cref="OnLogResponse"/> default value is set to <c>null</c>.
        /// </remarks>
        public LoggingMiddlewareOptions()
        {
            DoNotLog = null;
            SerializeRequestAsync = DefaultRequestSerializerAsync;
            SerializeResponseAsync = DefaultResponseSerializerAsync;
            OnLogRequestAsync = null;
            OnLogResponseAsync = null;
        }

        #endregion

        #region Methods

        private async Task<string> DefaultRequestSerializerAsync(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(HttpContext));
            }

            if (httpContext.Request == null)
            {
                return string.Empty;
            }

            var requestBody = await httpContext.Request.ReadBodyAsStringAsync();
            var shortenedRequestBody = requestBody != null
                ? requestBody.Substring(0, Math.Min(10000, requestBody.Length))
                : string.Empty;

            return $"{httpContext.Request.Method} {httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString} [Body: {shortenedRequestBody}]";
        }

        private async Task<string> DefaultResponseSerializerAsync(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(HttpContext));
            }

            if (httpContext.Response == null)
            {
                return string.Empty;
            }

            var responseBody = await httpContext.Response.ReadBodyAsStringAsync();
            var shortenedResponseBody = responseBody != null
                ? responseBody.Substring(0, Math.Min(10000, responseBody.Length))
                : string.Empty;

            return $"{httpContext.Response.StatusCode} {httpContext.Request.Method} {httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString} [Body: {shortenedResponseBody}]";
        }

        #endregion
    }
}
