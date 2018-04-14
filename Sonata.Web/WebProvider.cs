#region Namespace Sonata.Web
//	TODO
#endregion

using System;
using System.Diagnostics;

namespace Sonata.Web
{
	public class WebProvider
	{
		#region Constructors

		private WebProvider()
		{ }

		#endregion

		#region Methods

		/// <summary>
		/// Configures the behavior of the Sonata.Web library.
		/// </summary>
		/// <param name="enableTraces">true to enable debug traces; otherwise false.</param>
		public static void Configure(bool enableTraces = false)
		{
			WebConfiguration.IsDebugModeEnabled = enableTraces;
		}
		
		internal static void Trace(string message)
		{
			if (!WebConfiguration.IsDebugModeEnabled
				|| String.IsNullOrWhiteSpace(message))
				return;

			Console.WriteLine($"{DateTime.Now:HH:mm:ss} - [Sonata.Web] - {message}");
			Debug.WriteLine($"{DateTime.Now:HH:mm:ss} - [Sonata.Web] - {message}");
		}
		
		#endregion
	}
}
