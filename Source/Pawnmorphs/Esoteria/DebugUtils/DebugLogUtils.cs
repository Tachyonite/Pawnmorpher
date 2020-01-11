// DebugLogUtils.cs created by Iron Wolf for Pawnmorph on 09/23/2019 7:54 AM
// last updated 09/27/2019  8:00 AM

using System.Collections.Generic;
using System.Diagnostics;
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

            foreach (ThoughtDef thoughtDef in DefDatabase<ThoughtDef>.AllDefs)
            {
                var addedHeader = false;
                for (var index = 0; index < (thoughtDef?.stages?.Count ?? 0); index++)
                {
                    ThoughtStage stage = thoughtDef?.stages?[index];
                    if (stage == null) continue;
                    if (stage.label == "TODO" || stage.description == "TODO")
                    {
                        if (!addedHeader)
                        {
                            builder.AppendLine($"In {thoughtDef.defName}:");
                            addedHeader = true;
                        }

                        builder.AppendLine($"{index}) label:{stage.label} description:\"{stage.description}\"".Indented());
                    }
                }
            }

            Log.Message(builder.ToString());
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
                if (!typeof(Hediff_AddedMutation).IsAssignableFrom(def.hediffClass))
                    return false; //must be mutation hediff 
                return string.IsNullOrEmpty(def.description);
            }

            IEnumerable<HediffDef> mutations = DefDatabase<HediffDef>.AllDefs.Where(SelectionFunc);

            string str = string.Join("\n\t", mutations.Select(m => m.defName).ToArray());

            Log.Message(string.IsNullOrEmpty(str) ? "no parts with missing description" : str);
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


            List<MorphDef> bPMorphsR = DefDatabase<ThoughtDef>.AllDefs
                                                              .OfType<Def_MorphThought>()
                                                              .Where(d => IsMorphReaction(d, TraitDefOf.BodyPurist))
                                                              .Select(d => d.morph)
                                                              .ToList();

            List<MorphDef> furryMorphR = DefDatabase<ThoughtDef>.AllDefs.OfType<Def_MorphThought>()
                                                                .Where(d => IsMorphReaction(d, PMTraitDefOf.MutationAffinity))
                                                                .Select(d => d.morph)
                                                                .ToList();

            List<MorphDef> morphsMissingMemories = MorphDef.AllDefs
                                                           .Where(d => d.transformSettings?.transformationMemory == null)
                                                           .ToList();

            var builder = new StringBuilder();


            var missingBPMorphReactions = new List<MorphDef>();
            var missingFurryMorphReactions = new List<MorphDef>();

            foreach (MorphDef morphDef in MorphDef.AllDefs)
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
                builder.AppendLine("-------- Missing Furry Reactions ---------");
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
            IEnumerable<HediffDef> allMutations =
                MutationUtilities.AllMutations.Where(def => (def.stages?.Count ?? 0) <= 1); //all mutations without stages 
            var builder = new StringBuilder();


            foreach (HediffDef allMutation in allMutations) builder.AppendLine($"{allMutation.defName}");

            Log.Message(builder.ToString());
        }

        [Category(MAIN_CATEGORY_NAME)]
        [DebugOutput]
        public static void GetMutationsWithStages()
        {
            List<HediffDef> allMutations = DefDatabase<HediffDef>
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
            foreach (HediffDef allMutation in allMutations)
                builder.AppendLine($"\t{allMutation.defName}[{allMutation.stages.Count}]");

            builder.AppendLine("\n------------Stats--------------\n");


            foreach (HediffDef mutation in allMutations)
            {
                List<HediffStage> stages = mutation.stages;
                float severityPerDay = mutation.CompProps<HediffCompProperties_SeverityPerDay>().severityPerDay;
                builder.AppendLine($"{mutation.defName}:");
                float total = 0;
                for (var i = 0; i < stages.Count - 1; i++)
                {
                    HediffStage s = stages[i];
                    HediffStage s1 = stages[i + 1];
                    string sLabel = string.IsNullOrEmpty(s.label) ? i.ToString() : s.label;
                    string s1Label =
                        string.IsNullOrEmpty(s1.label)
                            ? (i + 1).ToString()
                            : s1.label; //if the label is null just use the index  
                    float diff = s1.minSeverity - s.minSeverity;
                    float tDiff = diff / severityPerDay;
                    total += tDiff;
                    builder.AppendLine($"\t\tstage[{sLabel}] {{{s.minSeverity}}} => stage[{s1Label}] {{{s1.minSeverity}}} takes {tDiff} days");
                }

                builder.AppendLine($"\ttotal time is {total} days");
            }

            Log.Message(builder.ToString());
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


            var allSpreadableMutations = MutationUtilities.AllMutations.Select(mut => new
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
                                                                                .GroupBy(vT => vT.First,
                                                                                         vT => vT.Second) //group by morph Hediff 
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

        [DebugOutput, Category(MAIN_CATEGORY_NAME)]
        static void MaximumMutationPointsForHumans()
        {
            Log.Message(MorphUtilities.MaxHumanInfluence.ToString());
        }

        [DebugOutput, Category(MAIN_CATEGORY_NAME)]
        static void ListAllSpreadableParts()
        {
            var bDef = BodyDefOf.Human;
            var sBuilder = new StringBuilder();

            foreach (var allMutablePart in bDef.GetAllMutableParts().Select(b => b.def).Distinct())
            {
                if (allMutablePart.IsSkinCoveredInDefinition_Debug)
                    sBuilder.AppendLine($"<li>{allMutablePart.defName}</li>");
            }

            Log.Message(sBuilder.ToString()); 

        }
    }
}