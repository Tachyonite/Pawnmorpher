// SyringeRifleTf.cs created by Iron Wolf for Pawnmorph on 05/17/2020 7:39 PM
// last updated 05/17/2020  7:39 PM

using System.Linq;
using Pawnmorph.TfSys;
using Pawnmorph.ThingComps;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Verse.Hediff" />
    public class SyringeRifleTf : Hediff
    {
        private int? _lastStage;

        private Thing _weapon;

        /// <summary>
        /// Posts the add.
        /// </summary>
        /// <param name="dinfo">The dinfo.</param>
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            _weapon = (dinfo?.Instigator as Pawn)?.equipment?.Primary; 
        }

        /// <summary>
        /// Ticks this instance.
        /// </summary>
        public override void Tick()
        {

            base.Tick();
            if (_lastStage != CurStageIndex)
            {
                _lastStage = CurStageIndex;
                if (CurStageIndex == def.stages.Count - 1)
                {
                    DoTf(); 
                }
            }
        }

        private void DoTf()
        {
            PawnKindDef pawnKind = _weapon?.TryGetComp<AnimalSelectorComp>()?.ChosenKind;
            if (pawnKind == null)
            {
                pawnKind = DefDatabase<PawnKindDef>.AllDefs.Where(p => p.RaceProps.Animal).RandomElement();
            }

            var tfRequest = new TransformationRequest(pawnKind, pawn);
            MutagenDefOf.defaultMutagen.MutagenCached.Transform(tfRequest); 

        }


        /// <summary>
        /// Exposes the data.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref _lastStage, "lastStage");
            Scribe_References.Look(ref _weapon, "weapon");
            base.ExposeData();
        }
    }
}