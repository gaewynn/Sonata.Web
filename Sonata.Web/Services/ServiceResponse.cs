#region Namespace Sonata.Web.Services
//	TODO
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Sonata.Diagnostics.Logs;

namespace Sonata.Web.Services
{
	/// <summary>
	/// A structure used to send back a common result structure to the client after a Web API call.
	/// </summary>
	[DataContract(Name = "serviceResponse")]
	public class ServiceResponse
	{
		#region Members

		private bool _isInfos;
		private bool _isDebugs;
		private bool _isWarnings;
		private bool _isErrors;
		private bool _isExceptions;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating the result returned to the client.
		/// </summary>
		[DataMember(Name = "result")]
		public object Result { get; set; }

		/// <summary>
		/// Gets a list of <see cref="LogLevels.Info"/> <see cref="Log"/> generated during the server process.
		/// </summary>
		[DataMember(Name = "infos")]
		public List<Log> Infos { get; set; }

		/// <summary>
		/// Gets a list of <see cref="LogLevels.Debug"/> <see cref="Log"/> generated during the server process.
		/// </summary>
		[DataMember(Name = "debugs")]
		public List<Log> Debugs { get; set; }

		/// <summary>
		/// Gets a list of <see cref="LogLevels.Warning"/> <see cref="Log"/> generated during the server process.
		/// </summary>
		[DataMember(Name = "warnings")]
		public List<Log> Warnings { get; set; }

		/// <summary>
		/// Gets a list of <see cref="LogLevels.Error"/> <see cref="Log"/> generated during the server process.
		/// </summary>
		[DataMember(Name = "errors")]
		public List<Log> Errors { get; set; }

		/// <summary>
		/// Gets a list of <see cref="LogLevels.Fatal"/> <see cref="Log"/> that occured during the server process.
		/// </summary>
		[DataMember(Name = "exceptions")]
		public List<Log> Exceptions { get; set; }

		/// <summary>
		/// Gets a value indicating whether the current <see cref="ServiceResponse"/> contains <see cref="LogLevels.Info"/> <see cref="Log"/>.
		/// </summary>
		[DataMember(Name = "isInfos")]
		public bool IsInfos
		{
			get => _isInfos || Infos.Any();
			set => _isInfos = value;
		}

		/// <summary>
		/// Gets a value indicating whether the current <see cref="ServiceResponse"/> contains <see cref="LogLevels.Debug"/> <see cref="Log"/>.
		/// </summary>
		[DataMember(Name = "isDebugs")]
		public bool IsDebugs
		{
			get => _isDebugs || Debugs.Any();
			set => _isDebugs = value;
		}

		/// <summary>
		/// Gets a value indicating whether the current <see cref="ServiceResponse"/> contains <see cref="LogLevels.Warning"/> <see cref="Log"/>.
		/// </summary>
		[DataMember(Name = "isWarnings")]
		public bool IsWarnings
		{
			get => _isWarnings || Warnings.Any();
			set => _isWarnings = value;
		}

		/// <summary>
		/// Gets a value indicating whether the current <see cref="ServiceResponse"/> contains <see cref="LogLevels.Error"/> <see cref="Log"/>.
		/// </summary>
		[DataMember(Name = "isErrors")]
		public bool IsErrors
		{
			get => _isErrors || Errors.Any();
			set => _isErrors = value;
		}

		/// <summary>
		/// Gets a value indicating whether the current <see cref="ServiceResponse"/> contains <see cref="LogLevels.Fatal"/> <see cref="Log"/>.
		/// </summary>
		[DataMember(Name = "isExceptions")]
		public bool IsExceptions
		{
			get => _isExceptions || Exceptions.Any();
			set => _isExceptions = value;
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceResponse"/> class.
		/// </summary>
		public ServiceResponse()
		{
			Infos = new List<Log>();
			Debugs = new List<Log>();
			Warnings = new List<Log>();
			Errors = new List<Log>();
			Exceptions = new List<Log>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds the specified <paramref name="logs"/> to the current <see cref="ServiceResponse"/>.
		/// </summary>
		/// <param name="logs">A list of <see cref="Log"/> to add to the current <see cref="ServiceResponse"/>.</param>
		/// <remarks>Existing logs are not overridden.</remarks>
		public void SetLogs(Dictionary<LogLevels, List<Log>> logs)
		{
			if (logs == null)
				throw new ArgumentNullException(nameof(logs));

			if (logs.ContainsKey(LogLevels.Info) && logs[LogLevels.Info] != null && logs[LogLevels.Info].Any())
				Infos.AddRange(logs[LogLevels.Info]);

			if (logs.ContainsKey(LogLevels.Debug) && logs[LogLevels.Debug] != null && logs[LogLevels.Debug].Any())
				Debugs.AddRange(logs[LogLevels.Debug]);

			if (logs.ContainsKey(LogLevels.Warning) && logs[LogLevels.Warning] != null && logs[LogLevels.Warning].Any())
				Warnings.AddRange(logs[LogLevels.Warning]);

			if (logs.ContainsKey(LogLevels.Error) && logs[LogLevels.Error] != null && logs[LogLevels.Error].Any())
				Errors.AddRange(logs[LogLevels.Error]);

			if (logs.ContainsKey(LogLevels.Fatal) && logs[LogLevels.Fatal] != null && logs[LogLevels.Fatal].Any())
				Exceptions.AddRange(logs[LogLevels.Fatal]);
		}

		/// <summary>
		/// Adds the specified <paramref name="log"/> to the current <see cref="ServiceResponse"/>.
		/// </summary>
		/// <param name="log">The <see cref="Log"/> to add to the current <see cref="ServiceResponse"/>.</param>
		public void Add(Log log)
		{
			if (log == null)
				throw new ArgumentNullException(nameof(log));

			switch (log.Level)
			{
				case LogLevels.Info: Infos.Add(log); break;
				case LogLevels.Debug: Debugs.Add(log); break;
				case LogLevels.Warning: Warnings.Add(log); break;
				case LogLevels.Error: Errors.Add(log); break;
				case LogLevels.Fatal: Exceptions.Add(log); break;
			}
		}

		/// <summary>
		/// Adds the specified Logs to the current <see cref="ServiceResponse"/>.
		/// </summary>
		/// <param name="logs">The Logs to add to the current <see cref="ServiceResponse"/>.</param>
		public void Add(IEnumerable<Log> logs)
		{
			if (logs == null)
				throw new ArgumentNullException(nameof(logs));

			foreach (var log in logs)
				Add(log);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="responseParams"></param>
		public void Add(ResponseParams responseParams)
		{
			if (responseParams == null)
				throw new ArgumentNullException(nameof(responseParams));

			Add(responseParams.Logs);
		}

		/// <summary>
		/// Adds the specified exception to the current <see cref="Exceptions"/> list.
		/// </summary>
		/// <param name="source">The source of the log.</param>
		/// <param name="exception">The exception to add.</param>
		/// <param name="message">A message for the log.</param>
		/// <param name="writeLog">TRUE to write the log in the log file; otherwise FALSE.</param>
		public void Add(Type source, Exception exception, string message = "Une erreur est survenue", bool writeLog = false)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));

			var log = new TechnicalLog(source, LogLevels.Fatal, message, exception);
			if (writeLog)
				log.Write();

			Add(log);
		}

		public List<string> GetLogsMessages(params LogLevels[] filters)
		{
			var messages = new List<string>();
			if (filters == null)
				return messages;

			if (filters.Contains(LogLevels.Fatal))
				messages.AddRange(Exceptions != null ? Exceptions.Select(e => e.Message) : new List<string>());

			if (filters.Contains(LogLevels.Error))
				messages.AddRange(Errors != null ? Errors.Select(e => e.Message) : new List<string>());

			if (filters.Contains(LogLevels.Warning))
				messages.AddRange(Warnings != null ? Warnings.Select(e => e.Message) : new List<string>());

			if (filters.Contains(LogLevels.Info))
				messages.AddRange(Infos != null ? Infos.Select(e => e.Message) : new List<string>());

			if (filters.Contains(LogLevels.Debug))
				messages.AddRange(Debugs != null ? Debugs.Select(e => e.Message) : new List<string>());

			return messages;
		}

		public bool IsAnyLogs()
		{
			return GetLogsMessages(LogLevels.Fatal, LogLevels.Error, LogLevels.Warning, LogLevels.Info, LogLevels.Debug).Any();
		}

		public bool IsAnyLogs(params LogLevels[] filters)
		{
			return GetLogsMessages(filters).Any();
		}

		#endregion
	}
}
