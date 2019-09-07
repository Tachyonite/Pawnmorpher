using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AlienRace;
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
        public static bool Assert(bool condition, string message)
        {
            if (!condition) Log.Error($"assertion failed:{message}");
            return condition;
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
            var morphs = DefDatabase<MorphDef>.AllDefs;
            var human = ThingDefOf.Human;

            Dictionary<StatDef, float> lookup = human.statBases.ToDictionary(s => s.stat, s => s.value); 


            StringBuilder builder = new StringBuilder();


            foreach (MorphDef morphDef in morphs)
            {
                builder.AppendLine($"{morphDef.label}:");

                foreach (StatModifier statModifier in morphDef.hybridRaceDef.statBases ?? Enumerable.Empty<StatModifier>())
                {

                    float humanVal = lookup.TryGetValue(statModifier.stat);
                    float diff = statModifier.value - humanVal;
                    var sym = diff > 0 ? "+" : "";  
                    var str = $"{statModifier.stat.label}:{sym}{diff}";
                    builder.AppendLine($"\t\t{str}"); 
                }



            }

            Log.Message($"{builder}");


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
        [Obsolete]
        public static void LogAllTransformedPawns()
        {
            var builder = new StringBuilder();
            var comp = Find.World.GetComponent<PawnmorphGameComp>();

            foreach (PawnMorphInstance compMorphInstance in comp.MorphInstances)
            {
                Pawn animal = compMorphInstance.replacement;
                Pawn origin = compMorphInstance.origin;

                string originString, animalString;

                originString = origin == null ? "[null pawn]" : origin.Name.ToStringFull;

                animalString = animal == null
                    ? "[null animal]"
                    : $"an {animal.kindDef?.race?.label ?? "[no race]"} named {animal.Name.ToStringFull}";


                builder.AppendLine($"{originString} is now a {animalString}");
            }

            foreach (PawnMorphInstanceMerged pawnMorphInstanceMerged in comp.MergeInstances)
            {
                Pawn merge = pawnMorphInstanceMerged.replacement;
                Pawn p0 = pawnMorphInstanceMerged.origin;
                Pawn p1 = pawnMorphInstanceMerged.origin2;

                string p0Str, p1Str, mergeStr;
                p0Str = p0 == null ? "[null pawn]" : p0.Name.ToStringFull;

                p1Str = p1 == null ? "[null pawn]" : p1.Name.ToStringFull;

                mergeStr = merge == null
                    ? "[null animal]"
                    : $"a {merge.kindDef?.race?.label ?? "[no race]"} called {merge.Name.ToStringFull}";


                builder.AppendLine($"{p0Str} and {p1Str} are now {mergeStr}");
            }


            if (builder.Length == 0)
                Log.Message("there are now transformed or merged pawns");
            else
                Log.Message(builder.ToString());
        }

        [Category(MAIN_CATEGORY_NAME)]
        [ModeRestrictionPlay]
        [DebugOutput]
        public static void LogColonyPawnStatuses()
        {
            var builder = new StringBuilder();
            var dict = new Dictionary<MorphDef, float>();
            foreach (Pawn colonyPawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
            {
                colonyPawn.GetMorphInfluences(dict);

                builder.AppendLine(colonyPawn.Name.ToStringFull + ":");
                foreach (KeyValuePair<MorphDef, float> keyValuePair in dict)
                    builder.AppendLine($"\t\t{keyValuePair.Key.defName}:{keyValuePair.Value}");

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
            bool SelectionFunc(HediffDef def) //local function to grab all morph hediffDefs that have givers that are missing tales 
            {
                if (!typeof(Hediff_Morph).IsAssignableFrom(def.hediffClass)) return false;
                return (def.stages ?? Enumerable.Empty<HediffStage>())
                      .SelectMany(s => s.hediffGivers ?? Enumerable.Empty<HediffGiver>())
                      .OfType<HediffGiver_Mutation>()
                      .Any(g => g.tale == null);
            }

            IEnumerable<Tuple<HediffDef, HediffGiver_Mutation>> GetMissing(HediffDef def) //local function that will grab all hediff givers missing a tale 
            {                                                                           //and keep the def it came from around 
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
                                                                                                        tup => tup.First); //and select the morph tfs their givers are found 
            var builder = new StringBuilder();

            HashSet<HediffDef> missingLst = new HashSet<HediffDef>(); 

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
                Log.Message("All parts have a tale");
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
    }
}