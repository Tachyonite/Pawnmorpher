using Pawnmorph.DefExtensions;
using Pawnmorph.Interfaces;
using Pawnmorph.TfSys;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Verbs
{
	internal class Verb_MeleeApplyMutagenicHediff : Verb_MeleeAttack
	{
		protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
		{
			DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
			if (tool == null)
			{
				Log.ErrorOnce("Attempted to apply melee hediff without a tool", 38381735);
				return damageResult;
			}
			if (!(target.Thing is Pawn pawn))
			{
				Log.ErrorOnce("Attempted to apply melee hediff without pawn target", 78330053);
				return damageResult;
			}
			foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, verbProps.bodypartTagTarget))
			{
				Hediff hediff = HediffMaker.MakeHediff(tool.hediff, pawn, notMissingPart);

				if (hediff is ICaused caused)
				{
					MutagenDef mutagen = maneuver.GetModExtension<MutagenExtension>()?.mutagen;
					if (mutagen != null)
						caused.Causes.Add(MutationCauses.MUTAGEN_PREFIX, mutagen);
				}

				pawn.health.AddHediff(hediff);
				damageResult.AddHediff(hediff);
				damageResult.AddPart(pawn, notMissingPart);
				damageResult.wounded = true;
			}
			return damageResult;
		}

		public override bool IsUsableOn(Thing target)
		{
			return target is Pawn;
		}
	}
}
