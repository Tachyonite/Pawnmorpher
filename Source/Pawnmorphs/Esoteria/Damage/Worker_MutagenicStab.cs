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
            for (BodyPartRecord bodyPartRecord = dInfo.HitPart; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
            {
                bodyPartRecordList.Add(bodyPartRecord);
                if (bodyPartRecord.depth == BodyPartDepth.Outside)
                    break;
            }

            foreach (BodyPartRecord forceHitPart in bodyPartRecordList)
            {
                float totalDamage1 = bodyPartRecordList.Count != 1
                                         ? forceHitPart.depth != BodyPartDepth.Outside ? totalDamage * 0.4f : totalDamage * 0.75f
                                         : totalDamage;
                DamageInfo dinfo1 = dInfo;
                dinfo1.SetHitPart(forceHitPart);

                if (Rand.Range(0, 1f) < 0.3f)
                    AddMutationOn(forceHitPart, pawn); 


                float num = FinalizeAndAddInjury(pawn, totalDamage1, dinfo1, result);
            }
        }

        private void AddMutationOn(BodyPartRecord forceHitPart, Pawn pawn)
        {
            while (forceHitPart != null)
            {
                var giver = MutationUtilities.GetMutationsFor(forceHitPart.def).RandomElementWithFallback();
                if (giver == null)
                {
                    forceHitPart = forceHitPart.parent;//go upward until we hit a mutable part 
                    continue; 
                }

                if (!giver.TryApply(pawn, forceHitPart))
                {
                    forceHitPart = forceHitPart.parent;//go upward until we hit a mutable part  
                    continue;
                }
                break; //if we apply a mutation break the loop 

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