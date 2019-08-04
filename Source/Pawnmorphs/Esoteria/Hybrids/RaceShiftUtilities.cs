// RaceShiftUtilities.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 7:34 PM
// last updated 08/02/2019  7:34 PM

using System;
using AlienRace;
using JetBrains.Annotations;
using RimWorld;
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
            Faction faction = pawn.Faction;
            Map map = pawn.Map;


            RegionListersUpdater.DeregisterInRegions(pawn, map);
            pawn.def = race;
            RegionListersUpdater.RegisterInRegions(pawn, map);
            map.mapPawns.UpdateRegistryForPawn(pawn);
            //no idea what HarmonyPatches.Patch.ChangeBodyType is for, not listed in pasterbin 
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();

            //save location 
            pawn.ExposeData();
            if (pawn.Faction != faction) pawn.SetFaction(faction);
        }


        public static void ChangePawnToMorph([NotNull] Pawn pawn, [NotNull] MorphDef morph)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (morph == null) throw new ArgumentNullException(nameof(morph));
            if (morph.hybridRaceDef == null)
                Log.Error($"tried to change pawn {pawn.Name.ToStringFull} to morph {morph.defName} but morph has no hybridRace!");

            ThingDef_AlienRace hRace = morph.hybridRaceDef;
            MorphDef.TransformSettings tfSettings = morph.transformSettings;

            ChangePawnRace(pawn, hRace);

            string labelId = string.IsNullOrEmpty(tfSettings.transformLetterLabelId)
                ? RACE_CHANGE_LETTER_LABEL
                : tfSettings.transformLetterLabelId;
            string contentID = string.IsNullOrEmpty(tfSettings.transformLetterContentId)
                ? RACE_CHANGE_LETTER_CONTENT
                : tfSettings.transformLetterContentId; //assign the correct default values if none are present 

            string label = labelId.Translate(pawn.LabelShort).CapitalizeFirst();
            string content = contentID.Translate(pawn.LabelShort).CapitalizeFirst();
            LetterDef letterDef = tfSettings.letterDef ?? LetterDefOf.PositiveEvent;
            Find.LetterStack.ReceiveLetter(label, content, letterDef, pawn);

            if (tfSettings.transformTale != null) TaleRecorder.RecordTale(tfSettings.transformTale, pawn);
        }
    }
}