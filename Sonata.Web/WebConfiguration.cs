#region Namespace Sonata.Web
//	TODO
#endregion

using System.Configuration;
using System.Linq;

namespace Sonata.Web
{
    internal class WebConfiguration
	{
		#region Constants

		private const string IsDebugModeEnabledKey = "Sonata.Internal.Debug";

		#endregion

		#region Properties

		public static bool IsDebugModeEnabled { get; set; }

		#endregion

		#region Constructors

		static WebConfiguration()
		{
			IsDebugModeEnabled = false;
			if (!ConfigurationManager.AppSettings.AllKeys.Contains(IsDebugModeEnabledKey))
				return;

			bool.TryParse(ConfigurationManager.AppSettings[IsDebugModeEnabledKey], out var isDebugModeEnabled);
			IsDebugModeEnabled = isDebugModeEnabled;
		}

		#endregion
	}
}
