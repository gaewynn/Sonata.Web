#region Namespace Sonata.Web.Middlewares
//	The Sonata.Web.Middlewares namespace contains custom middlewares.
#endregion

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Sonata.Web.Middlewares
{
    public static class ExceptionMiddlewareExtensions
    {
        #region Methods

        public static void ConfigureExceptionMiddleware(this IApplicationBuilder app, ExceptionMiddlewareOptions options)
        {
            app.UseMiddleware<ExceptionMiddleware>(options);
        }

        #endregion
    }

    /// <summary>
    /// Reprsents a custom middleware allowing to handle all <see cref="Exception"/> thrown in a <see cref="HttpRequest"/> pipeline.
    /// </summary>
    public class ExceptionMiddleware
    {
        #region Members

        private readonly RequestDelegate _next;
        private readonly ExceptionMiddlewareOptions _options;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"><see cref="ExceptionMiddlewareOptions"/> to use to customize the current <see cref="ExceptionMiddleware"/> behavior.</param>
        public ExceptionMiddleware(RequestDelegate next, ExceptionMiddlewareOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? new ExceptionMiddlewareOptions();
        }

        #endregion

        #region Methods

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _options.OnException?.Invoke(ex);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = _options.ResponseContentType;
            context.Response.StatusCode = (int)_options.ResponseStatusCode;

            return context.Response.WriteAsync(new
            {
                StatusCode = (int)_options.ResponseStatusCode,
                Message = _options.ResponseMessageBuildder(exception)

            }.ToString());
        }

        #endregion
    }

    public class ExceptionMiddlewareOptions
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value allowing to run a custom operation when an <see cref="Exception"/> is raised.
        /// </summary>
        /// <remarks>The <see cref="OnException"/> default value is set to <c>null</c>.</remarks>
        public Action<Exception> OnException { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the returned <see cref="HttpResponse.ContentType"/> when an <see cref="Exception"/> occured.
        /// </summary>
        /// <remarks>The <see cref="ResponseContentType"/> default value is set to "application/json" (<see cref="MediaTypeNameApplicationJson)"/>.</remarks>
        public string ResponseContentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the returned <see cref="HttpResponse.StatusCode"/> when an <see cref="Exception"/> occured.
        /// </summary>
        /// <remarks>The <see cref="ResponseStatusCode"/> default value is set to <see cref="HttpStatusCode.InternalServerError"/>.</remarks>
        public HttpStatusCode ResponseStatusCode { get; set; }

        /// <summary>
        /// Gets or sets a value allowing to customize the returned message when an <see cref="Exception"/> occured.
        /// </summary>
        /// <remarks>The <see cref="ResponseMessageBuildder"/> default value is set to "Internal Server Error.".</remarks>
        public Func<Exception, string> ResponseMessageBuildder { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionMiddlewareOptions"/> class.
        /// </summary>
        /// <remarks>
        /// By default:
        ///     - The <see cref="OnException"/> default value is set to <c>null</c>.
        ///     - The <see cref="ResponseContentType"/> default value is set to "application/json" (<see cref="MediaTypeNames.ApplicationJson)"/>.
        ///     - The <see cref="ResponseStatusCode"/> default value is set to <see cref="HttpStatusCode.InternalServerError"/>.
        ///     - The <see cref="ResponseMessageBuildder"/> default value is set to "Internal Server Error.".
        /// </remarks>
        public ExceptionMiddlewareOptions()
        {
            OnException = null;
            ResponseContentType = MediaTypeNames.ApplicationJson;
            ResponseStatusCode = HttpStatusCode.InternalServerError;
            ResponseMessageBuildder = _ => "Internal Server Error.";
        }

        #endregion
    }
}
