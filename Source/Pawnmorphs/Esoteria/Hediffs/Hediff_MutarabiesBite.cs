using System.Linq;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// The TF hediff for Mutarabies bites
    /// </summary>
    /// <seealso cref="Verse.Hediff" />
    public class Hediff_MutarabiesBite : Hediff_DynamicTf, IOnApplyViaAttack
    {
        /// <summary>
        /// Tries the merge with the other hediff
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public override bool TryMergeWith(Hediff other)
        {
            if (other is Hediff_MutarabiesBite buildup)
            {
                Severity += other.Severity;
                foreach (HediffComp hediffComp in comps)
                {
                    hediffComp.CompPostMerged(other);
                }

                ResetMutationOrder();
                return true;
            }

            return false;
        }

        /// <summary>
        /// A callback that's called when this hediff is added via an injury
        /// </summary>
        /// <param name="dinfo">the damage info of the attack that added the hediff</param>
        public void OnApplyViaAttack(DamageInfo dinfo)
        {
            ThingDef attackerDef = dinfo.Instigator?.def;
            if (attackerDef == null)
            {
                Log.Warning($"{pawn.Name} was given Mutarabies but the attacker was null, defaulting to random morphdef");
                return;
            }

            // TODO - rejigger this so we can apply the animal type even if there's not an associated morph type
            MorphDef infectionType = DefDatabase<MorphDef>.AllDefs.FirstOrDefault(m => m.race == attackerDef);
            if (infectionType != null)
            {
                MorphDef = infectionType;
            }
        }
    }
}