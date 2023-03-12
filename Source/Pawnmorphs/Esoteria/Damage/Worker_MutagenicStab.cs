// Worker_MutagenicStab.cs created by Iron Wolf for Pawnmorph on 11/11/2019 2:00 PM
// last updated 11/11/2019  2:00 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Damage
{
	/// <summary>
	///     damage worker for a mutagenic stab attack
	/// </summary>
	/// <seealso cref="Pawnmorph.Damage.Worker_MutagenicInjury" />
	public class Worker_MutagenicStab : Worker_MutagenicInjury
	{
		/// <summary>Applies the special effects to part.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="totalDamage">The total damage.</param>
		/// <param name="dInfo">The dInfo.</param>
		/// <param name="result">The result.</param>
		protected override void ApplySpecialEffectsToPart(
			Pawn pawn,
			float totalDamage,
			DamageInfo dInfo,
			DamageResult result)
		{
			totalDamage = ReduceDamageToPreserveOutsideParts(totalDamage, dInfo, pawn);
			var bodyPartRecordList = new List<BodyPartRecord>();
			BodyPartRecord hitPart = dInfo.HitPart;
			for (BodyPartRecord bodyPartRecord = hitPart; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
			{
				bodyPartRecordList.Add(bodyPartRecord);
				if (bodyPartRecord.depth == BodyPartDepth.Outside)
					break;
			}


			float l = hitPart.def.IsSolid(hitPart, pawn.health.hediffSet.hediffs) ? 0.5f : 0.3f;

			if (Rand.Range(0, 1f) < l)
				AddMutationOn(hitPart, pawn);

			if (hitPart.depth == BodyPartDepth.Inside)
				//add extra mutagenic buildup severity 
				AddExtraBuildup(pawn, dInfo);


			foreach (BodyPartRecord forceHitPart in bodyPartRecordList)
			{
				float totalDamage1 = bodyPartRecordList.Count != 1
										 ? forceHitPart.depth != BodyPartDepth.Outside ? totalDamage * 0.4f : totalDamage * 0.75f
										 : totalDamage;
				DamageInfo dinfo1 = dInfo;
				dinfo1.SetHitPart(forceHitPart);


				float num = FinalizeAndAddInjury(pawn, totalDamage1, dinfo1, result);
			}
		}

		/// <summary>Chooses the hit part.</summary>
		/// <param name="dinfo">The dInfo.</param>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			BodyPartRecord randomNotMissingPart1 =
				pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, dinfo.Depth);
			if (randomNotMissingPart1.depth != BodyPartDepth.Inside && Rand.Chance(def.stabChanceOfForcedInternal))
			{
				BodyPartRecord randomNotMissingPart2 =
					pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, BodyPartHeight.Undefined, BodyPartDepth.Inside,
																  randomNotMissingPart1);
				if (randomNotMissingPart2 != null)
					return randomNotMissingPart2;
			}

			return randomNotMissingPart1;
		}

	}
}