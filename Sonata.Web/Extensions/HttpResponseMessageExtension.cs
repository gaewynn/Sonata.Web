﻿#region Namespace Sonata.Web.Extensions
//	TODO
#endregion

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sonata.Web.Extensions
{
    public static class HttpResponseMessageExtension
    {
        #region Methods

        /// <summary>
        /// Creates an <see cref="IActionResult"/>  from a <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="message">The <see cref="HttpResponseMessage"/> to convert.</param>
        /// <param name="location">The location of the created resource. This location is used only when a <see cref="HttpStatusCode.Created"/> is handled.</param>
        /// <param name="authenticationSchemes">The authentication schemes to challenge. This location is used only when a <see cref="HttpStatusCode.Forbidden"/> is handled.</param>
        /// <param name="authenticationProperties"><see cref="AuthenticationProperties"/> used to perform the authentication challenge. This location is used only when a <see cref="HttpStatusCode.Forbidden"/> is handled.</param>
        /// <param name="objectResultTypeIfNotSupported">The type of the <see cref="ObjectResult"/> to convert to if the <see cref="HttpResponseMessage.StatusCode"/> of the specified <paramref name="message"/> if not managed.</param>
        /// <returns>
        /// An <see cref="IActionResult"/>:
        ///     - containing the stringified <see cref="HttpResponseMessage.Content"/> of the specified <paramref name="message"/>.
        ///     - having its <see cref="IActionResult.StatusCode"/> property set to the <see cref="HttpResponseMessage.StatusCode"/> of the specified <paramref name="message"/>.
        /// </returns>
        /// <remarks>
        /// Only the following status codes are managed:
        ///     - HttpStatusCode.BadGateway
        ///     - HttpStatusCode.BadRequest
        ///     - HttpStatusCode.Conflict
        ///     - HttpStatusCode.OK
        ///     - HttpStatusCode.NotFound
        ///     - HttpStatusCode.Unauthorized
        ///     - HttpStatusCode.Created
        ///     - HttpStatusCode.NoContent
        ///     - HttpStatusCode.Forbidden
        ///     - UnprocessableEntity (422)
        /// </remarks>
        public static async Task<IActionResult> ToActionResultAsync(this HttpResponseMessage message,
            Uri location = null,
            List<string> authenticationSchemes = null,
            AuthenticationProperties authenticationProperties = null,
            Type objectResultTypeIfNotSupported = null)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var messageContent = await message.Content?.ReadAsStringAsync();
            ObjectResult resultingObjectResult;

            switch (message.StatusCode)
            {
                case HttpStatusCode.OK:
                    resultingObjectResult = new OkObjectResult(messageContent);
                    break;

                case HttpStatusCode.Created:
                    resultingObjectResult = new CreatedResult(location, messageContent);
                    break;

                case HttpStatusCode.NotFound:
                    resultingObjectResult = new NotFoundObjectResult(messageContent);
                    break;

                case HttpStatusCode.BadRequest:
                    resultingObjectResult = new BadRequestObjectResult(messageContent);
                    break;

                case HttpStatusCode.Unauthorized:
                    resultingObjectResult = new UnauthorizedObjectResult(messageContent);
                    break;

                case HttpStatusCode.BadGateway:
                    resultingObjectResult = new BadRequestObjectResult(messageContent);
                    break;

                case HttpStatusCode.Conflict:
                    resultingObjectResult = new ConflictObjectResult(messageContent);
                    break;

                case HttpStatusCode.NoContent:
                    return new NoContentResult();

                case HttpStatusCode.Forbidden:
                    return new ForbidResult(authenticationSchemes, authenticationProperties);

                case HttpStatusCode.InternalServerError:
                    return new StatusCodeResult((int)HttpStatusCode.InternalServerError);

                default:
                    if ((int)message.StatusCode == 422)
                    {
                        resultingObjectResult = new UnprocessableEntityObjectResult(messageContent);
                    }
                    else
                    {
                        objectResultTypeIfNotSupported = objectResultTypeIfNotSupported ?? typeof(OkObjectResult);
                        resultingObjectResult = Activator.CreateInstance(objectResultTypeIfNotSupported) as ObjectResult;
                        if (resultingObjectResult != null)
                        {
                            resultingObjectResult.Value = messageContent;
                        }
                    }
                    break;
            }

            resultingObjectResult.StatusCode = (int)message.StatusCode;

            return resultingObjectResult;
        }

        #endregion
    }
}
