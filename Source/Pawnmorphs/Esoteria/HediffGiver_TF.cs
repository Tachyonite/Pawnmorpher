using System.Collections.Generic;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// hediff giver that tries to transform a pawn
    /// </summary>
    /// <seealso cref="Verse.HediffGiver" />
    public class HediffGiver_TF : HediffGiver
    {
        /// The pawnKind of the animal to be transformed into.
        public List<PawnKindDef> pawnkinds;
        /// Tale to add to the tales.
        public TaleDef tale;
        /// The gender that will be forced (i.e. a ChookMorph will be forced female).
        public TFGender forceGender = TFGender.Original;
        /// If forceGender is provided, this is the chance the gender will be forced.
        public float forceGenderChance = 50f; 
        
        private float changeChance = -1;

        /// <summary>Tries to transform the pawn</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="cause">The cause.</param>
        /// <returns></returns>
        public bool TryTf(Pawn pawn, Hediff cause)
        {
            RandUtilities.PushState();

            float chance = changeChance < 0 // If changeChance wasn't overriden use the default from the settings.
                ? LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance
                : changeChance;

            bool changed = false;

            if (Rand.Range(0, 100) < chance)
            {
                changed = TransformPawn(pawn, cause);
            }

            RandUtilities.PopState();

            return changed;
        }
        /// <summary>Transforms the pawn.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="cause">The cause.</param>
        /// <returns></returns>
        public bool TransformPawn(Pawn pawn, Hediff cause)
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

            return inst != null;
        }
    }
}