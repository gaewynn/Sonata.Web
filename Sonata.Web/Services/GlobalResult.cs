#region Namespace Sonata.Web.Services
//	TODO
#endregion

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sonata.Web.Extensions;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sonata.Web.Services
{
    /// <summary>
    /// Represents a component allowing to encapsulate a <see cref="IActionResult"/> and a custom result depending on the needs.
    /// </summary>
    /// <typeparam name="TResult">The inner type encapsulated in the <see cref="GlobalResult{TResult}"/>.</typeparam>
    public class GlobalResult<TResult>
    {
        #region Members

        private HttpResponseMessage _response;
        private TResult _responseContent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the returned result in case of error.
        /// </summary>
        public IActionResult ErrorResult { get; private set; }

        /// <summary>
        /// Gets or sets the returned result in case of success.
        /// </summary>
        public TResult Content { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new indtance of the class <see cref="GlobalResult{TResult}"/>.
        /// </summary>
        /// <param name="errorResult">The <see cref="IActionResult"/> that will be used to valorize the <see cref="GlobalResult{TResult}.ErrorResult"/> property.</param>
        private GlobalResult(IActionResult errorResult)
        {
            ErrorResult = errorResult;
        }

        /// <summary>
        /// Initialize a new indtance of the class <see cref="GlobalResult{TResult}"/>.
        /// </summary>
        /// <param name="content">A value that will be used to valorize the <see cref="GlobalResult{TResult}.Content"/> property.</param>
        /// <param name="response">The wrapped <see cref="HttpResponseMessage"/>.</param>
        private GlobalResult(TResult content, HttpResponseMessage response = null)
        {
            Content = content;

            _response = response;
            _responseContent = content;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets either the <see cref="GlobalResult{TResult}.ErrorResult"/> if not null: otherwise gets an <see cref="OkObjectResult"/> containing the current <see cref="GlobalResult{TResult}.Content"/>.
        /// </summary>
        /// <param name="location">An expresion allowing to get the location of the created resource based on the response content: this location is only used when the wrapper <see cref="HttpResponseMessage.StatusCode"/> is <see cref="HttpStatusCode.Created"/>.</param>
        /// <returns>Either the <see cref="GlobalResult{TResult}.ErrorResult"/> if not null: otherwise gets an <see cref="OkObjectResult"/> containing the current <see cref="GlobalResult{TResult}.Content"/>.</returns>
        public IActionResult ToActionResult(Expression<Func<TResult, Uri>> location = null)
        {
            if (ErrorResult != null)
            {
                return ErrorResult;
            }

            if (_response == null)
            {
                return new OkObjectResult(Content);
            }

            switch (_response.StatusCode)
            {
                case HttpStatusCode.Created:
                    return new CreatedResult(_responseContent == null ? null : location?.Compile()(_responseContent), Content);

                case HttpStatusCode.NoContent:
                    return new NoContentResult();

                default:
                    return new OkObjectResult(Content);
            }
        }

        /// <summary>
        /// Gets either the <see cref="GlobalResult{TResult}.ErrorResult"/> if not null: otherwise gets an <see cref="OkObjectResult"/> containing the result of the transformation defined my the specified <paramref name="map"/> function.
        /// </summary>
        /// <typeparam name="TDestination">The returned type of the <paramref name="map"/> function.</typeparam>
        /// <param name="map">A function allowing to convert the current <see cref="GlobalResult{TResult}.Content"/> into a <typeparamref name="TDestination"/> type.</param>
        /// <param name="location">An expresion allowing to get the location of the created resource based on the response content: this location is only used when the wrapper <see cref="HttpResponseMessage.StatusCode"/> is <see cref="HttpStatusCode.Created"/>.</param>
        /// <returns>Either the <see cref="GlobalResult{TResult}.ErrorResult"/> if not null: otherwise gets an <see cref="OkObjectResult"/> containing the result of the transformation defined my the specified <paramref name="map"/> function.</returns>
        public IActionResult ToMappedActionResult<TDestination>(Func<TResult, TDestination> map, Expression<Func<TResult, Uri>> location = null)
        {
            if (ErrorResult != null)
            {
                return ErrorResult;
            }

            if (_response == null)
            {
                return new OkObjectResult(map(Content));
            }

            switch (_response.StatusCode)
            {
                case HttpStatusCode.Created:
                    return new CreatedResult(_responseContent == null ? null : location?.Compile()(_responseContent), map(Content));

                case HttpStatusCode.NoContent:
                    return new NoContentResult();

                default:
                    return new OkObjectResult(map(Content));
            }
        }

        /// <summary>
        /// Instanciate a new <see cref="GlobalResult{TResult}"/> with its content.
        /// Using this instanciation will result in a <see cref="OkObjectResult"/>.
        /// </summary>
        /// <param name="content">The content of the result.</param>
        /// <returns></returns>
        public static GlobalResult<TResult> Create(TResult content)
        {
            return new GlobalResult<TResult>(content);
        }

        /// <summary>
        /// Reads the specified <paramref name="response"/> and convert it to a <see cref="GlobalResult{TResult}"/> based on the inner <see cref="HttpResponseMessage.StatusCode"/> and <see cref="HttpResponseMessage.Content"/>.
        /// The returned <see cref="GlobalResult{TResult}.Content"/> will contain:
        ///     - the <see cref="HttpResponseMessage.Content"/> if the specified <paramref name="response"/> has a <see cref="HttpStatusCode.OK"/>
        ///     - <c>null</c> if the specified <paramref name="response"/> does not have a <see cref="HttpStatusCode.OK"/>
        /// The returned <see cref="GlobalResult{TResult}.ErrorResult"/> will contain:
        ///     - <c>null</c> if the specified <paramref name="response"/> has a <see cref="HttpStatusCode.OK"/>
        ///     - an <see cref="IActionResult"/> matching the <see cref="HttpStatusCode"/> of the specified <paramref name="response"/>
        /// </summary>
        /// <param name="response">A <see cref="HttpResponseMessage"/> containig information that will be used to convert it in a <see cref="GlobalResult{TResult}"/>.</param>
        /// <param name="shouldBeAnErrorResult">
        /// A function returning if the <paramref name="response"/> content should be coonsider as an error. 
        /// In such case, the <see cref="GlobalResult{TResult}.ErrorResult"/> will be filled.
        /// In <paramref name="shouldBeAnErrorResult"/> is null, a success will be considered if the <paramref name="response"/> HTTP status code is between 200 and 299 included.</param>
        /// <returns>A <see cref="GlobalResult{TResult}"/> containing information about the specified <paramref name="response"/>.</returns>
        public static async Task<GlobalResult<TResult>> ReadAsync(HttpResponseMessage response, Func<HttpResponseMessage, bool> shouldBeAnErrorResult = null, Func<HttpResponseMessage, Task> logResponseAsync = null)
        {
            await logResponseAsync?.Invoke(response);

            var isAnErrorResult = (int)response.StatusCode < 200 || (int)response.StatusCode >= 300;
            if (shouldBeAnErrorResult != null)
            {
                isAnErrorResult = shouldBeAnErrorResult(response);
            }

            if (!isAnErrorResult)
            {
                return response.Content == null
                    ? new GlobalResult<TResult>(null)
                    : new GlobalResult<TResult>(JsonConvert.DeserializeObject<TResult>(await response.Content.ReadAsStringAsync()), response);
            }

            return new GlobalResult<TResult>(await response.ToActionResultAsync());
        }

        #endregion
    }
}
