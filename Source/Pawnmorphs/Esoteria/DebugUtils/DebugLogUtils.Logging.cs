// DebugLogUtils.Logging.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 02/29/2020  1:39 PM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Verse;

#pragma warning disable 1591

namespace Pawnmorph.DebugUtils
{
	public static partial class DebugLogUtils
	{
		public static bool ShouldLog(LogLevel logLevel)
		{
			//if (!Prefs.DevMode && logLevel != LogLevel.Error) return false;    

			var cLevel = PMUtilities.GetSettings().logLevel;
			return logLevel <= cLevel;
		}

		[DebuggerHidden]
		public static void LogMsg(LogLevel logLevel, string message)
		{
			if (!ShouldLog(logLevel)) return;
			switch (logLevel)
			{
				case LogLevel.Error:
					Log.Error(message);
					break;
				case LogLevel.Warnings:
					Log.Warning(message);
					break;
				case LogLevel.Messages:
				case LogLevel.Pedantic:
					Log.Message(message);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
			}
		}

		[NotNull] private static readonly HashSet<int> _usedKeys = new HashSet<int>();


		public static void WarningOnce(string message, int key)
		{
			if (_usedKeys.Contains(key)) return;
			Log.Warning(message);
			_usedKeys.Add(key);
		}


		[DebuggerHidden]
		public static void LogMsg(LogLevel logLevel, object message)
		{
			string msg;
			if (message is IDebugString bObj)
			{
				msg = bObj.ToStringFull();
			}
			else
			{
				msg = message.ToStringSafe();
			}

			LogMsg(logLevel, msg);
		}

		[DebuggerHidden, Conditional("DEBUG")]
		public static void Pedantic(string message)
		{
			LogMsg(LogLevel.Pedantic, message);
		}

		[DebuggerHidden, Conditional("DEBUG")]
		public static void Pedantic(object message)
		{
			LogMsg(LogLevel.Pedantic, message);
		}


		[DebuggerHidden]
		public static void Warning(string message)
		{
			LogMsg(LogLevel.Warnings, message);
		}

		[DebuggerHidden]
		public static void Warning(object message)
		{
			LogMsg(LogLevel.Warnings, message);
		}

		[DebuggerHidden]
		public static void Error(string message)
		{
			LogMsg(LogLevel.Error, message);
		}

		[DebuggerHidden]
		public static void Error(object message)
		{
			LogMsg(LogLevel.Error, message);
		}

		/// <summary>
		///     Asserts the specified condition. if false an error message will be displayed
		/// </summary>
		/// <param name="condition">if false will display an error message</param>
		/// <param name="message">The message.</param>
		/// <returns>the condition</returns>
		[DebuggerHidden]
		[Conditional("DEBUG")]
		[AssertionMethod]
		public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition, string message)
		{
			if (!condition) Log.Error($"assertion failed:{message}");
		}
	}
}