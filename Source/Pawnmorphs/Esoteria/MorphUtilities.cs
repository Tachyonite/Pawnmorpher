// MorphUtilities.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 3:48 PM
// last updated 08/02/2019  3:48 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// Static collection of useful morph related functions. <br/>
    /// TransformerUtilities was getting a bit crowded.
    /// </summary>
    public static class MorphUtilities
    {
        /// <summary>
        /// scalar used to make it easier for pawns to become hybrids
        /// </summary>
        public const float HUMAN_CHANGE_FACTOR = 0.65f;

        private static List<BodyPartRecord> _possibleRecordsList;
        private static Dictionary<BodyPartDef, List<HediffGiver_Mutation>> _giversPerPartCache =
            new Dictionary<BodyPartDef, List<HediffGiver_Mutation>>();
        private static Dictionary<HediffDef, List<MorphDef>> _morphAssociationCache =
            new Dictionary<HediffDef, List<MorphDef>>(); // So we don't calculate the associations more then we have to.
        private static float? _maxHumanInfluence;

        static MorphUtilities() //this is really hacky 
        {
            IEnumerable<HediffDef> defs = MorphTransformationDefOf.AllMorphs;

            PossiblePartsLst = defs.SelectMany(def => def.stages ?? Enumerable.Empty<HediffStage>())
                                .SelectMany(s => s.hediffGivers ?? Enumerable.Empty<HediffGiver>())
                                .Where(giver => typeof(Hediff_AddedMutation).IsAssignableFrom(giver.hediff.hediffClass)) //get all hediff givers giving out mutations  
                                .SelectMany(g => g.partsToAffect ?? Enumerable.Empty<BodyPartDef>())
                                .Distinct() //get rid of duplicates 
                                .ToList(); //make a list to save the result 
            // Doing this to get the number of body parts that can have mutations so we can calculate how "human" a pawn is 
            // This feels wrong, but I can't think of a better way to calculate the "human-ness" of a pawn  - Iron 
        }

        private static List<BodyPartDef> PossiblePartsLst { get; }

        /// <summary>
        /// an enumerable collection of all body part defs that can have mutations 
        /// </summary>
        public static IEnumerable<BodyPartDef> PartsWithPossibleMutations => PossiblePartsLst;

        /// <summary>
        /// the total number of body part defs that can have mutations 
        /// </summary>
        public static int NumPartsWithPossibleMutations => PossiblePartsLst.Count;

        /// <summary> Enumerable collection of all mutation hediffs. </summary>
        public static IEnumerable<HediffDef> AllMutations
            => DefDatabase<HediffDef>.AllDefs.Where(d => typeof(Hediff_AddedMutation).IsAssignableFrom(d.hediffClass));

        /// <summary> The maximum possible human influence. </summary>
        public static float MaxHumanInfluence
        {
            get
            {
                if (_maxHumanInfluence == null)
                {
                    var body = BodyDefOf.Human;
                    _maxHumanInfluence = body.AllParts.Count(p => PossiblePartsLst.Contains(p.def));
                }

                return _maxHumanInfluence.Value;
            }
        }

        /// <summary> An enumerable collection of all body part records in humans that can be affected by a mutation. </summary>
        public static IEnumerable<BodyPartRecord> AllMutableRecords
        {
            get
            {
                if (_possibleRecordsList == null)
                {
                    var body = BodyDefOf.Human;
                    _possibleRecordsList = body.AllParts.Where(p => PossiblePartsLst.Contains(p.def)).ToList();

                }

                return _possibleRecordsList;
            }
        }

        /// <summary> Get an enumerable collection of mutation givers that can affect the given part. </summary>
        public static IEnumerable<HediffGiver_Mutation> GetMutationGivers(BodyPartDef partDef)
        {
            if (_giversPerPartCache.TryGetValue(partDef, out List<HediffGiver_Mutation> mutations)) return mutations;

            IEnumerable<HediffGiver_Mutation> SelectGivers(HediffDef def)
            {
                IGrouping<HediffDef, HediffGiver_Mutation> givers = def.GetAllHediffGivers()
                                                                       .OfType<HediffGiver_Mutation>()
                                                                       .Where(g => g.partsToAffect?.Contains(partDef)
                                                                                ?? //select the givers that can affecet the given part 
                                                                                   false)
                                                                       .GroupBy(g => g.hediff) //grab only 1 giver per hediff 
                                                                       .FirstOrDefault();
                return givers ?? Enumerable.Empty<HediffGiver_Mutation>();
            }

            mutations = DefDatabase<HediffDef>
                       .AllDefs.Where(h => typeof(Hediff_Morph).IsAssignableFrom(h.hediffClass)) //grab only morph hediffs 
                       .SelectMany(SelectGivers)
                       .ToList(); //convert to list to save for later 
            _giversPerPartCache[partDef] = mutations; //save the result so we only have to look this info up once 
            return mutations;
        }

        /// <summary> Get all morphs defs associated with this transformation hediff def. </summary>
        /// <param name="transformationDef"> The transformation definition. </param>
        static IEnumerable<MorphDef> GetAssociatedMorphInternal(HediffDef transformationDef) //might want to add it the hediff defs themselves rather then check at runtime 
        {
            var mutationsGiven =
                transformationDef.stages?.SelectMany(s => s.hediffGivers?.OfType<HediffGiver_Mutation>()
                                                       ?? Enumerable.Empty<HediffGiver_Mutation>())
             ?? Enumerable.Empty<HediffGiver_Mutation>(); //all mutations in the def 

            foreach (HediffGiver_Mutation hediffGiverMutation in mutationsGiven)
            {
                var comps = hediffGiverMutation.hediff.comps?.OfType<CompProperties_MorphInfluence>();

                if (comps == null) continue;

                foreach (CompProperties_MorphInfluence morphInfluence in comps)
                {
                    yield return morphInfluence.morph; 
                }
            }
        }

        [CanBeNull]
        public static MorphGroupDef GetMorphGroup([NotNull] this Pawn pawn)
        {
            if (pawn.def == ThingDefOf.Human) return MorphGroupDefOf.Humans;

            return pawn.def.GetMorphOfRace()?.@group; 

        }

        /// <summary> Get the morph tracking component on this pawn. </summary>
        public static MorphTrackingComp GetTrackerComp([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            var comp = pawn.GetComp<MorphTrackingComp>();
            if (comp == null)
            {
                comp = new MorphTrackingComp();
                pawn.AllComps.Add(comp);
                comp.PostSpawnSetup(true); 
            }

            return comp;
        }

        /// <summary> Gets all morphDefs associated with the given transformation. </summary>
        /// <param name="transformationDef"> The transformation definition. </param>
        public static IEnumerable<MorphDef> GetAssociatedMorph(HediffDef transformationDef)
        {
            if (_morphAssociationCache.TryGetValue(transformationDef, out List<MorphDef> lst))
            {
                return lst; 
            }

            lst = GetAssociatedMorphInternal(transformationDef).ToList();
            _morphAssociationCache[transformationDef] = lst;
            return lst;
        }


        /// <summary> Gets the morphs in map. </summary>
        /// <param name="map"> The map. </param>
        /// <param name="morph"> The morph. </param>
        /// <exception cref="System.ArgumentNullException"> map or morph is null. </exception>
        public static IEnumerable<Pawn> GetMorphsInMap([NotNull] this Map map, [NotNull] MorphDef morph)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (morph == null) throw new ArgumentNullException(nameof(morph));

            return map.listerThings.ThingsOfDef(morph.hybridRaceDef).OfType<Pawn>();
        }

        /// <summary>
        /// simple value tuple struct 
        /// </summary>
        public struct Tuple //TODO move to outer scope and make this generic 
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="morph"></param>
            /// <param name="influence"></param>
            public Tuple(MorphDef morph, float influence)
            {
                this.morph = morph;
                this.influence = influence;
            }
#pragma warning disable 1591
            public MorphDef morph;
            public float influence;
#pragma warning restore 
        }

        /// <summary> Group the morph influences on this collection of hediff_added mutations. </summary>
        public static IEnumerable<VTuple<MorphDef, float>> GetInfluences(
            [NotNull] this IEnumerable<Hediff_AddedMutation> mutations)
        {
            if (mutations == null) throw new ArgumentNullException(nameof(mutations));
            IEnumerable<IGrouping<MorphDef, float>> linq = mutations.Where(m => m?.Influence?.Props?.morph != null)
                                                                    .GroupBy(m => m.Influence.Props.morph,
                                                                             m => m.Influence?.Props.influence ?? 0);


            foreach (IGrouping<MorphDef, float> grouping in linq)
            {
                MorphDef key = grouping.Key;
                float accum = 0;
                foreach (float f in grouping) accum += f;

                yield return new VTuple<MorphDef,float>(key, accum); 
            }
        }

        /// <summary> Group the morph influences on this collection of hediffDefs. </summary>
        public static IEnumerable<VTuple<MorphDef, float>> GetMorphInfluences([NotNull] this IEnumerable<HediffDef> mutationDefs)
        {
            if (mutationDefs == null) throw new ArgumentNullException(nameof(mutationDefs));

            Type tp = typeof(Hediff_AddedMutation);
            IEnumerable<IGrouping<MorphDef, float>> linq = mutationDefs
                                                          .Where(def => tp.IsAssignableFrom(def.hediffClass)
                                                                     && (def.comps?.Count ?? 0) > 0)
                                                          .Select(def => def.comps.OfType<CompProperties_MorphInfluence>()
                                                                            .FirstOrDefault())
                                                          .Where(comp => comp != null)
                                                          .GroupBy(c => c.morph,
                                                                   c => c.influence); //will fail if any comps have null morphs 

            foreach (IGrouping<MorphDef, float> grouping in linq)
            {
                MorphDef morph = grouping.Key;
                float accum = 0;

                foreach (float f in grouping) accum += f;

                yield return new VTuple<MorphDef,float>(morph, accum);
            }
        }

        /// <summary> Gets the type of the transformation. </summary>
        /// <param name="inst"> The instance. </param>
        /// <returns> The type of the transformation. </returns>
        /// <exception cref="ArgumentNullException"> inst is null </exception>
        public static MorphTransformationTypes GetTransformationType([NotNull] this HediffDef inst)
        {
            if (inst == null) throw new ArgumentNullException(nameof(inst));
            if (!typeof(Hediff_Morph).IsAssignableFrom(inst.hediffClass)) return 0;

            var comp = inst.CompProps<HediffCompProperties_Single>();
            return comp == null ? MorphTransformationTypes.Full : MorphTransformationTypes.Partial;
        }

        /// <summary>
        /// Calculate all morph influences on a pawn. <br />
        /// Use this if there are many calculations, it's more efficient and easier on memory.
        /// </summary>
        /// <param name="fillDict"> The dictionary to fill. </param>
        /// <param name="normalize"> Whether or not to normalize the influences. </param>
        [Obsolete("use " + nameof(MutationTracker) + " instead")]
        public static void GetMorphInfluences([NotNull] this Pawn pawn, Dictionary<MorphDef, float> fillDict,
                                              bool normalize = false)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            float total = 0;
            if (pawn.health?.hediffSet?.hediffs == null) return;
            fillDict.Clear();
            foreach (Hediff_AddedMutation hediffAddedMutation in pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>())
            {
                CompProperties_MorphInfluence comp = hediffAddedMutation.Influence?.Props;
                if (comp == null) continue;

                float a;
                fillDict.TryGetValue(comp.morph, out a);
                a += comp.influence;
                total += comp.influence;
                fillDict[comp.morph] = a;
            }

            if (normalize && total > 0.001f)
                foreach (MorphDef fillDictKey in fillDict.Keys)
                    fillDict[fillDictKey] /= total;
        }

        /// <summary>
        /// Calculate all the influences on this pawn by morph category. <br />
        /// Use this if there are many calculations, it's more efficient and easier on memory.
        /// </summary>
        /// <param name="normalize"> Whether or not the resulting dict should be normalized. </param>
        public static void GetMorphCategoriesInfluences([NotNull] this Pawn pawn, Dictionary<MorphCategoryDef, float> fillDict,
                                                         bool normalize = false)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            fillDict.Clear();
            if (pawn.health?.hediffSet?.hediffs == null) return;
            var tracker = pawn.GetMutationTracker();
            if (tracker == null) return;  
            
            float total = 0;

            foreach (var kvp in tracker)
            {
                foreach (var category in kvp.Key.categories ?? Enumerable.Empty<MorphCategoryDef>())
                {
                    fillDict[category] = fillDict.TryGetValue(category) + kvp.Value; 
                }
            }

            if (normalize && total > 0.001f) // Prevent division by zero.
            {
                foreach (var fillDictKey in fillDict.Keys)
                {
                    fillDict[fillDictKey] /= total; 
                }
            }
        }

        /// <summary> Calculate all the influences on this pawn by morph category. </summary>
        /// <param name="normalize"> Whether or not the resulting dict should be normalized. </param>
        public static Dictionary<MorphCategoryDef, float> GetMorphCategoriesInfluences([NotNull] this Pawn pawn, bool normalize = false)
        {
            Dictionary<MorphCategoryDef, float> dict = new Dictionary<MorphCategoryDef, float>();
            GetMorphCategoriesInfluences(pawn, dict, normalize);
            return dict; 
        }

        /// <summary> Calculate all morph influences on a pawn. </summary>
        [Obsolete("use " + nameof(MutationTracker) +  " instead")]
        public static Dictionary<MorphDef, float> GetMorphInfluences([NotNull] this Pawn p, bool normalize = false)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (p.health?.hediffSet?.hediffs == null) return new Dictionary<MorphDef, float>();
            Dictionary<MorphDef, float> dict = p.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>()
                                                .GetInfluences()
                                                .ToDictionary(tp => tp.first, tp => tp.second);

            if (normalize && dict.Count > 0)
            {
                float total = 0;
                foreach (KeyValuePair<MorphDef, float> keyValuePair in dict) total += keyValuePair.Value;

                if (total > 0)
                    foreach (MorphDef morphDef in dict.Keys)
                        dict[morphDef] /= total;
            }

            return dict;
        }

        /// <summary> Get whether or not the given pawn should still be considered 'human'. </summary>
        public static bool ShouldBeConsideredHuman([NotNull]this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (pawn.health?.hediffSet?.hediffs == null) return true;

            var tracker = pawn.GetMutationTracker();
            if (tracker == null) return true;
            float totalInfluence = 0;

            foreach (Hediff_AddedMutation hediffAddedMutation in pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>())
            {
                totalInfluence += hediffAddedMutation.Influence?.Props?.influence ?? 1;
            }

            var humanInfluence = GetHumanInfluence(pawn);

            return humanInfluence * HUMAN_CHANGE_FACTOR > totalInfluence;
        }

        /// <summary> Gets the amount of influence a pawn has that's still human. </summary>
        /// <param name="normalize"> Whether or not the resulting influence should be normalized between [0,1] </param>
        /// <returns></returns>
        public static float GetHumanInfluence( [NotNull] this Pawn pawn, bool normalize=false)
        {
            HashSet<BodyPartRecord> mutatedRecords = new HashSet<BodyPartRecord>();

            foreach (Hediff_AddedMutation hediffAddedMutation in pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>())
            {
                mutatedRecords.Add(hediffAddedMutation.Part);
            }

            var humanInfluence = (float) pawn.health.hediffSet.GetNotMissingParts().Count(p => PossiblePartsLst.Contains(p.def) && !mutatedRecords.Contains(p));

            if (normalize)
                humanInfluence /= MaxHumanInfluence;

            return humanInfluence;
        }


        /// <summary> Check if this pawn is one of the hybrid races. </summary>
        public static bool IsHybridRace([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            foreach (MorphDef morphDef in DefDatabase<MorphDef>.AllDefs)
            {
                if (pawn.def == morphDef.hybridRaceDef) return true; 
            }

            return false; 
        }
    }
}
