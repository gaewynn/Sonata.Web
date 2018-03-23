#region Namespace Sonata.Web.Services
//	TODO
#endregion

using Sonata.Diagnostics.Logging;
using System.Collections.Generic;

namespace Sonata.Web.Services
{
	/// <summary>
	/// 
	/// </summary>
	public class ResponseParams
	{
		#region Properties

		/// <summary>
		/// 
		/// </summary>
		public List<ILog4NetProperties> Logs { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Username { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// 
		/// </summary>
		public ResponseParams()
		{
			Logs = new List<ILog4NetProperties>();
		}

		#endregion
	}
}
