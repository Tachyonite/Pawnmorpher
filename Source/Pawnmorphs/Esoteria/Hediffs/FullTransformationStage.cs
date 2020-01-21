// FullTransformationStage.cs modified by Iron Wolf for Pawnmorph on 01/13/2020 5:26 PM
// last updated 01/13/2020  5:26 PM

using System;
using System.Collections.Generic;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff stage that controls
    /// </summary>
    /// <seealso cref="Pawnmorph.IPawnTransformer" />
    /// <seealso cref="Verse.HediffStage" />
    /// <seealso cref="Pawnmorph.Hediffs.IExecutableStage" />
    public class FullTransformationStage : HediffStage, IExecutableStage, IPawnTransformer, IInitializable
    {

        /// The pawnKind of the animal to be transformed into.
        public List<PawnKindDef> pawnkinds;
        /// Tale to add to the tales.
        public TaleDef transformationTale;
        /// The gender that will be forced (i.e. a ChookMorph will be forced female).
        public TFGender forceGender = TFGender.Original;
        /// If forceGender is provided, this is the chance the gender will be forced.
        public float forceGenderChance = 50f;

        private float changeChance = -1;

        /// <summary>Tries to transform the pawn</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="cause">The cause.</param>
        /// <returns></returns>
        public bool TryTransform(Pawn pawn, Hediff cause)
        {
            RandUtilities.PushState();
            bool changed = false; 
            try
            {
                float chance = changeChance < 0 // If changeChance wasn't overriden use the default from the settings.
                                   ? LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance
                                   : changeChance;

                //apply the new stat 
                chance *= pawn.GetStatValue(PMStatDefOf.TransformationSensitivity);
                chance = Mathf.Clamp(chance, 0, 100);


                if (Rand.Range(0, 100) < chance) changed= TransformPawn(pawn, cause);
            }
            finally
            {
                RandUtilities.PopState();
            }

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
                tale = transformationTale
            };

            var inst = mutagen.MutagenCached.Transform(request);

            if (inst != null)
            {
                var comp = Find.World.GetComponent<PawnmorphGameComp>();
                comp.AddTransformedPawn(inst);
            }

            return inst != null;
        }

        /// <summary>called when the given hediff enters this stage</summary>
        /// <param name="hediff">The hediff.</param>
        public void EnteredStage(Hediff hediff)
        {
            TryTransform(hediff.pawn, hediff); 
        }

        /// <summary>
        /// Gets all Configuration errors in this instance.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ConfigErrors()
        {
            if (pawnkinds.NullOrEmpty()) yield return "no pawnkinds set"; 
        }
    }
}