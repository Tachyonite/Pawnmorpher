// DebugLogUtils.cs created by Iron Wolf for Pawnmorph on 09/23/2019 7:54 AM
// last updated 09/27/2019  8:00 AM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Thoughts;
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

        [Category(MAIN_CATEGORY_NAME), DebugOutput, ModeRestrictionPlay]
        public static void PrintMutationLogs()
        {

            var logEntries = Find.PlayLog.AllEntries.OfType<MutationLogEntry>().ToList(); //save the list for later 

            StringBuilder builder = new StringBuilder();
            const string joinStr = "\n___________________________________\n"; 
            foreach (Pawn colonist in PawnsFinder.AllMaps_FreeColonists)
            {
                List<string> lst = new List<string>(); 

                foreach (MutationLogEntry log in logEntries.Where(l => l.Concerns(colonist)))
                {
                    lst.Add(log.ToGameStringFromPOV(colonist)); 
                    
                }

                if (lst.Count > 0)
                {
                    builder.AppendLine($"$$--------------{colonist.Name}-----------------$$");
                    builder.AppendLine(string.Join(joinStr, lst.ToArray())); 
                }
            }

            Log.Message(builder.ToString()); 


        }

        /// <summary>Prints all mutations that are missing extension.</summary>
        [DebugOutput] 
        [Category(MAIN_CATEGORY_NAME)]
        public static void PrintAllMutationsMissingExtension()
        {
            var mutations = DefDatabase<HediffDef>
                           .AllDefs.Where(d => typeof(Hediff_AddedMutation).IsAssignableFrom(d.hediffClass) && !d.IsObsolete())
                           .Where(d => !d.HasModExtension<MutationHediffExtension>())
                           .Select(d => d.defName); 
            Log.Message($"hediffs missing mutation extension:\n{string.Join("\n", mutations.ToArray())}");
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

            var bodyAddons = human.alienRace.generalSettings.alienPartGenerator.bodyAddons;


            var lookupDict = new Dictionary<string, string>();

            foreach (var bodyAddon in bodyAddons)
            foreach (var bodyAddonHediffGraphic in bodyAddon.hediffGraphics)
                lookupDict[bodyAddonHediffGraphic.hediff] =
                    bodyAddon.bodyPart; //find out what parts what hediffs are assigned to in the patch file 

            var builder = new StringBuilder();
            var errStrs = new List<string>();
            foreach (var giverEntry in givers)
            {
                errStrs.Clear();
                foreach (var hediffGiverMutation in giverEntry.givers)
                {
                    var hediff = hediffGiverMutation.hediff;

                    var addPart = hediffGiverMutation.partsToAffect.FirstOrDefault();
                    if (addPart == null) continue; //if there are no parts to affect just skip 

                    if (lookupDict.TryGetValue(hediff.defName, out var part))
                        if (part != addPart.defName)
                            errStrs.Add($"hediff {hediff.defName} is being attached to {addPart.defName} but is assigned to {part} in patch file");
                }

                if (errStrs.Count > 0)
                {
                    builder.AppendLine($"in def {giverEntry.def.defName}: ");
                    foreach (var errStr in errStrs) builder.AppendLine($"\t\t{errStr}");

                    builder.AppendLine("");
                }
            }

            if (builder.Length > 0)
                Log.Warning(builder.ToString());
            else
                Log.Message("no inconsistencies found");
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        [ModeRestrictionPlay]
        public static void ShowColonyAspectInfo()
        {
            IEnumerable<Pawn> allCPawns = PawnsFinder.AllMaps_FreeColonists;
            var builder = new StringBuilder();
            foreach (Pawn pawn in allCPawns)
            {
                AspectTracker aspectTracker = pawn.GetAspectTracker();
                if (aspectTracker == null) continue;

                builder.AppendLine(pawn.Name.ToStringFull);
                foreach (Aspect aspect in aspectTracker.Aspects)
                {
                    builder.AppendLine($"\t{aspect.Label}: [{aspect.def.defName}:{aspect.StageIndex}]");
                    if (!string.IsNullOrEmpty(aspect.Description)) builder.AppendLine($"\t{aspect.Description}");
                }
            }

            Log.Message(builder.ToString());
        }


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        [ModeRestrictionPlay]
        public static void CheckMorphTracker()
        {
            var builder = new StringBuilder();
            foreach (var allMapsFreeColonist in PawnsFinder.AllMaps_FreeColonists)
                if (allMapsFreeColonist.GetComp<MorphTrackingComp>() == null)
                    builder.AppendLine($"{allMapsFreeColonist.Name} does not have a morphTracker!");

            if (builder.Length > 0)
            {
                Log.Warning(builder.ToString());
                builder = new StringBuilder();
            }

            var map = Find.CurrentMap;
            if (map == null) return;
            var comp = map.GetComponent<MorphTracker>();

            foreach (var morph in DefDatabase<MorphDef>.AllDefs)
            {
                var i = comp[morph];
                builder.AppendLine($"{morph.defName}={i}");
            }


            builder.AppendLine("--------------Groups------------");

            foreach (var morphGroupDef in DefDatabase<MorphGroupDef>.AllDefs)
            {
                var counter = 0;
                foreach (var morphDef in morphGroupDef.MorphsInGroup) counter += comp[morphDef];

                builder.AppendLine($"{morphGroupDef.defName}={counter}");
            }


            Log.Message(builder.ToString());
        }


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void FindMissingMorphDescriptions()
        {
            var morphs = DefDatabase<MorphDef>.AllDefs.Where(def => string.IsNullOrEmpty(def.description)).ToList();
            if (morphs.Count == 0)
            {
                Log.Message("all morphs have descriptions c:");
            }
            else
            {
                var str = string.Join("\n", morphs.Select(def => def.defName).ToArray());
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
                return string.IsNullOrEmpty(def.description);
            }

            var mutations = DefDatabase<HediffDef>.AllDefs.Where(SelectionFunc);

            var str = string.Join("\n\t", mutations.Select(m => m.defName).ToArray());

            Log.Message(string.IsNullOrEmpty(str) ? "no parts with missing description" : str);
        }

        static string CSVFormat(this string str)
        {
            if (str == null) return ""; 
            if (str.Contains('\"') || str.Contains('\n') || str.Contains(','))
            {
                str = str.Replace("\"", "\\\"");
                str = $"\"{str}\"";
            }
            return str; 


        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void GetSpreadingMutationStats()
        {
            var isSkinCoveredF =
                typeof(BodyPartDef).GetField("skinCovered", //have to get isSkinCovered field by reflection because it's not public 
                                             BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var spreadableParts = MorphUtilities.AllMutableRecords.Where(r => (bool) isSkinCoveredF.GetValue(r.def)).ToList();

            var spreadablePartsCount = spreadableParts.Count;


            var allSpreadableMutations = MutationUtilities.AllMutations.Select(mut => new
                                                           {
                                                               mut = mut, //make an anonymous object to keep track of both the mutation and comp
                                                               comp = mut.CompProps<Hediffs.SpreadingMutationCompProperties>()
                                                           })
                                                          .Where(o => o.comp != null) //keep only those with a spreading property 
                                                          .ToList();

            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"{spreadablePartsCount} parts that spreadable parts can spread onto");
            foreach (var sMutation in allSpreadableMutations)
            {
                if(sMutation.mut.stages == null) continue;

                builder.AppendLine($"----{sMutation.mut.defName}-----");

                //print some stuff about the comp 
                builder.AppendLine($"mtb:{sMutation.comp.mtb}");
                builder.AppendLine($"searchDepth:{sMutation.comp.maxTreeSearchDepth}"); 


                for (int i = 0; i < sMutation.mut.stages.Count; i++)
                {
                    var stage = sMutation.mut.stages[i];
                    builder.AppendLine($"\tstage:{stage.label},{i}");


                    foreach (PawnCapacityModifier capMod in stage.capMods ?? Enumerable.Empty<PawnCapacityModifier>())
                    {
                        var postFactor = Mathf.Pow(capMod.postFactor, spreadablePartsCount); //use pow because the post factors are multiplied together, not added 
                        builder.AppendLine($"\t\t{capMod.capacity.defName}:[offset={capMod.offset * spreadablePartsCount}, postFactor={capMod.postFactor = postFactor}]");
                    }

                    foreach (StatModifier statOffset in stage.statOffsets ?? Enumerable.Empty<StatModifier>())
                    {
                        builder.AppendLine($"\t\t{statOffset.stat.defName}:{statOffset.value * spreadablePartsCount}");
                    }

                }

                builder.AppendLine("");//make an empty line between mutations 


            }


            Log.Message(builder.ToString()); 
        }


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void GenerateWritingReport()
        {
            var builder = new StringBuilder();

            IEnumerable<HediffDef> allMutations =
                DefDatabase<HediffDef>.AllDefs.Where(d => typeof(Hediff_AddedMutation).IsAssignableFrom(d.hediffClass));

            List<HediffDef> allHediffsWithGivers =
                DefDatabase<HediffDef>.AllDefs.Where(d => d.GetAllHediffGivers().Any(g => g is HediffGiver_Mutation)).ToList();

            List<HediffGiver_Mutation> allGivers = allHediffsWithGivers
                                                  .SelectMany(h => h.GetAllHediffGivers().OfType<HediffGiver_Mutation>())
                                                  .ToList();

            IEnumerable<MorphDef> allMorphs = MorphDef.AllDefs;
            List<Def_MorphThought> allBPReactions = DefDatabase<ThoughtDef>
                                                   .AllDefs.OfType<Def_MorphThought>()
                                                   .Where(d => d?.requiredTraits?.Contains(TraitDefOf.BodyPurist) ?? false)
                                                   .ToList();
            List<Def_MorphThought> allFurryReactions = DefDatabase<ThoughtDef>
                                                      .AllDefs.OfType<Def_MorphThought>()
                                                      .Where(d => d.requiredTraits?.Contains(PMTraitDefOf.MutationAffinity)
                                                               ?? false)
                                                      .ToList();

            var mutationRows = new List<string[]>
            {
                new[] {"defName", "label", "description", "tale", "memory"}
            };

            var morphRRows = new List<string[]>
            {
                new[]
                {
                    "defName", "label", "description", "tfMemory", "tfTale", "furry tf reaction", " body purist tf reaction",
                    "default reversion memory", "furry reversion memory", "bp reversion memory"
                }
            };

            foreach (HediffDef mutation in allMutations)
            {
                string defName = mutation.defName;
                string label = mutation.label;
                string description = mutation.description;

                HediffGiver_Mutation giver =
                    allGivers.FirstOrDefault(g => g.hediff == mutation && (g.tale != null || g.memory != null));

                TaleDef tale = giver?.tale;
                ThoughtDef memory = giver?.memory;

                string taleDefName = tale?.defName ?? "";
                string memoryDefName = memory?.defName ?? "";
                mutationRows.Add(new[]
                {
                    defName.CSVFormat(), label.CSVFormat(), description.CSVFormat(), taleDefName.CSVFormat(),
                    memoryDefName.CSVFormat()
                });
            }


            foreach (MorphDef morph in allMorphs)
            {
                string defName = morph.defName;
                string label = morph.label;
                string description = morph.description;
                MorphDef.TransformSettings tfSettings = morph.transformSettings;
                string tfMemory = tfSettings?.transformationMemory?.defName ?? "";
                string tfTale = tfSettings?.transformTale?.defName ?? "";
                string furryTfReaction = allFurryReactions.FirstOrDefault(d => d.morph == morph)?.defName ?? "";
                string bpTfReaction = allBPReactions.FirstOrDefault(d => d.morph == morph)?.defName ?? "";
                string defaultReversionMem = tfSettings?.revertedMemory?.defName ?? "";
                string furryReversionMem = tfSettings?.revertedMemoryFurry?.defName ?? "";
                string bpReversionMem = tfSettings?.revertedMemoryBP?.defName ?? "";
                morphRRows.Add(new[]
                {
                    defName.CSVFormat(), label.CSVFormat(), description.CSVFormat(), tfMemory.CSVFormat(), tfTale.CSVFormat(),
                    furryTfReaction.CSVFormat(), bpTfReaction.CSVFormat(),
                    defaultReversionMem.CSVFormat(), furryReversionMem.CSVFormat(), bpReversionMem.CSVFormat()
                });
            }


            foreach (string[] row in mutationRows) builder.AppendLine(string.Join(",", row));

            string dPath = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents");
            dPath = Path.Combine(dPath, "PawnmorpherReports");
            Directory.CreateDirectory(dPath);

            string mutationRPath = Path.Combine(dPath, "mutation_report.csv");

            File.WriteAllText(mutationRPath, builder.ToString());
            builder.Length = 0;

            foreach (string[] row in morphRRows) builder.AppendLine(string.Join(",", row));

            string morphRPath = Path.Combine(dPath, "morph_report.csv");
            File.WriteAllText(morphRPath, builder.ToString());

            Log.Message($"mutation reports written to {dPath}");
        }


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void GetMissingMorphReactionThoughts()
        {
            bool IsMorphReaction(Def_MorphThought d, TraitDef def)
            {
                if (!typeof(Worker_HasMutations).IsAssignableFrom(d.workerClass))
                    return false; //grab only has mutation thoughts 
                return d.requiredTraits?.Contains(def) ?? false; //make
            }


            var bPMorphsR = DefDatabase<ThoughtDef>.AllDefs
                                                   .OfType<Def_MorphThought>()
                                                   .Where(d => IsMorphReaction(d, TraitDefOf.BodyPurist))
                                                   .Select(d => d.morph)
                                                   .ToList();

            var furryMorphR = DefDatabase<ThoughtDef>.AllDefs.OfType<Def_MorphThought>()
                                                     .Where(d => IsMorphReaction(d, PMTraitDefOf.MutationAffinity))
                                                     .Select(d => d.morph)
                                                     .ToList();

            var morphsMissingMemories = MorphDef.AllDefs
                                                .Where(d => d.transformSettings?.transformationMemory == null)
                                                .ToList();

            StringBuilder builder = new StringBuilder();



            var missingBPMorphReactions = new List<MorphDef>();
            var missingFurryMorphReactions = new List<MorphDef>();

            foreach (var morphDef in MorphDef.AllDefs)
            {
                if (!bPMorphsR.Contains(morphDef))
                    missingBPMorphReactions.Add(morphDef);
                if (!furryMorphR.Contains(morphDef))
                    missingFurryMorphReactions.Add(morphDef); 
            }


            if (missingFurryMorphReactions.Count == 0 && missingBPMorphReactions.Count == 0 && morphsMissingMemories.Count == 0)
            {
                Log.Message("no missing morph transformation reactions!");
                return;
            }

            if (missingFurryMorphReactions.Count != 0)
            {
                builder.AppendLine($"-------- Missing Furry Reactions ---------");
                builder.AppendLine(string.Join("\n", missingFurryMorphReactions.Select(m => m.defName).ToArray()));
            }

            if (missingBPMorphReactions.Count != 0)
            {
                builder.AppendLine("--------------- Missing Body Purist Reactions ------------");
                builder.AppendLine(string.Join("\n", missingBPMorphReactions.Select(m => m.defName).ToArray()));
            }

            if (morphsMissingMemories.Count != 0)
            {
                builder.AppendLine("-------------- Morph Shift Memory --------------");
                builder.AppendLine(string.Join("\n", morphsMissingMemories.Select(d => d.defName).ToArray()));
            }

            Log.Message(builder.ToString()); 
        }

        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
        public static void GetMutationsWithoutStages()
        {
            var allMutations =
                MutationUtilities.AllMutations.Where(def => (def.stages?.Count ?? 0) <=
                                                            1); //all mutations without stages 
            var builder = new StringBuilder();


            foreach (var allMutation in allMutations) builder.AppendLine($"{allMutation.defName}");

            Log.Message(builder.ToString());
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void GetMutationsWithStages()
        {
            var allMutations = DefDatabase<HediffDef>
                               .AllDefs.Where(def => typeof(Hediff_AddedMutation).IsAssignableFrom(def.hediffClass))
                               .Where(def => def.stages?.Count > 1 && def.HasComp(typeof(HediffComp_SeverityPerDay)))
                               .ToList();


            if (allMutations.Count == 0)
            {
                Log.Message("no mutations with multiple stages");
                return;
            }

            var builder = new StringBuilder();

            builder.AppendLine("mutations:");
            foreach (var allMutation in allMutations)
                builder.AppendLine($"\t{allMutation.defName}[{allMutation.stages.Count}]");

            builder.AppendLine("\n------------Stats--------------\n");


            foreach (var mutation in allMutations)
            {
                var stages = mutation.stages;
                var severityPerDay = mutation.CompProps<HediffCompProperties_SeverityPerDay>().severityPerDay;
                builder.AppendLine($"{mutation.defName}:");
                float total = 0;
                for (var i = 0; i < stages.Count - 1; i++)
                {
                    var s = stages[i];
                    var s1 = stages[i + 1];
                    var sLabel = string.IsNullOrEmpty(s.label) ? i.ToString() : s.label;
                    var s1Label =
                        string.IsNullOrEmpty(s1.label)
                            ? (i + 1).ToString()
                            : s1.label; //if the label is null just use the index  
                    var diff = s1.minSeverity - s.minSeverity;
                    var tDiff = diff / severityPerDay;
                    total += tDiff;
                    builder.AppendLine($"\t\tstage[{sLabel}] {{{s.minSeverity}}} => stage[{s1Label}] {{{s1.minSeverity}}} takes {tDiff} days");
                }

                builder.AppendLine($"\ttotal time is {total} days");
            }

            Log.Message(builder.ToString());
        }


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void GetThoughtlessMutations()
        {
            IEnumerable<VTuple<HediffDef, HediffGiver_Mutation>> SelectionFunc(HediffDef def)
            {
                foreach (var hediffGiverMutation in def.GetAllHediffGivers().OfType<HediffGiver_Mutation>())
                {
                    if (hediffGiverMutation.memory != null) continue; //if it has a memory associated with it ignore it 

                    yield return new VTuple<HediffDef, HediffGiver_Mutation>(def, hediffGiverMutation);
                }
            }

            var allGiversWithData = DefDatabase<HediffDef>
                                    .AllDefs
                                    .Where(d =>
                                               typeof(Hediff_Morph)
                                                   .IsAssignableFrom(d.hediffClass)) //grab all morph hediffs 
                                    .SelectMany(SelectionFunc) //select all hediffGivers but keep the morph hediff around 
                                    .GroupBy(vT => vT.first,
                                             vT => vT.second) //group by morph Hediff 
                                    .ToList(); //save it as a list 


            if (allGiversWithData.Count == 0) Log.Message("All mutations have memories");
            var builder = new StringBuilder();
            var allMuts = allGiversWithData.SelectMany(g => g.Select(mG => mG.hediff)).Distinct();
            foreach (var mutation in allMuts) builder.AppendLine(mutation.defName);


            builder.AppendLine("---------Giver Locations----------------");

            foreach (var group in allGiversWithData)
            {
                builder.AppendLine($"{group.Key.defName} contains givers of the following mutations that do not have memories with them:");
                foreach (var mutation in group.Select(g => g.hediff)) builder.AppendLine($"\t\t{mutation.defName}");
            }

            Log.Message(builder.ToString());
        }

        /// <summary>
        ///     list all transformation hediffs defined (hediffs of class Hediff_Morph or a subtype there of
        /// </summary>
        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void ListAllMorphTfHediffs()
        {
            var builder = new StringBuilder();
            var morphs = MorphTransformationDefOf.AllMorphs;
            foreach (var morph in morphs)
                builder.AppendLine($"defName:{morph.defName} label:{morph.label} class:{morph.hediffClass.Name}");

            if (builder.Length == 0)
                Log.Warning("no morph tf loaded!");
            else
                Log.Message(builder.ToString());
        }

        /// <summary>
        ///     list all defined mutations (hediffs of the class Hediff_AddedMutation or a subtype there of)
        /// </summary>
        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void ListAllMutations()
        {
            var builder = new StringBuilder();

            var mutations =
                DefDatabase<HediffDef>.AllDefs.Where(def =>
                                                         typeof(Hediff_AddedMutation)
                                                             .IsAssignableFrom(def.hediffClass));
            var counter = 0;
            foreach (var hediffDef in mutations)
            {
                counter++;
                builder.AppendLine($"{hediffDef.defName}: ");
                builder.AppendLine($"\t\tlabel:{hediffDef.label}");
                builder.AppendLine($"\t\tdescription:{hediffDef.description}");

                var comp = hediffDef.comps?.OfType<CompProperties_MorphInfluence>().FirstOrDefault();
                if (comp != null)
                    builder.AppendLine($"\t\tmorph:{comp.morph.defName}\n\t\tinfluence:{comp.influence}");
                else
                    builder.AppendLine("\t\tno morph influence component");

                //builder.AppendLine($"\t\tcategory: {MorphUtils.GetMorphType(hediffDef)?.ToString() ?? "No category"}");
                builder.AppendLine("");
            }

            if (counter == 0)
                Log.Warning("there are no mutations loaded!");
            else
                Log.Message($"{counter} mutations loaded\n{builder}");
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void ListHybridStateOffset()
        {
            var morphs = DefDatabase<MorphDef>.AllDefs;
            var human = ThingDefOf.Human;

            var lookup = human.statBases.ToDictionary(s => s.stat, s => s.value);

            var builder = new StringBuilder();

            foreach (var morphDef in morphs)
            {
                builder.AppendLine($"{morphDef.label}:");

                foreach (var statModifier in morphDef.hybridRaceDef.statBases ?? Enumerable.Empty<StatModifier>())
                {
                    var humanVal = lookup.TryGetValue(statModifier.stat);
                    var diff = statModifier.value - humanVal;
                    var sym = diff > 0 ? "+" : "";
                    var str = $"{statModifier.stat.label}:{sym}{diff}";
                    builder.AppendLine($"\t\t{str}");
                }
            }

            Log.Message($"{builder}");
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        [ModeRestrictionPlay]
        public static void ListInteractionWeights()
        {
            Find.WindowStack.Add(new Pawnmorpher_InteractionWeightLogDialogue());
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void ListNewTfPawns()
        {
            var comp = Find.World.GetComponent<PawnmorphGameComp>();

            var strs = new List<string>();
            foreach (var tfPawn in comp.TransformedPawns)
                strs.Add($"{tfPawn.ToDebugString()} of type {tfPawn.GetType().FullName}");

            if (strs.Count > 0)
                Log.Message($"transformed pawns:\n\t{string.Join("\n\t", strs.ToArray())}");
            else
                Log.Message("no transformed pawns");
        }


        [Category(MAIN_CATEGORY_NAME)]
        [ModeRestrictionPlay]
        [DebugOutput]
        public static void LogColonyPawnStatuses()
        {
            var builder = new StringBuilder();
            foreach (var colonyPawn in PawnsFinder
                .AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
            {
                builder.AppendLine(colonyPawn.Name.ToStringFull + ":");

                var comp = colonyPawn.GetMutationTracker();
                if (comp != null)
                {
                    foreach (var kvp in comp)
                        builder.AppendLine($"\t\t{kvp.Key.defName}:{kvp.Value} normalized:{comp.GetNormalizedInfluence(kvp.Key)}");
                }
                else
                {
                    var enumer =
                        colonyPawn.GetMutationTracker() ?? Enumerable.Empty<KeyValuePair<MorphDef, float>>();


                    foreach (var keyValuePair in enumer)
                        builder.AppendLine($"\t\t{keyValuePair.Key.defName}:{keyValuePair.Value}");
                }

                builder.AppendLine($"is human:{colonyPawn.ShouldBeConsideredHuman()}\n");
            }

            Log.Message(builder.ToString());
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
                var givers =
                    def.stages?.SelectMany(s => s.hediffGivers ?? Enumerable.Empty<HediffGiver>())
                       .OfType<HediffGiver_Mutation>()
                       .Where(g => g.tale == null)
                    ?? Enumerable.Empty<HediffGiver_Mutation>();
                foreach (var hediffGiverMutation in givers)
                    yield return new Tuple<HediffDef, HediffGiver_Mutation>(def, hediffGiverMutation);
            }

            var missingGivers = DefDatabase<HediffDef>.AllDefs.Where(SelectionFunc)
                                                      .SelectMany(GetMissing)
                                                      .GroupBy(tup => tup.Second.hediff, //group by the part that needs a tale  
                                                               tup => tup
                                                                   .First); //and select the morph tfs their givers are found 
            var builder = new StringBuilder();

            var missingLst = new HashSet<HediffDef>();

            foreach (var missingGiver in missingGivers)
            {
                var keyStr = $"{missingGiver.Key.defName} is missing a tale in the following morph hediffs:";
                missingLst.Add(missingGiver.Key); //keep the keys for the summary later
                builder.AppendLine(keyStr);
                foreach (var hediffDef in missingGiver) builder.AppendLine($"\t\t{hediffDef.defName}");
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

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void OutputRelationshipPatches()
        {
            var defs = DefDatabase<PawnRelationDef>.AllDefs;

            var builder = new StringBuilder();
            foreach (var relationDef in defs)
            {
                var modPatch = relationDef.GetModExtension<RelationshipDefExtension>();
                if (modPatch != null)
                {
                    builder.AppendLine($"{relationDef.defName}:");
                    builder.AppendLine($"\t\ttransformThought:{modPatch.transformThought?.defName ?? "NULL"}");
                    builder.AppendLine($"\t\ttransformThoughtFemale:{modPatch.transformThoughtFemale?.defName ?? "NULL"}");
                    builder.AppendLine($"\t\trevertedThought:{modPatch.revertedThought?.defName ?? "NULL"}");
                    builder.AppendLine($"\t\trevertedThoughtFemale:{modPatch.revertedThoughtFemale?.defName ?? "NULL"}");
                    builder.AppendLine($"\t\tpermanentlyFeralThought:{modPatch.permanentlyFeral?.defName ?? "NULL"}");
                    builder.AppendLine($"\t\tpermanentlyFeralThought:{modPatch.permanentlyFeralFemale?.defName ?? "NULL"}");
                    builder.AppendLine($"\t\tmergedThought: {modPatch.mergedThought?.defName ?? "NULL"}");
                    builder.AppendLine($"\t\tmergedThoughtFemale: {modPatch.mergedThoughtFemale?.defName ?? "NULL"} ");
                }
                else
                {
                    builder.AppendLine($"{relationDef.defName} no mod patch found!!");
                }

                builder.AppendLine("");
            }

            Log.Message(builder.ToString());
        }
    }
}