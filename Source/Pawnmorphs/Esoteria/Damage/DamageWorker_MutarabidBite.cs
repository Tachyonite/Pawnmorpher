using Verse;

namespace Pawnmorph.Damage
{
    /// <summary>
    /// Damage worker for a mutarabies-infected bite attack
    /// </summary>
    /// <seealso cref="Pawnmorph.Damage.Worker_MutagenicInjury" />
    /// <seealso cref="Verse.DamageWorker_Bite" />
    public class DamageWorker_MutarabidBite : Worker_MutagenicInjury
    {
        /// <summary>Chooses the hit part.</summary>
        /// <param name="dinfo">The dInfo.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
        {
            return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside, null);
        }
    }
}