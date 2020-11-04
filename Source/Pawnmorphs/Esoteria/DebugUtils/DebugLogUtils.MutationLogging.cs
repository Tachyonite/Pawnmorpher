// DebugLogUtils.MutationLogging.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 03/01/2020  6:57 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph.DebugUtils
{
#pragma warning disable 1591

    public static partial class DebugLogUtils
    {
        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        private static void LogAllMutationInfo()
        {
            var builder = new StringBuilder();
            foreach (MutationDef allMutation in MutationDef.AllMutations)
            {
                builder.AppendLine(allMutation.ToStringFull()); 
            }

            Log.Message(builder.ToString());
        }

        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        static void GetRaceMaxInfluence()
        {
            StringBuilder builder = new StringBuilder(); 
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(t => t.race?.body != null))
            {
                builder.AppendLine($"{thingDef.defName}/{thingDef.label}={MorphUtilities.GetMaxInfluenceOfRace(thingDef)}"); 
            }

            Log.Message(builder.ToString()); 
        }


        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        static void CheckForSeverityPerDay()
        {
            StringBuilder builder = new StringBuilder();



            foreach (MutationDef mutation in MutationDef.AllMutations)
            {
                if (!mutation.HasComp(typeof(Comp_MutationSeverityAdjust)))
                {
                    builder.AppendLine($"{mutation.defName} does not have a {nameof(Comp_MutationSeverityAdjust)} comp!");
                }
            }

            if (builder.Length == 0) Log.Message("All mutations have a severity per day comp");
            else Log.Message(builder.ToString()); 
        }

        
        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        static void GetMissingSlotsPerMorph()
        {
            var allPossibleSites = DefDatabase<MutationDef>
                                  .AllDefs.SelectMany(m => m.GetAllMutationSites(BodyDefOf.Human))
                                  .Distinct()
                                  .ToList();

            List<MutationSite> scratchList = new List<MutationSite>();
            List<MutationSite> missingScratch = new List<MutationSite>(); 
            StringBuilder builder = new StringBuilder(); 

            foreach (MorphDef morphDef in MorphDef.AllDefs)
            {
                var allMorphSites = morphDef.AllAssociatedMutations.SelectMany(m => m.GetAllMutationSites(BodyDefOf.Human))
                                            .Distinct(); 

                scratchList.Clear();
                scratchList.AddRange(allMorphSites);
                missingScratch.Clear();
                var missing = allPossibleSites.Where(m => !scratchList.Contains(m));
                missingScratch.AddRange(missing);

                if (missingScratch.Count == 0)
                {
                    builder.AppendLine($"{morphDef.defName} has all possible mutations"); 
                }
                else
                {
                    float maxInfluence = ((float) scratchList.Count) / MorphUtilities.GetMaxInfluenceOfRace(ThingDefOf.Human); 
                    builder.AppendLine($"{morphDef.defName}: {nameof(maxInfluence)}={maxInfluence.ToStringByStyle(ToStringStyle.PercentOne)}");
                    var grouping = missingScratch.GroupBy(g => g.Layer, g=> g.Record);
                    foreach (var group in grouping)
                    {
                        builder.AppendLine($"-\t{group.Key}: [{group.Select(s => s.def).Distinct().Join(s => s?.defName ?? "NULL")}]"); 
                    }
                }
            }
            Log.Message(builder.ToString());

        }

        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        static void LogMutationsPerSlot()
        {
            List<MutationDef> allMutations = DefDatabase<MutationDef>.AllDefs.ToList();
            List<BodyPartDef> allValidParts = allMutations.SelectMany(m => m.parts).Distinct().ToList();
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Mutations by body part:");
            foreach (BodyPartDef part in allValidParts)
            {
                builder.AppendLine($"--{part}--");
                foreach (MutationLayer layer in Enum.GetValues(typeof(MutationLayer)))
                {
                    List<MutationDef> mutationsOnLayer = allMutations.Where(m => m.parts.Contains(part) && m.RemoveComp.layer == layer).ToList();
                    if (mutationsOnLayer.Count > 0) builder.AppendLine($"{layer} ({mutationsOnLayer.Count}): {string.Join(", ", mutationsOnLayer)}");
                }
            }
            Log.Message(builder.ToString());
        }

        [DebugOutput(category = MAIN_CATEGORY_NAME, onlyWhenPlaying = true)]
        static void LogMutationSensitivity()
        {
            StringBuilder builder = new StringBuilder();

            foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsAndPrisoners)
            {
                var mStat = pawn.GetStatValue(PMStatDefOf.MutagenSensitivity);
                var mFStat = pawn.GetMutagenicBuildupMultiplier();

                builder.AppendLine($"{pawn.Name} = {{{PMStatDefOf.MutagenSensitivity}:{mStat}, MutagenicBuildupMultiplier:{mFStat} }}"); 

            }

            Log.Message(builder.ToString()); 

        }


        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        private static void LogMutationValue()
        {
            var builder = new StringBuilder();
            foreach (MutationDef mutation in DefDatabase<MutationDef>.AllDefs)
                builder.AppendLine($"{mutation.defName},{mutation.value}");

            Log.Message(builder.ToString());
        }

    }
}