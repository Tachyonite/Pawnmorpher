// DebugLogUtils.cs modified by Iron Wolf for Pawnmorph on 07/30/2019 6:01 PM
// last updated 07/30/2019  6:01 PM

using System.Collections.Generic;
using System.Linq;
using System.Text;
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

                var comp = hediffDef.comps?.OfType<Hediffs.CompProperties_MorphInfluence>().FirstOrDefault();
                if (comp != null)
                {
                    builder.AppendLine($"\t\tmorph:{comp.morph.defName}\n\t\tinfluence:{comp.influence}"); 
                }
                else
                {
                    builder.AppendLine("\t\tno morph influence component"); 
                }

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

        /// <summary>
        ///     this outputs a collection of all mutations that are given by only one mutation hediff grouped by mutation hediff to
        ///     the log.
        ///     this is to generate the "expected" morphs and their unique parts for debug and development use
        /// </summary>
        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void GetExpectedMorphsAndParts()
        {
            IEnumerable<HediffDef> morphs =
                TfDefOf.AllMorphs.Where(def => def.defName != CHAOMORPH_DEF_NAME && def.defName != DAMAGE_DEF_NAME);

            Dictionary<HediffDef, List<HediffDef>> dict = morphs.Select(m => new
                                                                 {
                                                                     morph = m,
                                                                     mutations =
                                                                         m.stages?.SelectMany(s => s.hediffGivers
                                                                                                ?? Enumerable
                                                                                                      .Empty<HediffGiver>())
                                                ?
                                               .Where(h => typeof(Hediff_AddedMutation)
                                                         .IsAssignableFrom(h.hediff
                                                                            .hediffClass))
                                                                         ?.Select(h => h.hediff)
                                                                      ?? Enumerable
                                                                            .Empty<HediffDef
                                                                             >() //get all givers that give a mutation
                                                                 })
                                                                .GroupBy(a => a.morph,
                                                                         a => a
                                                                            .mutations) //group by morph but select only the mutations 
                                                                .ToDictionary(g => g.Key,
                                                                              g => g?.SelectMany(c => c)
                                                                                     .ToList()); //convert to a dictionary 


            var nonUnique = new HashSet<HediffDef>();

            List<HediffDef> keys = dict.Keys.ToList();

            for (var i = 0; i < keys.Count - 1; i++) //now get all the mutations that are in two or more mutation hediffs 
            {
                HediffDef iMorph = keys[i];

                for (int j = i + 1; j < keys.Count; j++)
                {
                    HediffDef jMorph = keys[j];

                    foreach (HediffDef mutation in dict[iMorph])
                    {
                        if (nonUnique.Contains(mutation)) continue; //if already added we don't need to check again 
                        if (dict[jMorph].Contains(mutation))
                            nonUnique.Add(mutation);
                    }
                }
            }

            //now get rid of all the non unique ones 
            foreach (HediffDef nonUniqueDef in nonUnique)
            foreach (List<HediffDef> hediffDefs in dict.Values)
                hediffDefs.Remove(nonUniqueDef);

            //now generate the log Message 
            var builder = new StringBuilder();


            foreach (KeyValuePair<HediffDef, List<HediffDef>> keyValuePair in dict)
            {
                List<HediffDef> lst = keyValuePair.Value;
                if (lst.Count == 0)
                {
                    builder.AppendLine($"morph {keyValuePair.Key.defName} has no unique parts!\n");
                }
                else
                {
                    builder.AppendLine($"morph {keyValuePair.Key.defName}:");
                    foreach (HediffDef mutation in lst) builder.AppendLine($"\t\tdef:{mutation.defName} label:{mutation.label}");

                    builder.AppendLine("");
                }
            }

            if (builder.Length == 0)
                Log.Warning("there are not morphs?");
            else
                Log.Message($"unique morph mutations:\n{builder}");
        }


        [Category(MAIN_CATEGORY_NAME)]
        [ModeRestrictionPlay]
        [DebugOutput]
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
        [ModeRestrictionPlay, DebugOutput]
        public static void LogColonyPawnStatuses()
        {

            StringBuilder builder = new StringBuilder();
            Dictionary<MorphDef, float> dict = new Dictionary<MorphDef, float>(); 
            foreach (Pawn colonyPawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
            {
                colonyPawn.GetMorphInfluences(dict);

                builder.AppendLine(colonyPawn.Name.ToStringFull + ":");
                foreach (KeyValuePair<MorphDef, float> keyValuePair in dict)
                {
                    builder.AppendLine($"\t\t{keyValuePair.Key.defName}:{keyValuePair.Value}"); 

                }

                builder.AppendLine($"is human:{colonyPawn.ShouldBeConsideredHuman()}\n"); 


            }

            Log.Message(builder.ToString()); 
        }

    }
}