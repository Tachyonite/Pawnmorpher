// MutationUtilities.cs modified by Iron Wolf for Pawnmorph on 08/26/2019 2:19 PM
// last updated 08/26/2019  2:19 PM

using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
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
            return pawn.GetComp<MutationTracker>(); 
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