#region Namespace Sonata.Web.Middlewares
//	The Sonata.Web.Middlewares namespace contains custom middlewares.
#endregion

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Sonata.Web.Extensions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sonata.Web.Middlewares
{
    public static class BearerTokenValidatorMiddlewareExtensions
    {
        #region Methods

        public static void ConfigureBearerTokenValidatorMiddleware(this IApplicationBuilder app, BearerTokenValidatorMiddlewareOptions options)
        {
            app.UseMiddleware<BearerTokenValidatorMiddleware>(options);
        }

        #endregion
    }

    /// <summary>
    /// Reprsents a custom middleware allowing to validate a bearer token provided in the Authorization <see cref="HttpRequest.Headers"/.
    /// </summary>
    public class BearerTokenValidatorMiddleware
    {
        #region Members

        private readonly RequestDelegate _next;
        private readonly BearerTokenValidatorMiddlewareOptions _options;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BearerTokenValidatorMiddleware"/> class.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"><see cref="BearerTokenValidatorMiddlewareOptions"/> to use to customize the current <see cref="BearerTokenValidatorMiddleware"/> behavior.</param>
        public BearerTokenValidatorMiddleware(RequestDelegate next, BearerTokenValidatorMiddlewareOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? new BearerTokenValidatorMiddlewareOptions();
        }

        #endregion

        #region Methods

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (_options.DoNotValidate != null && _options.DoNotValidate(httpContext))
            {
                await _next(httpContext);
            }
            else
            {
                var encodedToken = httpContext.Request.GetBearer();
                if (encodedToken == null)
                {
                    if (_options.WriteErrorIfNoTokenFound)
                    {
                        var missingTokenMessage = await _options.MissingTokenMessageBuilderAsync(httpContext);
                        await HandleTokenAsync(httpContext, missingTokenMessage);
                    }
                    else
                    {
                        await _next(httpContext);
                    }
                }
                else
                {
                    var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                    if (!jwtSecurityTokenHandler.CanReadToken(encodedToken))
                    {
                        var invalidTokenFoundMessage = await _options.CantReadTokenMessageBuilderAsync(httpContext);
                        await HandleTokenAsync(httpContext, invalidTokenFoundMessage);
                    }
                    else
                    {
                        var decodedToken = new JwtSecurityToken(encodedToken);
                        var errorMessagesBuilder = new StringBuilder();
                        var isError = false;

                        if (_options.MandatoriesClaims != null && _options.MandatoriesClaims.Any())
                        {
                            foreach (var claimType in _options.MandatoriesClaims)
                            {
                                if (string.IsNullOrEmpty(decodedToken.Claims.Where(c => c.Type == claimType).FirstOrDefault()?.Value))
                                {
                                    var errorMessage = await _options.MandatoriesClaimsErrorMessageBuilderAsync(claimType);
                                    errorMessagesBuilder.AppendLine(errorMessage);
                                    isError = true;
                                }
                            }
                        }

                        if (isError)
                        {
                            await HandleTokenAsync(httpContext, errorMessagesBuilder.ToString());
                        }
                        else
                        {
                            var ruleErrorMessagesBuilder = new StringBuilder();
                            var isRuleError = false;

                            if (_options.MandatoriesClaimsRules != null && _options.MandatoriesClaimsRules.Any())
                            {
                                foreach (var rule in _options.MandatoriesClaimsRules)
                                {
                                    var invalidTokenState = await _options.MandatoriesClaimsRules[rule.Key](decodedToken, rule.Key);
                                    if (!invalidTokenState.IsValid)
                                    {
                                        ruleErrorMessagesBuilder.AppendLine(invalidTokenState.ErrorMessage);
                                        isRuleError = true;
                                    }
                                }

                                if (isRuleError)
                                {
                                    await HandleTokenAsync(httpContext, ruleErrorMessagesBuilder.ToString());
                                }
                                else
                                {
                                    await _next(httpContext);
                                }
                            }
                            else
                            {
                                await _next(httpContext);
                            }
                        }
                    }
                }
            }
        }

        private Task HandleTokenAsync(HttpContext httpContext, string message)
        {
            httpContext.Response.ContentType = _options.ResponseContentType;
            httpContext.Response.StatusCode = (int)_options.ResponseStatusCode;

            return httpContext.Response.WriteAsync(new
            {
                StatusCode = httpContext.Response.StatusCode,
                Message = message

            }.ToString());
        }

        #endregion
    }

    public class BearerTokenValidatorMiddlewareOptions
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value allowing to prevent the current <see cref="LoggingMiddleware"/> to validate the current <see cref="HttpRequest"/>.
        /// </summary>
        /// <remarks>The <see cref="DoNotValidate"/> default value is set to <c>null</c>.</remarks>
        public Func<HttpContext, bool> DoNotValidate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the <see cref="BearerTokenValidatorMiddleware"/> has to write an error in the <see cref="HttpResponse"/> if no token has been found.
        /// </summary>
        /// remarks>The <see cref="WriteErrorIfNoTokenFound"/> default value is set to <c>true</c>.</remarks>
        public bool WriteErrorIfNoTokenFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the mandatories claims that have to be in the bearer token of the current <see cref="HttpRequest"/>.
        /// </summary>
        /// <remarks>The <see cref="MandatoriesClaims"/> default value is set to <c>null</c>.</remarks>
        public IEnumerable<string> MandatoriesClaims { get; set; }

        /// <summary>
        /// Gets or sets a value allowing to customize the error message to write on the <see cref="HttpResponse"/> if the claims is not found.
        /// </summary>
        /// <remarks>The <see cref="MandatoriesClaimsErrorMessageBuilderAsync"/> default value is set to "'CLAIM_TYPE' not found".</remarks>
        public Func<string, Task<string>> MandatoriesClaimsErrorMessageBuilderAsync { get; set; }

        /// <summary>
        /// Gets or sets a value allowing to set custom rules to validate each claims on the current <see cref="HttpRequest"/>.
        /// </summary>
        /// <remarks>The <see cref="MandatoriesClaimsRules"/> default value is set to <c>null</c>.</remarks>
        public Dictionary<string, Func<JwtSecurityToken, string, Task<InvalidTokenState>>> MandatoriesClaimsRules { get; set; }

        /// <summary>
        /// Gets or sets a value allowing to customize the returned error message if no toekn has been found in the current <see cref="HttpRequest"/>.
        /// </summary>
        /// <remarks>The <see cref="MissingTokenMessageBuilderAsync"/> default value is set to returned the following message: 'No token found'.</remarks>
        public Func<HttpContext, Task<string>> MissingTokenMessageBuilderAsync { get; set; }

        /// <summary>
        /// Gets or sets a value allowing to customize the returned error message if the token is not readable in the current <see cref="HttpRequest"/>.
        /// </summary>
        /// <remarks>The <see cref="CantReadTokenMessageBuilderAsync"/> default value is set to returned the following message: 'Invalid token found'.</remarks>
        public Func<HttpContext, Task<string>> CantReadTokenMessageBuilderAsync { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the returned <see cref="HttpResponse.ContentType"/> when an error is found on the token.
        /// </summary>
        /// <remarks>The <see cref="ResponseContentType"/> default value is set to "application/json" (<see cref="MediaTypeNameApplicationJson)"/>.</remarks>
        public string ResponseContentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the returned <see cref="HttpResponse.StatusCode"/> when an error is found on the token.
        /// </summary>
        /// <remarks>The <see cref="ResponseStatusCode"/> default value is set to <see cref="HttpStatusCode.Unauthorized"/>.</remarks>
        public HttpStatusCode ResponseStatusCode { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BearerTokenValidatorMiddlewareOptions"/> class.
        /// </summary>
        /// <remarks>
        /// By default:
        ///     - The <see cref="DoNotValidate"/> default value is set to <c>null</c>.
        ///     - The <see cref="WriteErrorIfNoTokenFound"/> default value is set to <c>true</c>.
        ///     - The <see cref="MandatoriesClaims"/> default value is set to <c>null</c>.
        ///     - The <see cref="MandatoriesClaimsErrorMessageBuilderAsync"/> default value is set to "'CLAIM_TYPE' not found".
        ///     - The <see cref="MandatoriesClaimsRules"/> default value is set to <c>null</c>.
        ///     - The <see cref="MissingTokenMessageBuilderAsync"/> default value is set to returned the following message: 'No token found'.
        ///     - The <see cref="CantReadTokenMessageBuilderAsync"/> default value is set to returned the following message: 'Invalid token found'.
        ///     - The <see cref="ResponseContentType"/> default value is set to "application/json" (<see cref="MediaTypeNames.ApplicationJson)"/>.
        ///     - The <see cref="ResponseStatusCode"/> default value is set to <see cref="HttpStatusCode.Unauthorized"/>.
        /// </remarks>
        public BearerTokenValidatorMiddlewareOptions()
        {
            DoNotValidate = null;
            WriteErrorIfNoTokenFound = true;
            MandatoriesClaims = null;
            MandatoriesClaimsErrorMessageBuilderAsync = async (claimType) => "'{0}' not found";
            MandatoriesClaimsRules = null;
            MissingTokenMessageBuilderAsync = async (httpContext) => "No token found";
            CantReadTokenMessageBuilderAsync = async (httpContext) => "Invalid token found";
            ResponseContentType = MediaTypeNames.ApplicationJson;
            ResponseStatusCode = HttpStatusCode.Unauthorized;
        }

        #endregion
    }

    public class InvalidTokenState
    {
        #region Properties

        public string ErrorMessage { get; set; }

        public bool IsValid { get; set; }

        #endregion

        #region Constructors

        public InvalidTokenState()
        {
            IsValid = true;
        }

        #endregion
    }
}
