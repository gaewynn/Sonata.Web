#region Namespace Sonata.Web.Http
//	The Sonata.Web.Http namespace contains classes of HTTP attributes.
#endregion

using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Sonata.Web.Http
{
	/// <inheritdoc />
	/// <summary>
	/// Represents a components allowing to send a file back to the browser and so download the file.
	/// </summary>
	public class FileActionResult : IActionResult
	{
		#region Constants

		/// <summary>
		/// The header value to download a file as an attachment.
		/// </summary>
		private const string AttachmentContentDisposition = "attachment";

		#endregion

		#region Members

		private readonly string _fileName;
		private readonly MemoryStream _stream;

		#endregion

		#region Constructors

		/// <summary>
		/// Initialize a new <see cref="FileActionResult"/>.
		/// </summary>
		/// <param name="fileName">The name of the file which will be displayed to the user when downloading.</param>
		/// <param name="stream">The content of the file to download.</param>
		public FileActionResult(string fileName, MemoryStream stream)
		{
			if (String.IsNullOrWhiteSpace(fileName))
				throw new ArgumentNullException(nameof(fileName));

			_fileName = fileName;
			_stream = stream ?? throw new ArgumentNullException(nameof(stream));

			_stream.Position = 0;
			_stream.Seek(0, SeekOrigin.Begin);
		}

		#endregion

		#region Methods

		#region IHttpActionResult Members
		
		/// <inheritdoc />
		/// <summary>
		/// Send the downloaded file back to the browser.
		/// </summary>
		/// <param name="context">The context in which the result is executed. The context information includes information about the action that was executed and request information.</param>
		/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that, when completed, contains the <see cref="T:System.Net.Http.HttpResponseMessage" /> with the downloaded file.</returns>
		public Task ExecuteResultAsync(ActionContext context)
		{
			var response = new HttpResponseMessage { Content = new StreamContent(_stream) };
			response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(AttachmentContentDisposition)
			{
				FileName = Path.GetFileName(_fileName)
			};

			return Task.FromResult(response);
		}

		#endregion

		#endregion
	}
}