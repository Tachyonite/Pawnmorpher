// DebugLogUtils.cs created by Iron Wolf for Pawnmorph on 09/23/2019 7:54 AM
// last updated 09/27/2019  8:00 AM

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using AlienRace;
using Harmony;
using HugsLib.Utils;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Debug = System.Diagnostics.Debug;

#pragma warning disable 1591
namespace Pawnmorph.DebugUtils
{
    [HasDebugOutput]
    public static class DebugLogUtils
    {
        public const string MAIN_CATEGORY_NAME = "Pawnmorpher";

        /// <summary>
        ///     Asserts the specified condition. if false an error message will be displayed
        /// </summary>
        /// <param name="condition">if false will display an error message</param>
        /// <param name="message">The message.</param>
        /// <returns>the condition</returns>
        [DebuggerHidden]
        [Conditional("DEBUG")]
        [AssertionMethod]
        public static void Assert(bool condition, string message)
        {
            if (!condition) Log.Error($"assertion failed:{message}");
        }


        [DebugOutput, Category(MAIN_CATEGORY_NAME)]
        static void ListAllMutationsPerMorph()
        {
            StringBuilder builder = new StringBuilder();

            foreach (MorphDef morphDef in MorphDef.AllDefs)
            {
                builder.AppendLine(morphDef.defName);

                builder.AppendLine(morphDef.AllAssociatedMutations.Select(m => m.defName).Join(","));
            }

            Log.Message(builder.ToString()); 


        }

        static bool IsTODOThought(ThoughtDef thoughtDef)
        {
            if (thoughtDef?.stages == null) return true;
            if (thoughtDef.stages.Any(s => string.IsNullOrEmpty(s?.label) || s.label.StartsWith("TODO") || s.label.StartsWith("!!!")))
                return true;
            if (thoughtDef.stages.Any(s => string.IsNullOrEmpty(s?.description)
                                        || s.description.StartsWith("TODO")
                                        || s.description.StartsWith("!!!")))
                return true;
            return false; 
        }

        [DebugOutput, Category(MAIN_CATEGORY_NAME)]
        static void FindMissingMorphReactions()
        {
            List<MorphDef> missingMorphs = new List<MorphDef>();


            foreach (MorphDef morphDef in MorphDef.AllDefs)
            {
                var tfSettings = morphDef.transformSettings;
                if (IsTODOThought(tfSettings?.transformationMemory) || IsTODOThought(tfSettings?.revertedMemory))
                    missingMorphs.Add(morphDef); 
            }

            var msgText = missingMorphs.Select(m => m.defName).Join("\n");
            Log.Message(msgText); 
        }


        [DebugOutput, Category(MAIN_CATEGORY_NAME)]
        static void FindMutaniteCommonalityOnMap()
        {

            var mineablesOnMap = Find.CurrentMap.listerThings.AllThings
                                     .Where(t => t.def.building?.mineableThing != null)
                                     .ToList();
            var mutaniteOnMap = mineablesOnMap.Where(t => t.def.defName == "Mutonite")
                                              .ToList();
            var chunkCat = ThingCategoryDefOf.Chunks;
            
            var totalOres = mineablesOnMap.Where(t => !chunkCat.ContainedInThisOrDescendant(t.def.building.mineableThing)).ToList();

            Log.Message($"Mutonite:{mutaniteOnMap.Count}\tOres:{totalOres.Count}\tAll Chunks:{mineablesOnMap.Count}");



        }

        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
        public static void CheckBodyHediffGraphics()
        {
            IEnumerable<HediffGiver_Mutation> GetMutationGivers(IEnumerable<HediffStage> stages)
            {
                return stages.SelectMany(s => s.hediffGivers ?? Enumerable.Empty<HediffGiver>())
                             .OfType<HediffGiver_Mutation>();
            }


            var givers = DefDatabase<HediffDef>
                        .AllDefs.Where(def => typeof(Hediff_Morph).IsAssignableFrom(def.hediffClass))
                        .Select(def => new
                         {
                             def,
                             givers = GetMutationGivers(def.stages ?? Enumerable.Empty<HediffStage>())
                                .ToList() //grab all mutation givers but keep the def it came from around 
                         })
                        .Where(a => a.givers.Count
                                  > 0); //keep only the entries that have some givers in them 

            var human = (ThingDef_AlienRace) ThingDefOf.Human;

            List<AlienPartGenerator.BodyAddon> bodyAddons = human.alienRace.generalSettings.alienPartGenerator.bodyAddons;


            var lookupDict = new Dictionary<string, string>();

            foreach (AlienPartGenerator.BodyAddon bodyAddon in bodyAddons)
            foreach (AlienPartGenerator.BodyAddonHediffGraphic bodyAddonHediffGraphic in bodyAddon.hediffGraphics)
                lookupDict[bodyAddonHediffGraphic.hediff] =
                    bodyAddon.bodyPart; //find out what parts what hediffs are assigned to in the patch file 

            var builder = new StringBuilder();
            var errStrs = new List<string>();
            foreach (var giverEntry in givers)
            {
                errStrs.Clear();
                foreach (HediffGiver_Mutation hediffGiverMutation in giverEntry.givers)
                {
                    HediffDef hediff = hediffGiverMutation.hediff;

                    BodyPartDef addPart = hediffGiverMutation.partsToAffect.FirstOrDefault();
                    if (addPart == null) continue; //if there are no parts to affect just skip 

                    if (lookupDict.TryGetValue(hediff.defName, out string part))
                        if (part != addPart.defName)
                            errStrs.Add($"hediff {hediff.defName} is being attached to {addPart.defName} but is assigned to {part} in patch file");
                }

                if (errStrs.Count > 0)
                {
                    builder.AppendLine($"in def {giverEntry.def.defName}: ");
                    foreach (string errStr in errStrs) builder.AppendLine($"\t\t{errStr}");

                    builder.AppendLine("");
                }
            }

            if (builder.Length > 0)
                Log.Warning(builder.ToString());
            else
                Log.Message("no inconsistencies found");
        }



        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
        public static void FindAllTODOThoughts()
        {
            var builder = new StringBuilder();
            var defNames = new List<string>();
            foreach (ThoughtDef thoughtDef in DefDatabase<ThoughtDef>.AllDefs)
            {
                var addedHeader = false;
                for (var index = 0; index < (thoughtDef?.stages?.Count ?? 0); index++)
                {
                    ThoughtStage stage = thoughtDef?.stages?[index];
                    if (stage == null) continue;
                    if (string.IsNullOrEmpty(stage.label) || string.IsNullOrEmpty(stage.description)) continue;
                    if (stage.label == "TODO"
                     || stage.description == "TODO"
                     || stage.description.StartsWith("!!!")
                     || stage.label.StartsWith("!!!"))
                    {
                        if (!addedHeader)
                        {
                            builder.AppendLine($"In {thoughtDef.defName}:");
                            addedHeader = true;
                        }

                        defNames.Add(thoughtDef.defName);
                        builder.AppendLine($"{index}) label:{stage.label} description:\"{stage.description}\"".Indented());
                    }
                }
            }

            builder.AppendLine(defNames.Distinct().Join("\n"));

            Log.Message(builder.ToString());
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
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


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
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

        struct HediffStageInfo
        {
            public string defName;
            public int stageIndex;
            public float minSeverity;
            public float severityPerDay;

            /// <summary>Returns the fully qualified type name of this instance.</summary>
            /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
            public override string ToString()
            {
                return $"{defName},{stageIndex},{minSeverity},{severityPerDay}"; 
            }

            public static string Header => "defName,stageIndex,minSeverity,severityPerDay"; 
        }


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void GetMutationsWithStages()
        {
            var allMutations = MutationDef.AllMutations.ToList(); 


            if (allMutations.Count == 0)
            {
                Log.Message("no mutations with multiple stages");
                return;
            }

            List<HediffStageInfo> infoLst = new List<HediffStageInfo>(); 
            
            

            foreach (var mutation in allMutations)
            {
                List<HediffStage> stages = mutation.stages;
                if(stages == null) continue;
                float severityPerDay = mutation.CompProps<CompProperties_MutationSeverityAdjust>()?.severityPerDay ?? 0;

                if(severityPerDay <= 0) 
                    continue; //productive mutations don't progress like the others 


                for (var index = 0; index < stages.Count; index++)
                {
                    HediffStage hediffStage = stages[index];
                    var stageInfo = new HediffStageInfo()
                    {
                        defName = mutation.defName,
                        minSeverity = hediffStage.minSeverity,
                        severityPerDay = severityPerDay,
                        stageIndex = index
                    };
                    infoLst.Add(stageInfo); 
                }
            }

            var outStr = infoLst.Select(s => s.ToString()).Join("\n");
            Log.Message($"Mutation Stage Info:\n{HediffStageInfo.Header}\n{outStr}");
        }


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
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


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void GetThoughtlessMutations()
        {

            var missingThought = DefDatabase<MutationDef>.AllDefs.Where(m => m.mutationMemory == null).Join(m => m.defName, "\n"); 

            Log.Message(missingThought);
        }


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
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


        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
        public static void LogMissingMutationTales()
        {
            bool
                SelectionFunc(
                    HediffDef def) //local function to grab all morph hediffDefs that have givers that are missing tales 
            {
                if (!typeof(Hediff_Morph).IsAssignableFrom(def.hediffClass)) return false;
                return (def.stages ?? Enumerable.Empty<HediffStage>())
                      .SelectMany(s => s.hediffGivers ?? Enumerable.Empty<HediffGiver>())
                      .OfType<HediffGiver_Mutation>()
                      .Any(g => g.tale == null);
            }

            IEnumerable<Tuple<HediffDef, HediffGiver_Mutation>>
                GetMissing(HediffDef def) //local function that will grab all hediff givers missing a tale 
            {
                //and keep the def it came from around 
                IEnumerable<HediffGiver_Mutation> givers =
                    def.stages?.SelectMany(s => s.hediffGivers ?? Enumerable.Empty<HediffGiver>())
                       .OfType<HediffGiver_Mutation>()
                       .Where(g => g.tale == null)
                 ?? Enumerable.Empty<HediffGiver_Mutation>();
                foreach (HediffGiver_Mutation hediffGiverMutation in givers)
                    yield return new Tuple<HediffDef, HediffGiver_Mutation>(def, hediffGiverMutation);
            }

            IEnumerable<IGrouping<HediffDef, HediffDef>> missingGivers = DefDatabase<HediffDef>.AllDefs.Where(SelectionFunc)
                                                                                               .SelectMany(GetMissing)
                                                                                               .GroupBy(tup => tup.Second.hediff, //group by the part that needs a tale  
                                                                                                        tup => tup
                                                                                                           .First); //and select the morph tfs their givers are found 
            var builder = new StringBuilder();

            var missingLst = new HashSet<HediffDef>();

            foreach (IGrouping<HediffDef, HediffDef> missingGiver in missingGivers)
            {
                string keyStr = $"{missingGiver.Key.defName} is missing a tale in the following morph hediffs:";
                missingLst.Add(missingGiver.Key); //keep the keys for the summary later
                builder.AppendLine(keyStr);
                foreach (HediffDef hediffDef in missingGiver) builder.AppendLine($"\t\t{hediffDef.defName}");
            }


            if (builder.Length > 0)
            {
                Log.Message(builder.ToString());
                builder = new StringBuilder();
                builder.AppendLine($"-------------------{missingLst.Count} parts need tales----------------"); //summary for convenience 
                builder.AppendLine(string.Join("\n", missingLst.Select(def => def.defName).ToArray()));
                Log.Message(builder.ToString());
            }
            else
            {
                Log.Message("All parts have a tale");
            }
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        [ModeRestrictionPlay]
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

        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
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


        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
        private static void GetOldMorphTfDefs()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Full Transformations:");
            foreach (MorphDef morphDef in MorphDef.AllDefs)
                if (morphDef.fullTransformation != null
                 && typeof(Hediff_Morph).IsAssignableFrom(morphDef.fullTransformation.hediffClass))
                    builder.AppendLine(morphDef.fullTransformation.defName);

            builder.AppendLine("Partials:");
            foreach (MorphDef morphDef in MorphDef.AllDefs)
                if (morphDef.partialTransformation != null
                 && typeof(Hediff_Morph).IsAssignableFrom(morphDef.partialTransformation.hediffClass))
                    builder.AppendLine(morphDef.partialTransformation.defName);

            Log.Message(builder.ToString());
        }

        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
        [ModeRestrictionPlay]
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

            string msg = strings.Join("\n----------------------------\n");
            Log.Message(msg);
        }

        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
        private static void ListAllSpreadableParts()
        {
            BodyDef bDef = BodyDefOf.Human;
            var sBuilder = new StringBuilder();

            foreach (BodyPartDef allMutablePart in bDef.GetAllMutableParts().Select(b => b.def).Distinct())
                if (allMutablePart.IsSkinCoveredInDefinition_Debug)
                    sBuilder.AppendLine($"<li>{allMutablePart.defName}</li>");

            Log.Message(sBuilder.ToString());
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        private static void ListMutationsInMorphs()
        {
            var builder = new StringBuilder();

            foreach (MorphDef morphDef in DefDatabase<MorphDef>.AllDefsListForReading)
            {
                string outStr = morphDef.AllAssociatedMutations.Select(m => m.defName).Join(",");
                builder.AppendLine($"{morphDef.defName}:{outStr}");
            }

            Log.Message(builder.ToString());
        }


        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
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

            builder.AppendLine($"Needs Stages:\n{needsStages.Join("\n")}");
            builder.AppendLine($"\nNeeds Paragon Stages:\n{needsParagonStage.Join("\n")}");
            builder.AppendLine($"\nNeeds Afflicted Stages:\n{needsAfflictedStage.Join("\n")}");

            Log.Message(builder.ToString());
        }

        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
        private static void MaximumMutationPointsForHumans()
        {
            Log.Message(MorphUtilities.MaxHumanInfluence.ToString());
        }

        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
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
    }
}