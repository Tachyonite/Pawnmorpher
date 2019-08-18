using System.Collections.Generic;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class HediffGiver_TF : HediffGiver
    {
        //[Unsaved] private readonly Dictionary<Pawn, bool> _sawDict = new Dictionary<Pawn, bool>();

        public List<PawnKindDef> pawnkinds; // The pawnKind of the animal to be transformed into.
        public TaleDef tale; // Tale to add to the tales.

        public TFGender
            forceGender = TFGender.Original; // The gender that will be forced (i.e. a ChookMorph will be forced female).

        public float forceGenderChance = 50f; // If forceGender is provided, this is the chance the gender will be forced.

        private float changeChance = -1;
        


        public bool TryTf(Pawn pawn, Hediff cause)
        {
            float chance = changeChance < 0 //if changeChance wasn't overriden use the default from the settings 
                ? LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance
                : changeChance;
            //Log.Message($"{cause.def} tf is being triggered with prob of {chance}");

            

            if (Rand.RangeInclusive(0, 100) < chance)
            {
                var hediffMorph = (cause as Hediff_Morph);
                var mutagen = hediffMorph?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;

                var request = new TransformationRequest(pawnkinds.RandElement(), pawn)
                {
                    forcedGender = forceGender,
                    forcedGenderChance = forceGenderChance,
                    cause = hediffMorph,
                    tale = tale
                };

                var inst = mutagen.MutagenCached.Transform(request);


                if (inst != null)
                {
                    var comp = Find.World.GetComponent<PawnmorphGameComp>();
                    comp.AddTransformedPawn(inst); 
                }


                //TransformerUtility.Transform(pawn, cause, hediff, pawnkinds, tale, forceGender, forceGenderChance); 
                return true; 
            }

            return false; 


        }

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)

        {
            //if (!triggered
            // && Rand.RangeInclusive(0, 100)
            // <= LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance)
            //    TransformerUtility.Transform(pawn, cause, hediff, pawnkinds, tale, forceGender, forceGenderChance);
            //else
            //    triggered = true;

            //_sawDict[pawn] = true;
            //empty 
        }
    }
}