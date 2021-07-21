// HistoryEventUtilities.cs created by Iron Wolf for Pawnmorph on 07/21/2021 4:33 PM
// last updated 07/21/2021  4:33 PM

using System;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// 
    /// </summary>
    public static class HistoryEventUtilities
    {

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
        public static T GetEnumValue<T>(this HistoryEvent historyEvent, string name) where T : struct,Enum
        {
            var sVal = (string) historyEvent.args.GetArg(name).arg; 
            return (T) Enum.Parse(typeof(T), sVal);
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
            var historyEvent = new HistoryEvent(PMHistoryEventDefOf.SapienceLevelChanged); 
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
            var historyEvent = new HistoryEvent(PMHistoryEventDefOf.SapienceLevelChanged, arg1);
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
            var historyEvent = new HistoryEvent(PMHistoryEventDefOf.SapienceLevelChanged, arg1, arg2);
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
            var historyEvent = new HistoryEvent(PMHistoryEventDefOf.SapienceLevelChanged, arg1, arg3);
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
            var historyEvent = new HistoryEvent(PMHistoryEventDefOf.SapienceLevelChanged) {args = new SignalArgs(namedArgs)};

            Find.HistoryEventsManager.RecordEvent(historyEvent);
            return historyEvent; 

        }
    }
}