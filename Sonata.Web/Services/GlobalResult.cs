#region Namespace Sonata.Web.Services
//	TODO
#endregion

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sonata.Web.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sonata.Web.Services
{
    /// <summary>
    /// Represents a component allowing to encapsulate a <see cref="IActionResult"/> and a custom result depending on the needs.
    /// </summary>
    /// <typeparam name="TResult">The inner type encapsulated in the <see cref="GlobalResult"/>.</typeparam>
    public class GlobalResult<TResult>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the retrned result in case of error.
        /// </summary>
        public IActionResult ErrorResult { get; private set; }

        /// <summary>
        /// Gets or sets the retrned result in case of success.
        /// </summary>
        public TResult OkResult { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new indtance of the class <see cref="GlobalResult{TResult}"/>.
        /// </summary>
        /// <param name="errorResult">The <see cref="IActionResult"/> that will be used to valorize the <see cref="GlobalResult{TResult}.ErrorResult"/> property.</param>
        public GlobalResult(IActionResult errorResult)
        {
            ErrorResult = errorResult;
        }

        /// <summary>
        /// Initialize a new indtance of the class <see cref="GlobalResult{TResult}"/>.
        /// </summary>
        /// <param name="okResult">A value that will be used to valorize the <see cref="GlobalResult{TResult}.OkResult"/> property.</param>
        public GlobalResult(TResult okResult)
        {
            OkResult = okResult;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets either the <see cref="GlobalResult{TResult}.ErrorResult"/> if not null: otherwise gets an <see cref="OkObjectResult"/> containing the current <see cref="GlobalResult{TResult}.OkResult"/>.
        /// </summary>
        /// <returns>Either the <see cref="GlobalResult{TResult}.ErrorResult"/> if not null: otherwise gets an <see cref="OkObjectResult"/> containing the current <see cref="GlobalResult{TResult}.OkResult"/>.</returns>
        public IActionResult ToActionResult()
        {
            return ErrorResult ?? new OkObjectResult(OkResult);
        }

        /// <summary>
        /// Gets either the <see cref="GlobalResult{TResult}.ErrorResult"/> if not null: otherwise gets an <see cref="OkObjectResult"/> containing the result of the transformation defined my the specified <paramref name="map"/> function.
        /// </summary>
        /// <typeparam name="TDestination">The returned type of the <paramref name="map"/> function.</typeparam>
        /// <param name="map">A function allowing to convert the current <see cref="GlobalResult{TResult}.OkResult"/> into a <typeparamref name="TDestination"/> type.</param>
        /// <returns>Either the <see cref="GlobalResult{TResult}.ErrorResult"/> if not null: otherwise gets an <see cref="OkObjectResult"/> containing the result of the transformation defined my the specified <paramref name="map"/> function.</returns>
        public IActionResult ToActionResult<TDestination>(Func<TResult, TDestination> map)
        {
            return ErrorResult ?? new OkObjectResult(map(OkResult));
        }

        /// <summary>
        /// Reads the specified <paramref name="response"/> and convert it to a <see cref="GlobalResult{TResult}"/> based on the inner <see cref="HttpResponseMessage.StatusCode"/> and <see cref="HttpResponseMessage.Content"/>.
        /// The returned <see cref="GlobalResult{TResult}.OkResult"/> will contain:
        ///     - the <see cref="HttpResponseMessage.Content"/> if the specified <paramref name="response"/> has a <see cref="HttpStatusCode.OK"/>
        ///     - <c>null</c> if the specified <paramref name="response"/> does not have a <see cref="HttpStatusCode.OK"/>
        /// The returned <see cref="GlobalResult{TResult}.ErrorResult"/> will contain:
        ///     - <c>null</c> if the specified <paramref name="response"/> has a <see cref="HttpStatusCode.OK"/>
        ///     - an <see cref="IActionResult"/> matching the <see cref="HttpStatusCode"/> of the specified <paramref name="response"/>
        /// </summary>
        /// <param name="response">A <see cref="HttpResponseMessage"/> containig information that will be used to convert it in a <see cref="GlobalResult{TResult}"/>.</param>
        /// <returns>A <see cref="GlobalResult{TResult}"/> containing information about the specified <paramref name="response"/>.</returns>
        public static async Task<GlobalResult<TResult>> ReadAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content == null
                    ? new GlobalResult<TResult>(null)
                    : new GlobalResult<TResult>(JsonConvert.DeserializeObject<TResult>(await response.Content.ReadAsStringAsync()));
            }

            return new GlobalResult<TResult>(await response.ToActionResultAsync());
        }

        #endregion
    }
}
