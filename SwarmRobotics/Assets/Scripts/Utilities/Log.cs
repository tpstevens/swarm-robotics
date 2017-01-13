using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;

namespace Utilities
{
	public enum LogLevel
	{
		Verbose,
		Debug,
		Warning,
		Error,
		Assert
	}

	public static class LogLevelExtension
	{
		public static LogLevel level = LogLevel.Verbose;

		public static string ToPaddedString(this LogLevel level)
		{
			return string.Format("{0,-7}", level.ToString());
		}
	}

	public class LogTag
	{
		public string Value
		{
			get;
			set;
		}

		public static LogTag MAIN { get { return new LogTag("MAIN"); } }
		public static LogTag ARGS { get { return new LogTag("ARGS"); } }
		public static LogTag CONFIG { get { return new LogTag("CONFIG"); } }
		public static LogTag FILEUTILITIES { get { return new LogTag("FILE-UTILITIES"); } }
		public static LogTag ROBOTICS { get { return new LogTag("ROBOTICS"); } }

		private LogTag(string value)
		{
			this.Value = value;
		}
	}

	public sealed class Log
	{
		private class LogMessage
		{
			public readonly int tag;
			public readonly LogLevel level;
			public readonly long timeStamp;
			public readonly string message;

			public LogMessage(LogLevel level, int tag, string message)
			{
				this.level = level;
				this.tag = tag;
				this.message = message;
				timeStamp = DateTime.Now.Ticks;
			}

			public string ToFormattedString(string tagString)
			{
				DateTime dateTime = new DateTime(this.timeStamp);
				string text = level.ToString().Substring(0, 1);
				return text + " | " + dateTime.ToLongTimeString() + " | " + tagString + " | " + message;
			}
		}

		private static volatile Log log;
		private static object mutex = new object();

		private Dictionary<string, int> tagStringToInt;
		private Dictionary<int, string> tagIntToString;
		private int longestTagLength = 0;
		private int nextTagId;
		private List<LogMessage> messages;

		private Log()
		{
			messages = new List<LogMessage>();
			nextTagId = 0;
			tagStringToInt = new Dictionary<string, int>();
			tagIntToString = new Dictionary<int, string>();
		}

		public static void a(LogTag tag, string message)
		{
			write(LogLevel.Assert, tag, message);
		}

		public static void a(string tag, string message)
		{
			write(LogLevel.Assert, tag, message);
		}

		public static void d(LogTag tag, string message)
		{
			write(LogLevel.Debug, tag, message);
		}

		public static void d(string tag, string message)
		{
			write(LogLevel.Debug, tag, message);
		}

		public static void e(LogTag tag, string message)
		{
			write(LogLevel.Error, tag, message);
		}

		public static void e(string tag, string message)
		{
			write(LogLevel.Error, tag, message);
		}

		public static void w(LogTag tag, string message)
		{
			write(LogLevel.Warning, tag, message);
		}

		public static void w(string tag, string message)
		{
			write(LogLevel.Warning, tag, message);
		}

		public static void v(LogTag tag, string message)
		{
			write(LogLevel.Verbose, tag, message);
		}

		public static void v(string tag, string message)
		{
			write(LogLevel.Verbose, tag, message);
		}

		public static void clear()
		{
			log = new Log();
		}

		public static void write(LogLevel level, LogTag tag, string message)
		{
			write(level, tag.Value, message);
		}

		public static void write(LogLevel level, string tag, string message)
		{
			Log log = Instance();
			int tagId;

			message.Trim();
			tag.Trim();

			if (!log.tagStringToInt.TryGetValue(tag, out tagId))
			{
				tagId = log.nextTagId++;

				if (tag.Length > log.longestTagLength)
					log.longestTagLength = tag.Length;
				
				log.tagStringToInt.Add(tag, tagId);
				log.tagIntToString.Add(tagId, tag);
			}

			log.messages.Add(new LogMessage(level, tagId, message));

			switch (level) {
				case LogLevel.Verbose:
					Debug.Log(tag + " - " + message);
					break;
				case LogLevel.Debug:
					Debug.Log(tag + " - " + message);
					break;
				case LogLevel.Warning:
					Debug.LogWarning(tag + " - " + message);
					break;
				case LogLevel.Error:
					Debug.LogError(tag + " - " + message);
					break;
				case LogLevel.Assert:
					Debug.LogAssertion(tag + " - " + message);
					break;
			}
		}

		public static void writeToFile(string fileName)
		{
			Log log = Instance();
			string filePath = FileUtilities.buildLogPath(fileName);

			if (filePath.EndsWith(".txt"))
			{
				if (File.Exists(filePath))
					File.Delete(filePath);

				File.Create(filePath).Dispose();

				Log.d("LOG", "Writing log to " + filePath);

				foreach (LogMessage l in log.messages)
				{
					string tag;
					log.tagIntToString.TryGetValue(l.tag, out tag);
					File.AppendAllText(filePath, l.ToFormattedString(tag + new string(' ', log.longestTagLength - tag.Length)) + "\n");
				}
			}
			else
			{
				Log.e("LOG", "Filename \"" + filePath + "\" does not end in the .txt extension");
			}
		}

		private static Log Instance()
		{
			if (log == null)
			{
				lock (mutex)
				{
					log = new Log();
				}
			}

			return log;
		}
	}
}
