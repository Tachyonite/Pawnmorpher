// MutationUtilities.cs modified by Iron Wolf for Pawnmorph on 08/26/2019 2:19 PM
// last updated 08/26/2019  2:19 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// static class containing mutation related utility functions 
    /// </summary>
    public static class MutationUtilities
    {
        private static List<HediffGiver_Mutation> _allGivers;

        private static List<BodyPartDef> _allMutablePartDefs;

        /// <summary>
        /// Determines whether this instance can apply mutations to the specified pawn.
        /// </summary>
        /// <param name="mutationGiver">The mutation giver.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can apply mutations to the specified pawn; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanApplyMutations([NotNull] this HediffGiver_Mutation mutationGiver, [NotNull] Pawn pawn)
        {
            if (mutationGiver.partsToAffect == null) return false;

            var allRecordsToCheck = pawn.health.hediffSet.GetNotMissingParts()
                                        .Where(p => mutationGiver.partsToAffect.Contains(p.def));


            var mutatedParts = pawn.health.hediffSet.hediffs.Where(h => h.def == mutationGiver.hediff)
                                   .Select(h => h.Part)
                                   .Distinct()
                                   .ToList();

            return allRecordsToCheck.Any(p => !mutatedParts.Contains(p)); //if there are any non missing parts missing mutations then the hediff_giver can be applied 


        }
        

        static List<BodyPartDef> AllMutablePartDefs //use lazy initialization 
        {
            get
            {
                if (_allMutablePartDefs == null)
                {
                    HashSet<BodyPartDef> tmpSet = new HashSet<BodyPartDef>(); //use a hash set so we don't have to worry about duplicates
                    var allPartsInGivers = AllGivers.SelectMany(g => g.partsToAffect ?? Enumerable.Empty<BodyPartDef>());
                    var allPartsInMutationExtensions = AllMutations.Select(d => d.GetModExtension<MutationHediffExtension>())//grab all the mod extensions off the mutations 
                                                                   .Where(e => e != null) //keep only non nulls 
                                                                   .SelectMany(e => e.parts ?? Enumerable.Empty<BodyPartDef>());//get all the body parts 
                    foreach (BodyPartDef partDef in allPartsInGivers)
                    {
                        tmpSet.Add(partDef); 
                    }

                    foreach (BodyPartDef partDef in allPartsInMutationExtensions)
                    {
                        tmpSet.Add(partDef); 
                    }

                    _allMutablePartDefs = tmpSet.ToList(); //convert to a list, lists are easier to enumerate over 
                }

                return _allMutablePartDefs; 
            }
        }


        /// <summary>
        /// Gets the influence of the given morph this mutationDef provides.
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <param name="morph">The morph.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// mutationDef
        /// or
        /// morph
        /// </exception>
        public static float GetInfluenceOf([NotNull] this HediffDef mutationDef,[NotNull] MorphDef morph)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            if (morph == null) throw new ArgumentNullException(nameof(morph));

            if (!typeof(Hediff_AddedMutation).IsAssignableFrom(mutationDef.hediffClass)) return 0; //if not a mutation just return 0 

            if (mutationDef.HasComp(typeof(SpreadingMutationComp))) return 0; //spreading mutations shouldn't give influence, too messy to deal with 

            var comp = mutationDef.CompProps<CompProperties_MorphInfluence>();
            if (comp != null && comp.morph == morph)
            {
                return comp.influence; //if it has a morph influence comp return the influence value
            }

            if (morph.AllAssociatedAndAdjacentMutations.Select(g => g.hediff).Contains(mutationDef))
                return 0.0f; //might want to let these guys give influence 
            return 0; 

        }

        /// <summary>
        /// Gets all mutable parts on this body def 
        /// </summary>
        /// <param name="bodyDef">The body definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">bodyDef</exception>
        public static IEnumerable<BodyPartRecord> GetAllMutableParts([NotNull] this BodyDef bodyDef)
        {
            if (bodyDef == null) throw new ArgumentNullException(nameof(bodyDef));

            if (_allMutablePartsLookup.TryGetValue(bodyDef, out List<BodyPartRecord> recordList)) //see if we already calculated the list previously 
                return recordList;
            recordList = new List<BodyPartRecord>();

            foreach (BodyPartRecord bodyPartRecord in bodyDef.AllParts)
            {
                if (AllMutablePartDefs.Contains(bodyPartRecord.def))
                    recordList.Add(bodyPartRecord); 
            }

            _allMutablePartsLookup[bodyDef] = recordList; //cache the result so we only have to do this once 
            return recordList; 
        }

        private const float EPSILON = 0.01f;

        /// <summary>
        /// Gets all non zero morph influences the given hediff def gives to a pawn 
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">def</exception>
        public static IEnumerable<VTuple<MorphDef, float>> GetAllNonZeroInfluences([NotNull] HediffDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (_influenceLookupTable.TryGetValue(def, out var lst)) //check if we calculated the value already 
            {
                return lst; 
            }
            else
            {
                lst = new List<VTuple<MorphDef, float>>(); 
                _influenceLookupTable[def] = lst; //make sure the list is saved so we don't have to calculate this more then once 
            }

            foreach (var morphDef in DefDatabase<MorphDef>.AllDefs)
            {
                float influence = GetInfluenceOf(def, morphDef);
                if (influence > EPSILON)
                    lst.Add(new VTuple<MorphDef, float>(morphDef, influence)); 
            }

            return lst; 
        }

        private static Dictionary<HediffDef, List<VTuple<MorphDef, float>>> _influenceLookupTable =
            new Dictionary<HediffDef, List<VTuple<MorphDef, float>>>(); 


        private static Dictionary<BodyDef, List<BodyPartRecord>>
            _allMutablePartsLookup = new Dictionary<BodyDef, List<BodyPartRecord>>();  



        /// <summary>
        /// get the pawn's outlook toward being mutated 
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static MutationOutlook GetOutlook([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            var traits = pawn.story?.traits;
            if (traits == null) return MutationOutlook.Neutral;
            if (traits.HasTrait(PMTraitDefOf.MutationAffinity)) return MutationOutlook.Furry;
            if (traits.HasTrait(TraitDefOf.BodyPurist)) return MutationOutlook.BodyPurist;
            return MutationOutlook.Neutral;

        }

        /// <summary>
        /// returns an enumerable collection of all hediffGiver_Mutations active
        /// note, this does <i>not</i> check for givers that give the same hediff 
        /// </summary>
        public static IEnumerable<HediffGiver_Mutation> AllGivers
        {
            get
            {
                if (_allGivers == null)
                {
                    _allGivers = AllMorphHediffs.SelectMany(def => def.GetAllHediffGivers().OfType<HediffGiver_Mutation>())
                                                .ToList();
                }

                return _allGivers; 
            }
        }


        private static Dictionary<HediffDef, List<BodyPartDef>> _partLookupDict = new Dictionary<HediffDef, List<BodyPartDef>>();


        /// <summary>
        ///     get the body parts this hediff can be assigned to
        /// </summary>
        /// <param name="def"></param>
        /// <returns>
        ///     an enumerable collection of all parts this hediff can be assigned to, Note the elements can contain duplicates
        ///     and null
        /// </returns>
        [NotNull]
        public static IEnumerable<BodyPartDef> GetPossibleParts([NotNull] this HediffDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            List<BodyPartDef> lst;
            if (_partLookupDict.TryGetValue(def, out lst)) return lst;

            lst = new List<BodyPartDef>();

            IEnumerable<HediffGiver> givers =
                DefDatabase<HediffDef>.AllDefs.SelectMany(d => d.GetAllHediffGivers().Where(g => g.hediff == def));

            foreach (HediffGiver hediffGiver in givers)
                if (hediffGiver.partsToAffect == null)
                    lst.Add(null);
                else
                    lst.AddRange(hediffGiver.partsToAffect);

            var ext = def.GetModExtension<MutationHediffExtension>();
            if (ext != null)
            {
                if (ext.parts == null)
                    lst.Add(null);
                else
                    lst.AddRange(ext.parts);
            }

            _partLookupDict[def] = lst; //cache the value so subsequent lookups are fast 
            return lst;
        }


        /// <summary>
        /// an enumerable collection of all mutations 
        /// </summary>
        public static IEnumerable<HediffDef> AllMutations =>
            DefDatabase<HediffDef>.AllDefs.Where(d => typeof(Hediff_AddedMutation).IsAssignableFrom(d.hediffClass));

        private static List<ThoughtDef> _allThoughts;

        /// <summary>
        /// an enumerable collection of all morph hediffs 
        /// </summary>
        public static IEnumerable<HediffDef> AllMorphHediffs =>
            DefDatabase<HediffDef>.AllDefs.Where(d => typeof(Hediff_Morph).IsAssignableFrom(d.hediffClass));

        /// <summary>
        /// an enumerable collection of all mutation related thoughts 
        /// </summary>
        public static IEnumerable<ThoughtDef> AllMutationMemories
        {
            get
            {
                if (_allThoughts == null)
                {
                    _allThoughts = DefDatabase<HediffDef>.AllDefs.SelectMany(d => d.GetAllHediffGivers()
                                                                                   .OfType<HediffGiver_Mutation>())
                                                         .Select(g => g.memory)
                                                         .Where(t => t != null)
                                                         .Distinct()
                                                         .ToList();
                }

                return _allThoughts; 
            }
        }

        static IEnumerable<HediffDef> GetAllMutationsWithGraphics()
        {
            
            List<AlienPartGenerator.BodyAddon> bodyAddons = ((ThingDef_AlienRace) ThingDefOf.Human).alienRace.generalSettings.alienPartGenerator.bodyAddons;
            var hediffDefNames =
                bodyAddons.SelectMany(add => add.hediffGraphics ?? Enumerable.Empty<AlienPartGenerator.BodyAddonHediffGraphic>())
                          .Select(h => h.hediff);




            foreach (string hediffDef in hediffDefNames)
            {
                yield return HediffDef.Named(hediffDef); 
            }



        }

        /// <summary>Determines whether this instance is obsolete.</summary>
        /// <param name="def">The definition.</param>
        /// <returns>
        ///   <c>true</c> if the specified definition is obsolete; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">def</exception>
        public static bool IsObsolete([NotNull] this HediffDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            return def.GetType().HasAttribute<ObsoleteAttribute>() || def.hediffClass.HasAttribute<ObsoleteAttribute>();
        }


        private static List<HediffDef> _allMutationsWithGraphics;

        /// <summary>
        /// try to get the mutation tracker on this pawn, null if the pawn does not have a tracker 
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="warnOnFail">if the pawn does not have a mutation tracker, display a warning message</param>
        /// <returns></returns>
        [CanBeNull]
        public static MutationTracker GetMutationTracker([NotNull]this Pawn pawn, bool warnOnFail=true)
        {
            var comp = pawn.GetComp<MutationTracker>();
            if(comp == null && warnOnFail) Log.Warning($"pawn {pawn.Name} does not have a mutation tracker comp");
            return comp; 
        }

        /// <summary>
        /// get the largest influence on this pawn
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        [CanBeNull]
        public static MorphDef GetHighestInfluence([NotNull] this Pawn pawn)
        {
            var comp = pawn.GetMutationTracker();
            if (comp == null) return null;


            MorphDef highest = null;
            float max = float.NegativeInfinity; 
            foreach (KeyValuePair<MorphDef, float> keyValuePair in comp)
            {
                if (max < keyValuePair.Value)
                {
                    max = keyValuePair.Value;
                    highest = keyValuePair.Key; 
                }
            }

            return highest; 
        }

        /// <summary>
        /// get the production hediffs of the pawn
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static IEnumerable<Hediff> GetProductionMutations([NotNull] this Pawn pawn)
        {

            var comp = pawn.GetMutationTracker();
            if (comp == null){ yield break;}
            foreach (var mutation in comp.AllMutations)
            {

                if (mutation.TryGetComp<HediffComp_Production>() != null)
                {
                    yield return mutation;
                }
           

            }

        }

        
        /// <summary>Gets all mutations with graphics.</summary>
        /// <value>All mutations with graphics.</value>
        public static IEnumerable<HediffDef> AllMutationsWithGraphics
        {
            get
            {
                if (_allMutationsWithGraphics == null)
                {
                    _allMutationsWithGraphics = GetAllMutationsWithGraphics().ToList();
                }

                return _allMutationsWithGraphics; 
            }
        }

    }
}