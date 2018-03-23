#region Namespace Sonata.Web.Services
//	TODO
#endregion

using log4net.Core;
using Microsoft.Extensions.Logging;
using Sonata.Diagnostics.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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
		/// Gets a list of <see cref="LogLevel.Information"/> <see cref="ILog4NetProperties"/> generated during the server process.
		/// </summary>
		[DataMember(Name = "infos")]
		public List<ILog4NetProperties> Infos { get; set; }

		/// <summary>
		/// Gets a list of <see cref="LogLevel.Debug"/> and <see cref="LogLevel.Trace"/> <see cref="ILog4NetProperties"/> generated during the server process.
		/// </summary>
		[DataMember(Name = "debugs")]
		public List<ILog4NetProperties> Debugs { get; set; }

		/// <summary>
		/// Gets a list of <see cref="LogLevel.Warning"/> <see cref="ILog4NetProperties"/> generated during the server process.
		/// </summary>
		[DataMember(Name = "warnings")]
		public List<ILog4NetProperties> Warnings { get; set; }

		/// <summary>
		/// Gets a list of <see cref="LogLevel.Error"/> <see cref="ILog4NetProperties"/> generated during the server process.
		/// </summary>
		[DataMember(Name = "errors")]
		public List<ILog4NetProperties> Errors { get; set; }

		/// <summary>
		/// Gets a list of <see cref="LogLevel.Critical"/> <see cref="ILog4NetProperties"/> that occured during the server process.
		/// </summary>
		[DataMember(Name = "exceptions")]
		public List<ILog4NetProperties> Exceptions { get; set; }

		/// <summary>
		/// Gets a value indicating whether the current <see cref="ServiceResponse"/> contains <see cref="LogLevel.Information"/> <see cref="ILog4NetProperties"/>.
		/// </summary>
		[DataMember(Name = "isInfos")]
		public bool IsInfos
		{
			get => _isInfos || Infos.Any();
			set => _isInfos = value;
		}

		/// <summary>
		/// Gets a value indicating whether the current <see cref="ServiceResponse"/> contains <see cref="LogLevel.Debug"/> <see cref="ILog4NetProperties"/>.
		/// </summary>
		[DataMember(Name = "isDebugs")]
		public bool IsDebugs
		{
			get => _isDebugs || Debugs.Any();
			set => _isDebugs = value;
		}

		/// <summary>
		/// Gets a value indicating whether the current <see cref="ServiceResponse"/> contains <see cref="LogLevel.Warning"/> <see cref="ILog4NetProperties"/>.
		/// </summary>
		[DataMember(Name = "isWarnings")]
		public bool IsWarnings
		{
			get => _isWarnings || Warnings.Any();
			set => _isWarnings = value;
		}

		/// <summary>
		/// Gets a value indicating whether the current <see cref="ServiceResponse"/> contains <see cref="LogLevel.Error"/> <see cref="ILog4NetProperties"/>.
		/// </summary>
		[DataMember(Name = "isErrors")]
		public bool IsErrors
		{
			get => _isErrors || Errors.Any();
			set => _isErrors = value;
		}

		/// <summary>
		/// Gets a value indicating whether the current <see cref="ServiceResponse"/> contains <see cref="LogLevel.Critical"/> <see cref="ILog4NetProperties"/>.
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
			Infos = new List<ILog4NetProperties>();
			Debugs = new List<ILog4NetProperties>();
			Warnings = new List<ILog4NetProperties>();
			Errors = new List<ILog4NetProperties>();
			Exceptions = new List<ILog4NetProperties>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds the specified <paramref name="logs"/> to the current <see cref="ServiceResponse"/>.
		/// </summary>
		/// <param name="logs">A list of <see cref="ILog4NetProperties"/> to add to the current <see cref="ServiceResponse"/>.</param>
		/// <remarks>Existing logs are not overridden.</remarks>
		public void SetLogs(Dictionary<LogLevel, List<ILog4NetProperties>> logs)
		{
			if (logs == null)
				throw new ArgumentNullException(nameof(logs));

			if (logs.ContainsKey(LogLevel.Information) && logs[LogLevel.Information] != null && logs[LogLevel.Information].Any())
				Infos.AddRange(logs[LogLevel.Information]);

			if (logs.ContainsKey(LogLevel.Debug) && logs[LogLevel.Debug] != null && logs[LogLevel.Debug].Any())
				Debugs.AddRange(logs[LogLevel.Debug]);

			if (logs.ContainsKey(LogLevel.Warning) && logs[LogLevel.Warning] != null && logs[LogLevel.Warning].Any())
				Warnings.AddRange(logs[LogLevel.Warning]);

			if (logs.ContainsKey(LogLevel.Error) && logs[LogLevel.Error] != null && logs[LogLevel.Error].Any())
				Errors.AddRange(logs[LogLevel.Error]);

			if (logs.ContainsKey(LogLevel.Critical) && logs[LogLevel.Critical] != null && logs[LogLevel.Critical].Any())
				Exceptions.AddRange(logs[LogLevel.Critical]);
		}

		/// <summary>
		/// Adds the specified <paramref name="log"/> to the current <see cref="ServiceResponse"/>.
		/// </summary>
		/// <param name="log">The <see cref="ILog4NetProperties"/> to add to the current <see cref="ServiceResponse"/>.</param>
		public void Add(ILog4NetProperties log)
		{
			if (log == null)
				throw new ArgumentNullException(nameof(log));

			if (log.Level == Level.Trace || log.Level == Level.Debug)
				Debugs.Add(log);

			if (log.Level == Level.Info)
				Infos.Add(log);

			if (log.Level == Level.Warn)
				Warnings.Add(log);

			if (log.Level == Level.Error)
				Errors.Add(log);

			if (log.Level == Level.Critical)
				Exceptions.Add(log);
		}

		/// <summary>
		/// Adds the specified Logs to the current <see cref="ServiceResponse"/>.
		/// </summary>
		/// <param name="logs">The Logs to add to the current <see cref="ServiceResponse"/>.</param>
		public void Add(IEnumerable<ILog4NetProperties> logs)
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
		public void Add(Type source, Exception exception, string message = "Une erreur est survenue")
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));

			var log = new Log4NetProperties
			{
				Level = Level.Critical,
				Source = source.FullName,
				Message = message,
				Exception = exception
			};

			Add(log);
		}

		public List<string> GetLogsMessages(params LogLevel[] filters)
		{
			var messages = new List<string>();
			if (filters == null)
				return messages;

			if (filters.Contains(LogLevel.Critical))
				messages.AddRange(Exceptions != null ? Exceptions.Select(e => e.Message) : new List<string>());

			if (filters.Contains(LogLevel.Error))
				messages.AddRange(Errors != null ? Errors.Select(e => e.Message) : new List<string>());

			if (filters.Contains(LogLevel.Warning))
				messages.AddRange(Warnings != null ? Warnings.Select(e => e.Message) : new List<string>());

			if (filters.Contains(LogLevel.Information))
				messages.AddRange(Infos != null ? Infos.Select(e => e.Message) : new List<string>());

			if (filters.Contains(LogLevel.Debug))
				messages.AddRange(Debugs != null ? Debugs.Select(e => e.Message) : new List<string>());

			return messages;
		}

		public bool IsAnyLogs()
		{
			return GetLogsMessages(LogLevel.Critical, LogLevel.Error, LogLevel.Warning, LogLevel.Information, LogLevel.Debug).Any();
		}

		public bool IsAnyLogs(params LogLevel[] filters)
		{
			return GetLogsMessages(filters).Any();
		}

		#endregion
	}
}
