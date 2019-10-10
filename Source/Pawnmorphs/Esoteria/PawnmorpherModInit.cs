using System.Collections.Generic;
using System.Linq;
using AlienRace;
using Verse;
using RimWorld;
using Pawnmorph.Hybrids;

namespace Pawnmorph
{
    [StaticConstructorOnStartup]
    public static class PawnmorpherModInit
    {
        static PawnmorpherModInit() // The one true constructor.
        {
            NotifySettingsChanged();
            GenerateImplicitRaces();
        }

        private static void GenerateImplicitRaces()
        {
            var allLoadedThingDefs = DefDatabase<ThingDef>.AllDefs;
            HashSet<ushort> takenHashes = new HashSet<ushort>(allLoadedThingDefs.Select(t => t.shortHash));  // Get the hashes already being used 

            List<ThingDef> genRaces = new List<ThingDef>();

            foreach (ThingDef_AlienRace thingDefAlienRace in RaceGenerator.ImplicitRaces)
            {
                var race = (ThingDef)thingDefAlienRace;
                genRaces.Add(race);
                DefGenerator.AddImpliedDef(race);
            }

            foreach (ThingDef thingDef in genRaces)
            {
                GiveHash(thingDef, takenHashes);
            }
        }

        static void GiveHash(ThingDef defToGiveHashTo, HashSet<ushort> takenHashes)
        {
            var num = (ushort)(GenText.StableStringHash(defToGiveHashTo.defName) % 65535);

            var num2 = 0;

            while (num == 0 || takenHashes.Contains(num))
            {
                num += 1;
                num2++;
                if (num2 > 5000) //cut off at 5000 tries 
                {
                    Log.Message("Short hashes are saturated. There are probably too many Defs.", false);
                }
            }

            defToGiveHashTo.shortHash = num;
            takenHashes.Add(num);
        }

        public static void NotifySettingsChanged()
        {
            PawnmorpherSettings settings = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>();
            IncidentDef mutagenIncident = IncidentDef.Named("MutagenicShipPartCrash");
            IncidentDef cowfluIncident = IncidentDef.Named("Disease_Cowflu");
            IncidentDef foxfluIncident = IncidentDef.Named("Disease_Foxflu");
            IncidentDef chookfluIncident = IncidentDef.Named("Disease_Chookflu");

            if (!settings.enableMutagenShipPart)
                mutagenIncident.baseChance = 0.0f;
            else
                mutagenIncident.baseChance = 2.0f;

            if (!settings.enableFallout)
                PMIncidentDefOf.MutagenicFallout.baseChance = 0;

            if (!settings.enableMutagenDiseases)
            {
                cowfluIncident.baseChance = 0.0f;
                foxfluIncident.baseChance = 0.0f;
                chookfluIncident.baseChance = 0.0f;
            }
            else
            {
                cowfluIncident.baseChance = 0.5f;
                foxfluIncident.baseChance = 0.5f;
                chookfluIncident.baseChance = 0.5f;
            }
        }
    }
}
