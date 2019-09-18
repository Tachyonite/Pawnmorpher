using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.TfSys;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.DebugUtils
{
    [HasDebugOutput]
    public static class DebugLogUtils
    {

        /// <summary>
        /// Asserts the specified condition. if false an error message will be displayed
        /// </summary>
        /// <param name="condition">if false will display an error message</param>
        /// <param name="message">The message.</param>
        /// <returns>the condition</returns>
        [DebuggerHidden]
        [Conditional("DEBUG"), AssertionMethod]
        public static void Assert(bool condition, string message)
        {
            if (!condition) Log.Error($"assertion failed:{message}");
        }


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void GetThoughtlessMutations()
        {
            IEnumerable<VTuple<HediffDef, HediffGiver_Mutation>> SelectionFunc(HediffDef def)
            {
                foreach (HediffGiver_Mutation hediffGiverMutation in def.GetAllHediffGivers().OfType<HediffGiver_Mutation>())
                {
                    if (hediffGiverMutation.memory != null) continue; //if it has a memory associated with it ignore it 

                    yield return new VTuple<HediffDef, HediffGiver_Mutation>(def, hediffGiverMutation);
                }
            }

            List<IGrouping<HediffDef, HediffGiver_Mutation>> allGiversWithData = DefDatabase<HediffDef>
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
            IEnumerable<HediffDef> allMuts = allGiversWithData.SelectMany(g => g.Select(mG => mG.hediff)).Distinct();
            foreach (HediffDef mutation in allMuts) builder.AppendLine(mutation.defName);


            builder.AppendLine("---------Giver Locations----------------");

            foreach (IGrouping<HediffDef, HediffGiver_Mutation> group in allGiversWithData)
            {
                builder.AppendLine($"{group.Key.defName} contains givers of the following mutations that do not have memories with them:");
                foreach (HediffDef mutation in group.Select(g => g.hediff)) builder.AppendLine($"\t\t{mutation.defName}");
            }

            Log.Message(builder.ToString());
        }


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput, ModeRestrictionPlay]
        public static void CheckMorphTracker()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Pawn allMapsFreeColonist in PawnsFinder.AllMaps_FreeColonists)
            {
                if (allMapsFreeColonist.GetComp<MorphTrackingComp>() == null)
                {
                    builder.AppendLine($"{allMapsFreeColonist.Name} does not have a morphTracker!");
                }
            }

            if (builder.Length > 0)
            {
                Log.Warning(builder.ToString());
                builder = new StringBuilder();
            }

            var map = Find.CurrentMap;
            if (map == null) return;
            var comp = map.GetComponent<MorphTracker>();

            foreach (MorphDef morph in DefDatabase<MorphDef>.AllDefs)
            {
                var i = comp[morph];
                builder.AppendLine($"{morph.defName}={i}");
            }


            builder.AppendLine("--------------Groups------------");

            foreach (MorphGroupDef morphGroupDef in DefDatabase<MorphGroupDef>.AllDefs)
            {
                int counter = 0;
                foreach (MorphDef morphDef in morphGroupDef.MorphsInGroup)
                {
                    counter += comp[morphDef];
                }

                builder.AppendLine($"{morphGroupDef.defName}={counter}");
            }



            Log.Message(builder.ToString()); 

        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void OutputRelationshipPatches()
        {
            IEnumerable<PawnRelationDef> defs = DefDatabase<PawnRelationDef>.AllDefs;

            var builder = new StringBuilder();
            foreach (PawnRelationDef relationDef in defs)
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

        public const string MAIN_CATEGORY_NAME = "Pawnmorpher";

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void GetMutationsWithStages()
        {
            var allMutations = DefDatabase<HediffDef>.AllDefs.Where(def => typeof(Hediff_AddedMutation).IsAssignableFrom(def.hediffClass))
                                                     .Where(def => def.stages?.Count > 1 && def.HasComp(typeof(HediffComp_SeverityPerDay)))
                                                     .ToList();


            if (allMutations.Count == 0)
            {
                Log.Message($"no mutations with multiple stages");
                return;
            }
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"mutations:");
            foreach (HediffDef allMutation in allMutations)
            {
                builder.AppendLine($"\t{allMutation.defName}[{allMutation.stages.Count}]");
            }

            builder.AppendLine($"\n------------Stats--------------\n");


            foreach (HediffDef mutation in allMutations)
            {
                var stages = mutation.stages;
                var severityPerDay = mutation.CompProps<HediffCompProperties_SeverityPerDay>().severityPerDay;
                builder.AppendLine($"{mutation.defName}:");
                float total = 0;  
                for (int i = 0; i < stages.Count - 1; i++)
                {
                    var s = stages[i];
                    var s1 = stages[i + 1];
                    var sLabel = string.IsNullOrEmpty(s.label) ? i.ToString() : s.label;
                    var s1Label = string.IsNullOrEmpty(s1.label) ? (i + 1).ToString() : s1.label; //if the label is null just use the index  
                    var diff = s1.minSeverity - s.minSeverity;
                    var tDiff = diff / severityPerDay;
                    total += tDiff; 
                    builder.AppendLine($"\t\tstage[{sLabel}] {{{s.minSeverity}}} => stage[{s1Label}] {{{s1.minSeverity}}} takes {tDiff} days");
                }

                builder.AppendLine($"\ttotal time is {total} days");
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
            IEnumerable<HediffDef> morphs = MorphTransformationDefOf.AllMorphs;
            foreach (HediffDef morph in morphs)
                builder.AppendLine($"defName:{morph.defName} label:{morph.label} class:{morph.hediffClass.Name}");

            if (builder.Length == 0)
                Log.Warning("no morph tf loaded!");
            else
                Log.Message(builder.ToString());
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput, ModeRestrictionPlay]
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
            foreach (TransformedPawn tfPawn in comp.TransformedPawns)
                strs.Add($"{tfPawn.ToDebugString()} of type {tfPawn.GetType().FullName}");

            if (strs.Count > 0)
                Log.Message($"transformed pawns:\n\t{string.Join("\n\t", strs.ToArray())}");
            else
                Log.Message("no transformed pawns");
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


        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void FindMissingMorphDescriptions()
        {
            List<MorphDef> morphs = DefDatabase<MorphDef>.AllDefs.Where(def => string.IsNullOrEmpty(def.description)).ToList();
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
                if (!typeof(Hediff_AddedMutation).IsAssignableFrom(def.hediffClass)) return false; //must be mutation hediff 
                return string.IsNullOrEmpty(def.description);
            }

            IEnumerable<HediffDef> mutations = DefDatabase<HediffDef>.AllDefs.Where(SelectionFunc);

            string str = string.Join("\n\t", mutations.Select(m => m.defName).ToArray());

            Log.Message(string.IsNullOrEmpty(str) ? "no parts with missing description" : str);
        }

        /// <summary>
        ///     list all defined mutations (hediffs of the class Hediff_AddedMutation or a subtype there of)
        /// </summary>
        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void ListAllMutations()
        {
            var builder = new StringBuilder();

            IEnumerable<HediffDef> mutations =
                DefDatabase<HediffDef>.AllDefs.Where(def => typeof(Hediff_AddedMutation).IsAssignableFrom(def.hediffClass));
            var counter = 0;
            foreach (HediffDef hediffDef in mutations)
            {
                counter++;
                builder.AppendLine($"{hediffDef.defName}: ");
                builder.AppendLine($"\t\tlabel:{hediffDef.label}");
                builder.AppendLine($"\t\tdescription:{hediffDef.description}");

                CompProperties_MorphInfluence comp = hediffDef.comps?.OfType<CompProperties_MorphInfluence>().FirstOrDefault();
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
        [ModeRestrictionPlay]
        [DebugOutput]
        public static void LogColonyPawnStatuses()
        {
            var builder = new StringBuilder();
            foreach (Pawn colonyPawn in PawnsFinder
               .AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
            {
                builder.AppendLine(colonyPawn.Name.ToStringFull + ":");

                MutationTracker comp = colonyPawn.GetMutationTracker();
                if (comp != null)
                {
                    foreach (KeyValuePair<MorphDef, float> kvp in comp)
                        builder.AppendLine($"\t\t{kvp.Key.defName}:{kvp.Value} normalized:{comp.GetNormalizedInfluence(kvp.Key)}");
                }
                else
                {
                    IEnumerable<KeyValuePair<MorphDef, float>> enumer =
                        colonyPawn.GetMutationTracker() ?? Enumerable.Empty<KeyValuePair<MorphDef, float>>();


                    foreach (KeyValuePair<MorphDef, float> keyValuePair in enumer)
                        builder.AppendLine($"\t\t{keyValuePair.Key.defName}:{keyValuePair.Value}");
                }

                builder.AppendLine($"is human:{colonyPawn.ShouldBeConsideredHuman()}\n");
            }

            Log.Message(builder.ToString());
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        [ModeRestrictionPlay]
        public static void OpenActionMenu()
        {
            Find.WindowStack.Add(new Pawnmorpher_DebugDialogue());
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

        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
        public static void CheckBodyHediffGraphics()
        {
            IEnumerable<HediffGiver_Mutation> GetMutationGivers(IEnumerable<HediffStage> stages)
            {
                return stages.SelectMany(s => s.hediffGivers ?? Enumerable.Empty<HediffGiver>()).OfType<HediffGiver_Mutation>();
            }


            var givers = DefDatabase<HediffDef>.AllDefs.Where(def => typeof(Hediff_Morph).IsAssignableFrom(def.hediffClass))
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

        [DebugOutput, Category(MAIN_CATEGORY_NAME)]
        public static void GetMutationsWithoutStages()
        {
            var allMutations = MutationUtilities.AllMutations.Where(def => (def.stages?.Count ?? 0) <= 1); //all mutations without stages 
            StringBuilder builder = new StringBuilder();


            foreach (HediffDef allMutation in allMutations)
            {
                builder.AppendLine($"{allMutation.defName}"); 
            }

            Log.Message(builder.ToString()); 
        }
    }
}