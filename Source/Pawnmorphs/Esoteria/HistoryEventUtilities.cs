// HistoryEventUtilities.cs created by Iron Wolf for Pawnmorph on 07/21/2021 4:33 PM
// last updated 07/21/2021  4:33 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// 
	/// </summary>
	[StaticConstructorOnStartup]
	public static class HistoryEventUtilities
	{

		/// <summary>
		/// Gets all custom events in this mod 
		/// </summary>
		/// <value>
		/// All custom events.
		/// </value>
		[NotNull]
		public static IReadOnlyList<HistoryEventDef> AllCustomEvents { get; }

		static HistoryEventUtilities()
		{
			var mod = PMHistoryEventDefOf.MutationLost.modContentPack;


			AllCustomEvents = DefDatabase<HistoryEventDef>.AllDefsListForReading.Where(h => h.modContentPack == mod).ToList();
		}


		/// <summary>
		/// Converts an enum to named argument .
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumVal">The enum value.</param>
		/// <returns></returns>
		public static NamedArgument ToNamedArgument<T>(this T enumVal) where T : struct, Enum
		{
			return enumVal.Named(typeof(T).Name);
		}

		/// <summary>
		/// converts an enum to a named arguments.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumVal">The enum value.</param>
		/// <param name="label">The label.</param>
		/// <returns></returns>
		public static NamedArgument ToNamedArgument<T>(this T enumVal, string label) where T : struct, Enum
		{
			return enumVal.Named(label);
		}

		/// <summary>
		/// Gets the enum value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="historyEvent">The history event.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">if name is not in the history event arguments</exception>
		public static T GetEnumValue<T>(this HistoryEvent historyEvent, string name) where T : struct, Enum
		{
			var sVal = (string)historyEvent.args.GetArg(name).arg;
			return (T)Enum.Parse(typeof(T), sVal);
		}


		/// <summary>
		/// Tries to get an enum value from the history event .
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="historyEvent">The history event.</param>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static bool TryGetEnumValue<T>(this HistoryEvent historyEvent, string name, out T value) where T : struct, Enum
		{
			if (!historyEvent.args.TryGetArg(name, out string sVal))
			{
				value = default(T);
				return false;
			}
			return Enum.TryParse(sVal, out value);
		}

		/// <summary>
		/// Sends a new history event with the given history def .
		/// </summary>
		/// <param name="def">The definition.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">def</exception>
		public static HistoryEvent SendEvent([NotNull] this HistoryEventDef def)
		{
			if (def == null) throw new ArgumentNullException(nameof(def));
			var historyEvent = new HistoryEvent(def);
			Find.HistoryEventsManager.RecordEvent(historyEvent);
			return historyEvent;

		}

		/// <summary>
		/// Sends a new history event with the given history def .
		/// </summary>
		/// <param name="def">The definition.</param>
		/// <param name="arg1">The arg1.</param>
		/// <exception cref="ArgumentNullException">def</exception>
		public static HistoryEvent SendEvent([NotNull] this HistoryEventDef def, NamedArgument arg1)
		{
			if (def == null) throw new ArgumentNullException(nameof(def));
			var historyEvent = new HistoryEvent(def, arg1);
			Find.HistoryEventsManager.RecordEvent(historyEvent);
			return historyEvent;
		}

		/// <summary>
		/// Sends a new history event with the given history def .
		/// </summary>
		/// <param name="def">The definition.</param>
		/// <param name="arg1">The arg1.</param>
		/// <param name="arg2">The arg2.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">def</exception>
		public static HistoryEvent SendEvent([NotNull] this HistoryEventDef def, NamedArgument arg1, NamedArgument arg2)
		{
			if (def == null) throw new ArgumentNullException(nameof(def));
			var historyEvent = new HistoryEvent(def, arg1, arg2);
			Find.HistoryEventsManager.RecordEvent(historyEvent);
			return historyEvent;
		}

		/// <summary>
		/// Sends a new history event with the given history def .
		/// </summary>
		/// <param name="def">The definition.</param>
		/// <param name="arg1">The arg1.</param>
		/// <param name="arg2">The arg2.</param>
		/// <param name="arg3">The arg3.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">def</exception>
		public static HistoryEvent SendEvent([NotNull] this HistoryEventDef def, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
		{
			if (def == null) throw new ArgumentNullException(nameof(def));
			var historyEvent = new HistoryEvent(def, arg1, arg2, arg3);
			Find.HistoryEventsManager.RecordEvent(historyEvent);
			return historyEvent;
		}

		/// <summary>
		/// Sends a new history event with the given history def .
		/// </summary>
		/// <param name="def">The definition.</param>
		/// <param name="namedArgs">The named arguments.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">def</exception>
		public static HistoryEvent SendEvent([NotNull] this HistoryEventDef def, params NamedArgument[] namedArgs)
		{
			if (def == null) throw new ArgumentNullException(nameof(def));
			var historyEvent = new HistoryEvent(def) { args = new SignalArgs(namedArgs) };

			Find.HistoryEventsManager.RecordEvent(historyEvent);
			return historyEvent;

		}


		/// <summary>
		/// Gets the argument.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ev">The ev.</param>
		/// <param name="label">The label.</param>
		/// <returns></returns>
		public static T GetArg<T>(this HistoryEvent ev, string label)
		{
			return ev.args.GetArg<T>(label);
		}

		/// <summary>
		/// Tries to get the given argument.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ev">The ev.</param>
		/// <param name="label">The label.</param>
		/// <param name="val">The value.</param>
		/// <returns></returns>
		public static bool TryGetArg<T>(this HistoryEvent ev, string label, out T val)
		{
			return ev.args.TryGetArg(label, out val);
		}


		/// <summary>
		/// Gets the doer. the pawn the event pertains 
		/// </summary>
		/// <param name="ev">The ev.</param>
		/// <returns></returns>
		public static Pawn GetDoer(this HistoryEvent ev)
		{
			return ev.args.GetArg<Pawn>(HistoryEventArgsNames.Doer);
		}
	}
}