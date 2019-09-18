// InsectBiteInjury.cs modified by Iron Wolf for Pawnmorph on 09/08/2019 2:37 PM
// last updated 09/08/2019  2:37 PM

using Verse;

namespace Pawnmorph.Hediffs
{
    public class InsectInjectionInjury : Hediff_Injury
    {
        private float _initialSeverity; //use the initial severity to determine how likely it is to add the tf hediff 

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            _initialSeverity = Severity;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _initialSeverity, nameof(_initialSeverity), 0); 
        }

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