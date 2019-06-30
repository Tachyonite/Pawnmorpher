﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using RimWorld;

namespace Pawnmorph
{
    public class PawnmorpherSettings : ModSettings
    {
        /// <summary>
        /// The three settings our mod has.
        /// </summary>
        public bool enableMutagenShipPart = true;
        public bool enableMutagenDiseases = true;
        public bool enableMutagenMeteor = true;
        public bool enableWildFormers = true;
        public float transformChance = 50f;
        public float formerChance = 2f;
        public float partialChance = 5f;

        /// <summary>
        /// The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableMutagenShipPart, "enableMutagenShipPart", true);
            Scribe_Values.Look(ref enableMutagenDiseases, "enableMutagenDiseases", true);
            Scribe_Values.Look(ref enableMutagenDiseases, "enableMutagenMeteor", true);
            Scribe_Values.Look(ref enableWildFormers, "enableWildFormers", true);
            Scribe_Values.Look(ref transformChance, "transformChance");
            Scribe_Values.Look(ref formerChance, "formerChance");
            Scribe_Values.Look(ref partialChance, "partialChance");
            base.ExposeData();
        }
    }

    public class PawnmorpherMod : Mod
    {

        public override void WriteSettings()
        {
            base.WriteSettings();
            PawnmorpherModInit.NotifySettingsChanged();
        }

        PawnmorpherSettings settings;

        public PawnmorpherMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<PawnmorpherSettings>();
        }

        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Enable Mutagenic Ship Parts", ref settings.enableMutagenShipPart, "Ship parts can crash that mutate pawns in an expanding radius");
            listingStandard.CheckboxLabeled("Enable Mutagenic Diseases", ref settings.enableMutagenDiseases, "Whether pawns can become ill with mutagenic diseases that will morph them");
            listingStandard.CheckboxLabeled("Enable Mutagenic Meteorite Morph Radius", ref settings.enableMutagenMeteor, "A mutonite meteor can still spawn, this determines if people getting too close will start to transform.");
            listingStandard.CheckboxLabeled("Enable Wild Former Humans", ref settings.enableWildFormers, "Whether animals can spawn as former humans, allowing them to be reverted");
            listingStandard.GapLine();
            listingStandard.Label(string.Format("Chance for mutagens to fully transform pawns: {0}%", settings.transformChance.ToString("F1")));
            settings.transformChance = listingStandard.Slider(settings.transformChance, 0f,100f);
            listingStandard.Label(string.Format("Chance for animals to spawn as former humans: {0}%", settings.formerChance.ToString("F1")));
            settings.formerChance = listingStandard.Slider(settings.formerChance, 0f, 100f);
            listingStandard.Label(string.Format("Chance for morph eggs/milk to trigger a complete transformation instead of a single mutation: {0}%", settings.partialChance.ToString("F1")));
            settings.partialChance = listingStandard.Slider(settings.partialChance, 0f, 100f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory()
        {
            return "PawnmorpherModName".Translate();
        }

        
    }

    [StaticConstructorOnStartup]
    public static class PawnmorpherModInit
    {
        static PawnmorpherModInit() //our constructor
        {
            NotifySettingsChanged();
        }

        public static void NotifySettingsChanged()
        {

            PawnmorpherSettings settings = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>();
            IncidentDef mutagenIncident = IncidentDef.Named("MutagenicShipPartCrash");
            IncidentDef cowfluIncident = IncidentDef.Named("Disease_Cowflu");
            IncidentDef foxfluIncident = IncidentDef.Named("Disease_Foxflu");
            IncidentDef chookfluIncident = IncidentDef.Named("Disease_Chookflu");
            if (!settings.enableMutagenShipPart)
            {
                
                mutagenIncident.baseChance = 0.0f;
            }
            else { mutagenIncident.baseChance = 2.0f; }
            if (!settings.enableMutagenDiseases)
            {
                cowfluIncident.baseChance = 0.0f;
                foxfluIncident.baseChance = 0.0f;
                chookfluIncident.baseChance = 0.0f;
            }
            else {
                cowfluIncident.baseChance = 0.5f;
                foxfluIncident.baseChance = 0.5f;
                chookfluIncident.baseChance = 0.5f;
            }

            Log.Message(mutagenIncident.baseChance.ToString());
            Log.Message(cowfluIncident.baseChance.ToString());
        }
    }
}
