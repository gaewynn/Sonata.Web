#region Namespace Sonata.Web.Extensions
//	TODO
#endregion

using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sonata.Web.Extensions
{
    public static class HttpResponseExtension
    {
        public static async Task<string> ReadBodyAsStringAsync(this HttpResponse instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (instance.Body.CanSeek)
            {
                instance.Body.Position = 0;
                instance.Body.Seek(0, SeekOrigin.Begin);
            }

            var streamReader = new StreamReader(instance.Body);
            var bodyAsText = await streamReader.ReadToEndAsync();

            if (instance.Body.CanSeek)
            {
                instance.Body.Position = 0;
                instance.Body.Seek(0, SeekOrigin.Begin);
            }

            return bodyAsText;
        }
    }
}
