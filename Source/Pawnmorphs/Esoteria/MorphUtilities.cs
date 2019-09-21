// MorphUtilities.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 3:48 PM
// last updated 08/02/2019  3:48 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Hybrids;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     static collection of useful morph related functions
    /// </summary>
    /// TransformerUtilities was getting a bit crowded
    public static class MorphUtilities
    {
        private static List<BodyPartDef> PossiblePartsLst { get; }

        public static IEnumerable<BodyPartDef> PartsWithPossibleMutations => PossiblePartsLst;
        public static int NumPartsWithPossibleMutations => PossiblePartsLst.Count;

        public const float HUMAN_CHANGE_FACTOR = 0.65f;

        static MorphUtilities() //this is really hacky 
        {
            IEnumerable<HediffDef> defs = MorphTransformationDefOf.AllMorphs; 

            PossiblePartsLst = defs.SelectMany(def => def.stages ?? Enumerable.Empty<HediffStage>())
                                .SelectMany(s => s.hediffGivers ?? Enumerable.Empty<HediffGiver>())
                                .Where(giver => typeof(Hediff_AddedMutation).IsAssignableFrom(giver.hediff.hediffClass) ) //get all hediff givers giving out mutations  
                                .SelectMany(g => g.partsToAffect ?? Enumerable.Empty<BodyPartDef>())
                                .Distinct() //get rid of duplicates 
                                .ToList(); //make a list to save the result 
            //doing this to get the number of body parts that can have mutations so we can calculate how "human" a pawn is 
            //this feels wrong, but I can't think of a better way to calculate the "human-ness" of a pawn  - Iron 
            

        }

        /// <summary>
        /// get all morphs defs associated with this transformation hediff def 
        /// </summary>
        /// <param name="transformationDef">The transformation definition.</param>
        /// <returns></returns>
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

        /// <summary>
        /// get the morph tracking component on this pawn 
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
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


        private static Dictionary<HediffDef, List<MorphDef>> _morphAssociationCache = new Dictionary<HediffDef, List<MorphDef>>(); //don't calculate the associations more then we have to 

        /// <summary>
        /// Gets all morphDefs associated with the given transformation 
        /// </summary>
        /// <param name="transformationDef">The transformation definition.</param>
        /// <returns></returns>
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


        /// <summary>
        /// Gets the morphs in map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="morph">The morph.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// map
        /// or
        /// morph
        /// </exception>
        public static IEnumerable<Pawn> GetMorphsInMap([NotNull] this Map map, [NotNull] MorphDef morph)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            if (morph == null) throw new ArgumentNullException(nameof(morph));

            return map.listerThings.ThingsOfDef(morph.hybridRaceDef).OfType<Pawn>();
        }


        public struct Tuple
        {
            public Tuple(MorphDef morph, float influence)
            {
                this.morph = morph;
                this.influence = influence;
            }

            public MorphDef morph;
            public float influence;
        }


        /// <summary>
        ///     group the morph influences on this collection of hediff_added mutations
        /// </summary>
        /// <param name="mutations"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple> GetInfluences([NotNull] this IEnumerable<Hediff_AddedMutation> mutations)
        {
            if (mutations == null) throw new ArgumentNullException(nameof(mutations));
            IEnumerable<IGrouping<MorphDef, float>> linq = mutations.Where(m => m?.Influence?.Props?.morph != null)
                                                                    .GroupBy(m => m.Influence.Props.morph,
                                                                             m => m.Influence.Props.influence);


            foreach (IGrouping<MorphDef, float> grouping in linq)
            {
                MorphDef key = grouping.Key;
                float accum = 0;
                foreach (float f in grouping) accum += f;

                yield return new Tuple {influence = accum, morph = key};
            }
        }

        /// <summary>
        ///     group the morph influences on this collection of hediffDefs
        /// </summary>
        /// <param name="mutationDefs"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple> GetMorphInfluences([NotNull] this IEnumerable<HediffDef> mutationDefs)
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

                yield return new Tuple(morph, accum);
            }
        }


        /// <summary>
        /// Gets the type of the transformation.
        /// </summary>
        /// <param name="inst">The inst.</param>
        /// <returns>the type of the transformation</returns>
        /// <exception cref="ArgumentNullException">inst</exception>
        public static MorphTransformationTypes GetTransformationType([NotNull] this HediffDef inst)
        {
            if (inst == null) throw new ArgumentNullException(nameof(inst));
            if (!typeof(Hediff_Morph).IsAssignableFrom(inst.hediffClass)) return 0;

            var comp = inst.CompProps<HediffCompProperties_Single>();
            return comp == null ? MorphTransformationTypes.Full : MorphTransformationTypes.Partial;
        }

        /// <summary>
        ///     calculate all morph influences on a pawn
        ///     us this if there are many calculations, it's more efficient and easier on memory
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="fillDict">the dictionary to fill</param>
        /// <param name="normalize">if to normalize the influences</param>
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
        /// calculate all the influences on this pawn by morph category
        /// us this if there are many calculations, it's more efficient and easier on memory
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="fillDict"></param>
        /// <param name="normalize">if the resulting dict should be normalized </param>
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

            if (normalize && total > 0.001f) //prevent division by zero 
            {
                foreach (var fillDictKey in fillDict.Keys)
                {
                    fillDict[fillDictKey] /= total; 
                }
            }
        }

        /// <summary>
        /// calculate all the influences on this pawn by morph category
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="normalize">if the resulting dict should be normalized</param>
        /// <returns></returns>
        public static Dictionary<MorphCategoryDef, float> GetMorphCategoriesInfluences([NotNull] this Pawn pawn, bool normalize = false)
        {
            Dictionary<MorphCategoryDef, float> dict = new Dictionary<MorphCategoryDef, float>();
            GetMorphCategoriesInfluences(pawn, dict, normalize);
            return dict; 
        }


        


        /// <summary>
        ///     calculate all morph influences on a pawn
        /// </summary>
        /// <param name="p"></param>
        /// <param name="normalize"></param>
        /// <returns></returns>
        [Obsolete("use " + nameof(MutationTracker) +  " instead")]
        public static Dictionary<MorphDef, float> GetMorphInfluences([NotNull] this Pawn p, bool normalize = false)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (p.health?.hediffSet?.hediffs == null) return new Dictionary<MorphDef, float>();
            Dictionary<MorphDef, float> dict = p.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>()
                                                .GetInfluences()
                                                .ToDictionary(tp => tp.morph, tp => tp.influence);

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

        
        /// <summary>
        /// get if the given pawn should still be considered 'human' 
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
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

        /// <summary>
        /// gets the amount of influence a pawn has that's still human
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="normalize">if the resulting influence should be normalized between [0,1]</param>
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


        private static float? _maxHumanInfluence;

        /// <summary>
        /// the maximum possible human influence 
        /// </summary>
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


        /// <summary>
        /// check if this pawn is one of the hybrid races 
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
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