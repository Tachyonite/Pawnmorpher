// Worker_Hellhound.cs created by Iron Wolf for Pawnmorph on 07/29/2020 9:04 PM
// last updated 07/29/2020  9:04 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Verse;

namespace Pawnmorph.MutationRules
{
	/// <summary>
	/// mutation worker for the hellhound 
	/// </summary>
	/// <seealso cref="Pawnmorph.MutationRuleWorker" />
	public class Worker_Hellhound : MutationRuleWorker
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MutationRuleWorker" /> class.
		/// </summary>
		/// <param name="ruleDef">The rule definition.</param>
		public Worker_Hellhound([NotNull] MutationRuleDef ruleDef) : base(ruleDef)
		{
		}


		private static List<HediffDef> _morphTfs;

		[NotNull]
		static IReadOnlyList<HediffDef> MorphTfs
		{
			get
			{
				if (_morphTfs == null)
				{
					var canine = AnimalClassDefOf.Canid;

					_morphTfs = canine.GetAllMorphsInClass().Where(m => m != MorphDefOfs.PM_HellhoundMorph).Select(m => m.fullTransformation).Where(m => m != null).ToList();

				}

				return _morphTfs;
			}
		}

		/// <summary>
		///     checks if the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		protected override bool ConditionsMet(Pawn pawn)
		{
			var hediffs = pawn.health?.hediffSet;
			if (hediffs == null) return false;

			if (hediffs.GetFirstHediffOfDef(TfHediffDefOf.LuciferiumHigh) == null) return false;

			foreach (HediffDef hediffDef in MorphTfs)
			{
				if (hediffs.GetFirstHediffOfDef(hediffDef) != null) return true;
			}

			return false;
		}

		/// <summary>
		///     Does the rule on the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		protected override void DoRule(Pawn pawn)
		{
			foreach (HediffDef hediffDef in MorphTfs)
			{
				Hediff mutagenicHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(hediffDef);
				if (mutagenicHediff == null)
					continue;

				if (mutagenicHediff is Hediff_MutagenicBase mutagen)
					mutagen.MarkForRemoval();
				else if (mutagenicHediff is MorphTf morph)
					morph.MarkForRemoval();
			}

			var newHediff = HediffMaker.MakeHediff(MorphDefOfs.PM_HellhoundMorph.fullTransformation, pawn);
			pawn.health?.AddHediff(newHediff);
		}
	}
}