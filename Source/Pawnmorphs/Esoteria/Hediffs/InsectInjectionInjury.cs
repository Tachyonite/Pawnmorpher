// InsectBiteInjury.cs modified by Iron Wolf for Pawnmorph on 09/08/2019 2:37 PM
// last updated 09/08/2019  2:37 PM

using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff for the unused insect injection injury 
    /// </summary>
    /// <seealso cref="Verse.Hediff_Injury" />
    public class InsectInjectionInjury : Hediff_Injury
    {
        private float _initialSeverity; //use the initial severity to determine how likely it is to add the tf hediff         
        /// <summary>Posts the add.</summary>
        /// <param name="dinfo">The dinfo.</param>
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            _initialSeverity = Severity;
        }

        /// <summary>Exposes the data.</summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _initialSeverity, nameof(_initialSeverity), 0); 
        }
        /// <summary>Heals the specified amount.</summary>
        /// <param name="amount">The amount.</param>
        public override void Heal(float amount)
        {
            base.Heal(amount);
            var mutagen = MutagenDefOf.defaultMutagen;
            if (Severity <= 0 && mutagen.CanInfect(pawn))
            {
                TryAddHediff(); 
            }
        }

        private void TryAddHediff()
        {
            throw new System.NotImplementedException();
        }
    }
}