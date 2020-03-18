// DebugLogUtils.cs created by Iron Wolf for Pawnmorph on 09/23/2019 7:54 AM
// last updated 09/27/2019  8:00 AM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using AlienRace;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph.DebugUtils
{
    [StaticConstructorOnStartup]
    public static partial class DebugLogUtils
    {
        [NotNull]
        private static MutationOutlook[] Outlooks { get; }

        static DebugLogUtils()
        {
            Outlooks = Enum.GetValues(typeof(MutationOutlook)).OfType<MutationOutlook>().ToArray(); 
        }


        public const string MAIN_CATEGORY_NAME = "Pawnmorpher";


        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        public static void FindMissingMorphDescriptions()
        {
            List<MorphDef> morphs = DefDatabase<MorphDef>
                                   .AllDefs.Where(def => string.IsNullOrEmpty(def.description)
                                                      || def.description.StartsWith("!!!"))
                                   .ToList();
            if (morphs.Count == 0)
            {
                Log.Message("all morphs have descriptions c:");
            }
            else
            {
                string str = string.Join("\n", morphs.Select(def => def.defName).ToArray());
                Log.Message($"Morphs Missing descriptions:\n{str}");
            }
        }


        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        public static void FindMissingMutationDescriptions()
        {
            bool SelectionFunc(HediffDef def)
            {
                if (!typeof(Hediff_AddedMutation).IsAssignableFrom(def.hediffClass))
                    return false; //must be mutation hediff 
                return string.IsNullOrEmpty(def.description) || def.description.StartsWith("!!!");
            }

            IEnumerable<HediffDef> mutations = DefDatabase<HediffDef>.AllDefs.Where(SelectionFunc);

            string str = string.Join("\n\t", mutations.Select(m => m.defName).ToArray());

            Log.Message(string.IsNullOrEmpty(str) ? "no parts with missing description" : str);
        }


        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        public static void GetMutationsWithStages()
        {
            List<MutationDef> allMutations = MutationDef.AllMutations.ToList();


            if (allMutations.Count == 0)
            {
                Log.Message("no mutations with multiple stages");
                return;
            }

            var infoLst = new List<HediffStageInfo>();


            foreach (MutationDef mutation in allMutations)
            {
                List<HediffStage> stages = mutation.stages;
                if (stages == null) continue;
                float severityPerDay = mutation.CompProps<CompProperties_MutationSeverityAdjust>()?.severityPerDay ?? 0;

                if (severityPerDay <= 0)
                    continue; //productive mutations don't progress like the others 


                for (var index = 0; index < stages.Count; index++)
                {
                    HediffStage hediffStage = stages[index];
                    var stageInfo = new HediffStageInfo
                    {
                        defName = mutation.defName,
                        minSeverity = hediffStage.minSeverity,
                        severityPerDay = severityPerDay,
                        stageIndex = index
                    };
                    infoLst.Add(stageInfo);
                }
            }

            string outStr = infoLst.Select(s => s.ToString()).Join(delimiter:"\n");
            Log.Message($"Mutation Stage Info:\n{HediffStageInfo.Header}\n{outStr}");
        }


        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        public static void GetSpreadingMutationStats()
        {
            FieldInfo isSkinCoveredF =
                typeof(BodyPartDef)
                   .GetField("skinCovered", //have to get isSkinCovered field by reflection because it's not public 
                             BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            List<BodyPartRecord> spreadableParts = BodyDefOf
                                                  .Human.GetAllMutableParts()
                                                  .Where(r => (bool) (isSkinCoveredF?.GetValue(r.def) ?? false))
                                                  .ToList();

            int spreadablePartsCount = spreadableParts.Count;


            var allSpreadableMutations = MutationDef.AllMutations.Select(mut => new
                                                     {
                                                         mut, //make an anonymous object to keep track of both the mutation and comp
                                                         comp = mut.CompProps<SpreadingMutationCompProperties>()
                                                     })
                                                    .Where(o => o.comp != null) //keep only those with a spreading property 
                                                    .ToList();

            var builder = new StringBuilder();

            builder.AppendLine($"{spreadablePartsCount} parts that spreadable parts can spread onto");
            foreach (var sMutation in allSpreadableMutations)
            {
                if (sMutation.mut.stages == null) continue;

                builder.AppendLine($"----{sMutation.mut.defName}-----");

                //print some stuff about the comp 
                builder.AppendLine($"mtb:{sMutation.comp.mtb}");
                builder.AppendLine($"searchDepth:{sMutation.comp.maxTreeSearchDepth}");


                for (var i = 0; i < sMutation.mut.stages.Count; i++)
                {
                    HediffStage stage = sMutation.mut.stages[i];
                    builder.AppendLine($"\tstage:{stage.label},{i}");


                    foreach (PawnCapacityModifier capMod in stage.capMods ?? Enumerable.Empty<PawnCapacityModifier>())
                    {
                        float postFactor =
                            Mathf.Pow(capMod.postFactor,
                                      spreadablePartsCount); //use pow because the post factors are multiplied together, not added 
                        builder.AppendLine($"\t\t{capMod.capacity.defName}:[offset={capMod.offset * spreadablePartsCount}, postFactor={capMod.postFactor = postFactor}]");
                    }

                    foreach (StatModifier statOffset in stage.statOffsets ?? Enumerable.Empty<StatModifier>())
                        builder.AppendLine($"\t\t{statOffset.stat.defName}:{statOffset.value * spreadablePartsCount}");
                }

                builder.AppendLine(""); //make an empty line between mutations 
            }


            Log.Message(builder.ToString());
        }


        [DebugOutput(category=MAIN_CATEGORY_NAME)]
        public static void ListHybridStateOffset()
        {
            IEnumerable<MorphDef> morphs = DefDatabase<MorphDef>.AllDefs;
            ThingDef human = ThingDefOf.Human;

            Dictionary<StatDef, float> lookup = human.statBases.ToDictionary(s => s.stat, s => s.value);

            var builder = new StringBuilder();

            foreach (MorphDef morphDef in morphs)
            {
                builder.AppendLine($"{morphDef.label}:");

                foreach (StatModifier statModifier in morphDef.hybridRaceDef.statBases ?? Enumerable.Empty<StatModifier>())
                {
                    float humanVal = lookup.TryGetValue(statModifier.stat);
                    float diff = statModifier.value - humanVal;
                    string sym = diff > 0 ? "+" : "";
                    string str = $"{statModifier.stat.label}:{sym}{diff}";
                    builder.AppendLine($"\t\t{str}");
                }
            }

            Log.Message($"{builder}");
        }


      

        [DebugOutput(MAIN_CATEGORY_NAME, onlyWhenPlaying = true)]
        public static void OpenActionMenu()
        {
            Find.WindowStack.Add(new Pawnmorpher_DebugDialogue());
        }

        private static void BuildGraphvizTree([NotNull] AnimalClassBase aBase, StringBuilder builder)
        {
            foreach (AnimalClassBase child in aBase.Children)
            {
                builder.AppendLine($"{aBase.defName}->{child.defName};");
                BuildGraphvizTree(child, builder);
            }
        }

        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        private static void FindMissingMorphReactions()
        {
            var missingMorphs = new List<MorphDef>();


            foreach (MorphDef morphDef in MorphDef.AllDefs)
            {
                MorphDef.TransformSettings tfSettings = morphDef.transformSettings;
                if (IsTODOThought(tfSettings?.transformationMemory) || IsTODOThought(tfSettings?.revertedMemory))
                    missingMorphs.Add(morphDef);
            }

            string msgText = missingMorphs.Select(m => m.defName).Join(delimiter: "\n");
            Log.Message(msgText);
        }


        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        private static void FindMutaniteCommonalityOnMap()
        {
            List<Thing> mineablesOnMap = Find.CurrentMap.listerThings.AllThings
                                             .Where(t => t.def.building?.mineableThing != null)
                                             .ToList();
            List<Thing> mutaniteOnMap = mineablesOnMap.Where(t => t.def.defName == "Mutonite")
                                                      .ToList();
            ThingCategoryDef chunkCat = ThingCategoryDefOf.Chunks;

            List<Thing> totalOres = mineablesOnMap.Where(t => !chunkCat.ContainedInThisOrDescendant(t.def.building.mineableThing))
                                                  .ToList();

            Log.Message($"Mutonite:{mutaniteOnMap.Count}\tOres:{totalOres.Count}\tAll Chunks:{mineablesOnMap.Count}");
        }

        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        private static void GetNonMutationMutations()
        {
            var builder = new StringBuilder();
            foreach (HediffDef hediffDef in DefDatabase<HediffDef>.AllDefs.Where(d => typeof(Hediff_AddedMutation)
                                                                                    .IsAssignableFrom(d.hediffClass)))
            {
                if (hediffDef is MutationDef) continue;
                builder.AppendLine(hediffDef.defName);
            }

            string msg = builder.Length > 0 ? builder.ToString() : $"all mutations use {nameof(MutationDef)} c:";
            Log.Message(msg);
        }


        [DebugOutput(category = MAIN_CATEGORY_NAME, onlyWhenPlaying = true)]
        private static void GetPawnsNewInfluence()
        {
            var wDict = new Dictionary<AnimalClassBase, float>();
            var mutations = new List<Hediff_AddedMutation>();
            var strings = new List<string>();
            float maxInf = MorphUtilities.MaxHumanInfluence;
            foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonists)
            {
                mutations.Clear();
                mutations.AddRange(pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>());

                AnimalClassUtilities.FillInfluenceDict(mutations, wDict);
                if (wDict.Count == 0) continue;
                //now build the log message entry 
                var builder = new StringBuilder();
                builder.AppendLine($"{pawn.Name}:");
                foreach (KeyValuePair<AnimalClassBase, float> kvp in wDict)
                {
                    var def = (Def) kvp.Key;
                    builder.AppendLine($"\t{def.label}:{(kvp.Value / maxInf).ToStringPercent()}");
                }

                strings.Add(builder.ToString());
            }

            string msg = strings.Join(delimiter: "\n----------------------------\n");
            Log.Message(msg);
        }



        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        private static void ListAllMutationsPerMorph()
        {
            var builder = new StringBuilder();

            foreach (MorphDef morphDef in MorphDef.AllDefs)
            {
                builder.AppendLine(morphDef.defName);

                builder.AppendLine(morphDef.AllAssociatedMutations.Select(m => m.defName).Join(delimiter: ","));
            }

            Log.Message(builder.ToString());
        }

        [DebugOutput(category = MAIN_CATEGORY_NAME, onlyWhenPlaying = true)]
        private static void ListAllSpreadableParts()
        {
            BodyDef bDef = BodyDefOf.Human;
            var sBuilder = new StringBuilder();

            foreach (BodyPartDef allMutablePart in bDef.GetAllMutableParts().Select(b => b.def).Distinct())
                if (allMutablePart.IsSkinCoveredInDefinition_Debug)
                    sBuilder.AppendLine($"<li>{allMutablePart.defName}</li>");

            Log.Message(sBuilder.ToString());
        }

        [DebugOutput(MAIN_CATEGORY_NAME, true)]
        private static void ListMutationsInMorphs()
        {
            var builder = new StringBuilder();

            foreach (MorphDef morphDef in DefDatabase<MorphDef>.AllDefsListForReading)
            {
                string outStr = morphDef.AllAssociatedMutations.Select(m => m.defName).Join(delimiter: ",");
                builder.AppendLine($"{morphDef.defName}:{outStr}");
            }

            Log.Message(builder.ToString());
        }


        [DebugOutput(MAIN_CATEGORY_NAME, true)]
        private static void LogMissingMutationStages()
        {
            var builder = new StringBuilder();
            List<string> needsStages = new List<string>(),
                         needsParagonStage = new List<string>(),
                         needsAfflictedStage = new List<string>();


            foreach (MutationDef allMutation in MutationDef.AllMutations)
            {
                if (allMutation.stages == null || allMutation.stages.Count == 0)
                {
                    needsStages.Add(allMutation.defName);
                    continue;
                }

                bool hasAfflicted = false, hasParagon = false;

                foreach (HediffStage allMutationStage in allMutation.stages)
                {
                    if (allMutationStage.minSeverity < 0) hasAfflicted = true;
                    if (allMutationStage.minSeverity > 1) hasParagon = true;
                }

                if (!hasParagon)
                    needsParagonStage.Add(allMutation.defName);
                if (!hasAfflicted) needsAfflictedStage.Add(allMutation.defName);
            }

            builder.AppendLine($"Needs Stages:\n{needsStages.Join(delimiter: "\n")}");
            builder.AppendLine($"\nNeeds Paragon Stages:\n{needsParagonStage.Join(delimiter: "\n")}");
            builder.AppendLine($"\nNeeds Afflicted Stages:\n{needsAfflictedStage.Join(delimiter: "\n")}");

            Log.Message(builder.ToString());
        }

        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        private static void MaximumMutationPointsForHumans()
        {
            Log.Message(MorphUtilities.MaxHumanInfluence.ToString());
        }

        [DebugOutput(MAIN_CATEGORY_NAME)]
        [UsedImplicitly]
        private static void PrintGraphVizAnimalTree()
        {
            var builder = new StringBuilder();
            builder.AppendLine("digraph animalClassTree\n{\n\trankdir=LR");

            foreach (AnimalClassDef aClass in DefDatabase<AnimalClassDef>.AllDefs)
                builder.AppendLine($"\t{aClass.defName}[shape=square]");

            BuildGraphvizTree(AnimalClassDefOf.Animal, builder);
            builder.AppendLine("}");
            Log.Message(builder.ToString());
        }

        private struct HediffStageInfo
        {
            public string defName;
            public int stageIndex;
            public float minSeverity;
            public float severityPerDay;

            public static string Header => "defName,stageIndex,minSeverity,severityPerDay";

            /// <summary>Returns the fully qualified type name of this instance.</summary>
            /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
            public override string ToString()
            {
                return $"{defName},{stageIndex},{minSeverity},{severityPerDay}";
            }
        }

        [DebugOutput(category=MAIN_CATEGORY_NAME)]
        static void LogAllBackstoryInfo()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var backstory in DefDatabase<AlienRace.BackstoryDef>.AllDefs)
            {
                var ext = backstory.GetModExtension<MorphPawnKindExtension>(); 
                if(ext == null) continue;
                builder.AppendLine($"---{backstory.defName}---");
                builder.AppendLine(ext.ToStringFull()); 

            }

            Log.Message(builder.ToString()); 
        }

        [DebugOutput(category = MAIN_CATEGORY_NAME, onlyWhenPlaying = true)]
        static void ListDesignatedFormerHumans()
        {
            var map = Find.CurrentMap;
            if (map == null) return;
            var designation =
                map.designationManager.allDesignations.Where(d => d.def == PMDesignationDefOf.RecruitSapientFormerHuman).ToList();
            if (designation.Count == 0)
            {
                Log.Message($"No {nameof(PMDesignationDefOf.RecruitSapientFormerHuman)} on map");
                return;
            }

            var str = designation.Join(s => s.target.Label, "\n");
            Log.Message(str); 



        }
    }
}