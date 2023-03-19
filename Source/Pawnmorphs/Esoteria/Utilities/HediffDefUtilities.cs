// HediffDefUtilities.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/11/2019 7:59 AM
// last updated 09/11/2019  7:59 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Utilities
{
	/// <summary> Collection of hediff def related utility functions. </summary>
	public static class HediffDefUtilities
	{
		/// <summary> Get all hediff givers attached to this HediffDef. </summary>
		/// <exception cref="ArgumentNullException">hediffDef is null</exception>
		[NotNull]
		public static IEnumerable<HediffGiver> GetAllHediffGivers([NotNull] this HediffDef hediffDef)
		{
			if (hediffDef == null) throw new ArgumentNullException(nameof(hediffDef));
			foreach (HediffGiver giver in hediffDef.hediffGivers ?? Enumerable.Empty<HediffGiver>()) yield return giver;

			foreach (HediffStage stage in hediffDef.stages ?? Enumerable.Empty<HediffStage>())
				foreach (HediffGiver giver in stage.hediffGivers ?? Enumerable.Empty<HediffGiver>())
					yield return giver;
		}
	}
}