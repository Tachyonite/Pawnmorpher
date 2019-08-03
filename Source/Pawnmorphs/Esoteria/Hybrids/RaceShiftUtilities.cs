// RaceShiftUtilities.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 7:34 PM
// last updated 08/02/2019  7:34 PM

using System;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hybrids
{
    public static class RaceShiftUtilities
    {
        public const string RACE_CHANGE_LETTER_LABEL = "LetterRaceChangeToMorphLabel";
        public const string RACE_CHANGE_LETTER_CONTENT = "LetterRaceChangeToMorphContent"; 

        public static void ChangePawnRace([NotNull] Pawn pawn, ThingDef race)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            //var pos = pawn.Position;
            var faction = pawn.Faction;
            var map = pawn.Map;


            RegionListersUpdater.DeregisterInRegions(pawn, map);
            pawn.def = race;
            RegionListersUpdater.RegisterInRegions(pawn, map);
            map.mapPawns.UpdateRegistryForPawn(pawn); 
            //no idea what HarmonyPatches.Patch.ChangeBodyType is for, not listed in pasterbin 
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();

            //save location 
            pawn.ExposeData();
            if (pawn.Faction != faction)
            {
                pawn.SetFaction(faction); 
            }


        }
    }
}