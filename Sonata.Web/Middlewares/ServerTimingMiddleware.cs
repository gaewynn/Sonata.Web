#region Namespace Sonata.Web.Middlewares
//	The Sonata.Web.Middlewares namespace contains custom middlewares.
#endregion

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sonata.Web.Middlewares
{
    public static class ServerTimingMiddlewareExtensions
    {
        #region Methods

        public static void ConfigureServerTimingMiddleware(this IApplicationBuilder app, ServerTimingMiddlewareOptions options)
        {
            app.UseMiddleware<ServerTimingMiddleware>(options);
        }

        #endregion
    }

    /// <summary>
    /// Reprsents a custom middleware allowing to trace the time to process the current HTTP Request.
    /// The Server Timing will be added to the current "Server-Timing" HTTP Response header.
    /// </summary>
    public class ServerTimingMiddleware
    {
        #region Members

        private readonly RequestDelegate _next;
        private readonly ServerTimingMiddlewareOptions _options;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTimingMiddleware"/> class.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"><see cref="ServerTimingMiddlewareOptions"/> to use to customize the current <see cref="ServerTimingMiddleware"/> behavior.</param>
        public ServerTimingMiddleware(RequestDelegate next, ServerTimingMiddlewareOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? new ServerTimingMiddlewareOptions();
        }

        #endregion

        #region Methods

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var watch = new Stopwatch();
            watch.Start();

            httpContext.Response.OnStarting(() =>
            {
                watch.Stop();

                var timingDescriptors = new List<ServerTimingDescriptor>
                { 
                    new ServerTimingDescriptor("overall")
                    {
                        Duration = watch.ElapsedMilliseconds
                    } 
                };

                if (_options.AdditionalDescriptors != null)
                {
                    var additionalDescriptors = _options.AdditionalDescriptors(httpContext);
                    if (additionalDescriptors != null && additionalDescriptors.Any())
                    {
                        timingDescriptors.AddRange(additionalDescriptors);
                    }
                }

                httpContext.Response.Headers[_options.ServerTimingHeaderName] = string.Join(", ", timingDescriptors.Select(e => e.ToString()));

                return Task.CompletedTask;
            });

            await _next(httpContext);
        }

        #endregion
    }

    public class ServerTimingMiddlewareOptions
    {
        #region Constants

        private const string ServerTimingDefaultHeaderName = "Server-Timing";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating the name of the header that will be added to the current HTTP Response.
        /// </summary>
        /// <remarks>The <see cref="ServerTimingHeaderName"/> default value is set to "Server-Timing" (according to the W3C: https://www.w3.org/TR/server-timing/).</remarks>
        public string ServerTimingHeaderName { get; set; }

        /// <summary>
        /// Gets or sets a value allowing to put additional <see cref="ServerTimingDescriptor"/> to the current HTTP Response header.
        /// </summary>
        /// <remarks>The <see cref="ServerTimingHeaderName"/> default value is set to <c>null</c></remarks>
        public Func<HttpContext, IEnumerable<ServerTimingDescriptor>> AdditionalDescriptors { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerTimingMiddlewareOptions"/> class.
        /// </summary>
        /// <remarks>
        /// By default:
        ///     - The <see cref="ServerTimingHeaderName"/> default value is set to "Server-Timing".
        ///     - The <see cref="AdditionalDescriptors"/> default value is set to <c>null</c>.
        /// </remarks>
        public ServerTimingMiddlewareOptions()
        {
            ServerTimingHeaderName = ServerTimingDefaultHeaderName;
            AdditionalDescriptors = null;
        }

        #endregion
    }

    public class ServerTimingDescriptor
    {
        #region Properties

        public string Name { get; private set; }

        public string Description { get; set; }

        public long? Duration { get; set; }

        #endregion

        #region Constructors

        public ServerTimingDescriptor(string name)
        {
            Name = name;
        }

        #endregion

        #region Methods

        #region Object Members

        public override string ToString()
        {
            var descriptor = Name;
            if (!string.IsNullOrWhiteSpace(Description))
            {
                descriptor += $";desc=\"{Description}\"";
            }

            if (Duration.HasValue)
            {
                descriptor += $";dur={Duration.Value}";
            }

            return descriptor;
        }

        #endregion

        #endregion
    }
}
