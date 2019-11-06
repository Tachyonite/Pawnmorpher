// Worker_MutagenicBlunt.cs created by Iron Wolf for Pawnmorph on 11/06/2019 1:55 PM
// last updated 11/06/2019  1:55 PM

using System;
using Verse;

namespace Pawnmorph.Damage
{
    /// <summary>
    /// damage worker for mutagenic blunt damage
    /// </summary>
    /// <seealso cref="Verse.DamageWorker_Blunt" />
    public class Worker_MutagenicBlunt : DamageWorker_Blunt
    {
        /// <summary>Applies the specified damage to the given thing.</summary>
        /// <param name="dinfo">The damage info.</param>
        /// <param name="thing">The thing.</param>
        /// <returns></returns>
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            if (thing is Pawn pawn && MutagenDefOf.defaultMutagen.CanInfect(pawn))
            {

                return ApplyToPawn(dinfo, pawn); 


            }
            else
            {
                return base.Apply(dinfo, thing); 
            }
        }

        DamageResult ApplyToPawn(DamageInfo dinfo, Pawn pawn)
        {

            var oDamage = dinfo.Amount;
            dinfo = MutagenicDamageUtilities.ReduceDamage(dinfo);
            var res = base.Apply(dinfo, pawn);
            MutagenicDamageUtilities.ApplyMutagenicDamage(oDamage, dinfo, pawn, res);
            return res; 
        }
    }
}