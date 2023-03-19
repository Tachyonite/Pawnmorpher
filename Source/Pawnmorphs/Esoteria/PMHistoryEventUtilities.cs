// PMHistoryEventUtilities.cs created by Iron Wolf for Pawnmorph on 03/07/2022 3:32 PM
// last updated 03/07/2022  3:32 PM

using System;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// static class for history related utilities
	/// </summary>
	public static class PMHistoryEventUtilities
	{

		/// <summary>
		/// determine if the given doer is willing to do this event def.
		/// </summary>
		/// <param name="eventDef">The event definition.</param>
		/// <param name="doer">The doer.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// eventDef
		/// or
		/// doer
		/// </exception>
		public static bool DoerWillingToDo([NotNull] this HistoryEventDef eventDef, [NotNull] Pawn doer)
		{
			if (eventDef == null) throw new ArgumentNullException(nameof(eventDef));
			if (doer == null) throw new ArgumentNullException(nameof(doer));

			var ideo = doer.Ideo;
			if (ideo == null) return true;
			var hEvent = new HistoryEvent(eventDef, doer.Named(HistoryEventArgsNames.Doer));
			return ideo.MemberWillingToDo(hEvent);
		}



	}



}