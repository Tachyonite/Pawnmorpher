// MutationUtilities.cs modified by Iron Wolf for Pawnmorph on 08/26/2019 2:19 PM
// last updated 08/26/2019  2:19 PM

using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
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