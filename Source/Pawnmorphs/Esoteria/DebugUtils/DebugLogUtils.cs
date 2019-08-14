// DebugLogUtils.cs modified by Iron Wolf for Pawnmorph on 07/30/2019 6:01 PM
// last updated 07/30/2019  6:01 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
using Pawnmorph.Hediffs;
using Pawnmorph.Thoughts;
using RimWorld;
using Verse;

namespace Pawnmorph.DebugUtils
{
    [HasDebugOutput]
    public static class DebugLogUtils
    {
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
            IEnumerable<HediffDef> morphs = TfDefOf.AllMorphs;
            foreach (HediffDef morph in morphs)
                builder.AppendLine($"defName:{morph.defName} label:{morph.label} class:{morph.hediffClass.Name}");

            if (builder.Length == 0)
                Log.Warning("no morph tf loaded!");
            else
                Log.Message(builder.ToString());
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void ListNewTfPawns()
        {
            var comp = Find.World.GetComponent<PawnmorphGameComp>();

            List<string> strs = new List<string>();
            foreach (var tfPawn in comp.TransformedPawns)
            {
                strs.Add($"{tfPawn.ToDebugString()} of type {tfPawn.GetType().FullName}");
            }

            if (strs.Count > 0)
            {
                Log.Message($"transformed pawns:\n\t{string.Join("\n\t", strs.ToArray())}");
            }
            else
            {
                Log.Message("no transformed pawns");
            }
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


        private const string CHAOMORPH_DEF_NAME = "FullRandomTFAnyOutcome";
        private const string DAMAGE_DEF_NAME = "PawnmorphGunshotTF";


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
            IEnumerable<HediffDef> allMutations = TfDefOf.AllMorphs;
            var mainBuilder = new StringBuilder();

            foreach (HediffDef transformHediff in allMutations)
            {
                var missingGivers = transformHediff.stages?.Select((s, i) => new
                                                    {
                                                        givers = s.hediffGivers?.OfType<HediffGiver_Mutation>()
                                                                  .Where(g => g.tale == null)
                                                              ?? Enumerable
                                                                    .Empty<HediffGiver_Mutation
                                                                     >(), //get all givers that are missing a tale, keep the index to
                                                        stageIndex = i
                                                    })
                                                   .Select(a => new
                                                    {
                                                        giversLst = a.givers
                                                                     .ToList(), //get how many and keep'em around to loop again 
                                                        index = a.stageIndex
                                                    })
                                                   .Where(a => a.giversLst.Count > 0)
                                                   .ToList(); //evil linq statement is ok because this is only a debug command 

                if (missingGivers == null) continue;
                if (missingGivers.Count == 0) continue;

                mainBuilder.AppendLine($"in hediff {transformHediff.defName}:");
                foreach (var missingGiver in missingGivers)
                {
                    mainBuilder.AppendLine($"in stage {missingGiver.index}");
                    foreach (HediffGiver_Mutation hediffGiverMutation in missingGiver.giversLst)
                        mainBuilder.AppendLine($"\t\tgiver of {hediffGiverMutation.hediff.defName} is missing a tale");

                    mainBuilder.AppendLine("");
                }

                if (mainBuilder.Length > 0)
                    Log.Message(mainBuilder.ToString());
                else
                    Log.Message("no missing tales in transformations!");
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
    }
}