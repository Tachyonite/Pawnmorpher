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

        private static List<HediffDef> _allMutationsWithGraphics;

        /// <summary>
        /// try to get the mutation tracker on this pawn, null if the pawn does not have a tracker 
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        [CanBeNull]
        public static MutationTracker GetMutationTracker([NotNull]this Pawn pawn)
        {
            var comp = pawn.GetComp<MutationTracker>();
            if(comp == null) Log.Warning($"pawn {pawn.Name} does not have a mutation tracker comp");
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

        /// <summary>
        /// get the normalized influences on the pawn
        ///     the values are normalized to the total influences of the morph
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static IEnumerable<VTuple<MorphDef, float>> GetNormalizedInfluences([NotNull] this Pawn pawn)
        {
            var comp = pawn.GetMutationTracker();
            if(comp == null) yield break;
            foreach (var kvp in comp)
            {
                var total = kvp.Value / kvp.Key.TotalInfluence;
                yield return new VTuple<MorphDef, float>(kvp.Key, total); 

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