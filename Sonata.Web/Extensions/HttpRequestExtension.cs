#region Namespace Sonata.Web.Extensions
//	TODO
#endregion

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace Sonata.Web.Extensions
{
	public static class HttpRequestExtension
	{
		public static string GetBearer(this HttpRequest instance)
		{
			WebProvider.Trace($"Call to {nameof(GetBearer)}.");

			if (instance == null)
				return null;

			if (!(instance is DefaultHttpRequest httpRequest))
			{
				WebProvider.Trace("		Invalid HTTP request.");
				return null;
			}

			if (!(httpRequest.Headers is FrameRequestHeaders headers))
			{
				WebProvider.Trace("		Invalid HTTP request headers.");
				return null;
			}

			if (StringValues.IsNullOrEmpty(headers.HeaderAuthorization))
			{
				WebProvider.Trace("		No Authorization header provided.");
				return null;
			}

			if (!headers.HeaderAuthorization.ToString().Trim().StartsWith("Bearer"))
			{
				WebProvider.Trace("		No Bearer token defined.");
				return null;
			}

			return headers.HeaderAuthorization.ToString().Replace("Bearer", String.Empty).Trim();
		}
	}
}
