// Giver_ChaomorphTf.cs created by Iron Wolf for Pawnmorph on 08/16/2021 12:32 PM
// last updated 08/16/2021  12:32 PM

using JetBrains.Annotations;
using Pawnmorph.TfSys;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff giver that transforms a pawn into a chaomorph using the chaomorph extension 
    /// </summary>
    /// <seealso cref="Verse.HediffGiver" />
    /// <seealso cref="Pawnmorph.IPawnTransformer" />
    public class Giver_ChaomorphTf : HediffGiver, IPawnTransformer
    {
        /// Tale to add to the tales.
        public TaleDef tale;
        /// The gender that will be forced (i.e. a ChookMorph will be forced female).
        public TFGender forceGender = TFGender.Original;
        /// If forceGender is provided, this is the chance the gender will be forced.
        public float forceGenderChance = 50f;

        private float changeChance = -1;

        /// <summary>
        /// The type of chaomorph to choose from 
        /// </summary>
        public ChaomorphType type = ChaomorphType.Chaomorph; 


        /// <summary>Tries to transform the pawn</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="cause">The cause.</param>
        /// <returns></returns>
        bool IPawnTransformer.TryTransform(Pawn pawn, [CanBeNull] Hediff cause)
        {
            float chance = changeChance < 0 // If changeChance wasn't overriden use the default from the settings.
                               ? LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance
                               : changeChance;

            //apply the new stat 

            chance = Mathf.Clamp(chance * pawn.GetStatValue(PMStatDefOf.TransformationSensitivity), 0, 100);

            bool changed = false;

            if (Rand.Range(0, 100) < chance)
            {
                changed = TransformPawn(pawn, cause);
            }

            return changed;
        }

        

        /// <summary>Transforms the pawn.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="cause">The cause.</param>
        /// <returns></returns>
        public bool TransformPawn(Pawn pawn, [CanBeNull] Hediff cause)
        {
            var mutagen = cause?.def?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;

            var pk = ChaomorphUtilities.GetRandomChaomorphPK(type);
            if (pk == null)
            {
                Log.Error($"unable to get random chaomorph for {pawn.Name} using type {type}!");
                return false; 
            }
            var request = new TransformationRequest(pk, pawn)
            {
                forcedGender = forceGender,
                forcedGenderChance = forceGenderChance,
                cause = cause,
                tale = tale,
                transformedTick = Find.TickManager?.TicksAbs
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