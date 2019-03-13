#region Namespace Sonata.Web.Services
//	TODO
#endregion

using System.Collections.Generic;
using Sonata.Diagnostics.Logs;

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
		public List<Log> Logs { get; set; }

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
			Logs = new List<Log>();
		}

		#endregion
	}
}
