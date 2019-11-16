using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
using Verse;
using RimWorld;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;

namespace Pawnmorph
{
    /// <summary>
    /// static class for initializing the mod 
    /// </summary>
    [StaticConstructorOnStartup]
    public static class PawnmorpherModInit
    {
        static PawnmorpherModInit() // The one true constructor.
        {
            NotifySettingsChanged();
            GenerateImplicitRaces();
            CheckForObsoletedComponents();
        }

        private static void CheckForObsoletedComponents()
        {
            IEnumerable<HediffDef> obsoleteHediffTypes = DefDatabase<HediffDef>
                                                        .AllDefs.Where(h => h.hediffClass.HasAttribute<ObsoleteAttribute>());
            //get all obsoleted hediffs in use 

            foreach (HediffDef obsoleteDef in obsoleteHediffTypes)
                Log.Warning($"obsolete hediff {obsoleteDef.hediffClass.Name} in {obsoleteDef.defName}");
            var tmp = new List<string>();
            foreach (HediffDef hediffDef in DefDatabase<HediffDef>.AllDefs)
            {
                IEnumerable<HediffGiver> obsoleteGivers =
                    hediffDef.GetAllHediffGivers().Where(g => g.GetType().HasAttribute<ObsoleteAttribute>());
                var builder = new StringBuilder();

                builder.AppendLine($"in {hediffDef.defName}");
                foreach (HediffGiver obsoleteGiver in obsoleteGivers)
                    builder.AppendLine($"obsolete hediff giver: {obsoleteGiver.GetType().Name}".Indented());
                IEnumerable<HediffGiver> giversGivingBadHediffs = hediffDef
                                                                 .GetAllHediffGivers() //find hediff giver that are giving obsolete hediffs 
                                                                 .Where(g => g.hediff?.GetType().HasAttribute<ObsoleteAttribute>()
                                                                          ?? false);

                foreach (HediffGiver giversGivingBadHediff in giversGivingBadHediffs)
                    tmp.Add($"giver {giversGivingBadHediff.GetType().Name} is giving obsolete hediff {giversGivingBadHediff.hediff.defName}");


                if (tmp.Count > 0)
                {
                    builder.Append(string.Join("\n", tmp.ToArray()).Indented());
                    tmp.Clear();
                    Log.Warning(builder.ToString());
                }
            }
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

        /// <summary>called when the settings are changed</summary>
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
