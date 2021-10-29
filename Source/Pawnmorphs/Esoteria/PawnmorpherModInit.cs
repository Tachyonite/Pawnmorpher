using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.DebugUtils;
using Pawnmorph.GraphicSys;
using Pawnmorph.Hediffs;
using Verse;
using RimWorld;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using UnityEngine;

//just a typedef to shorten long type name 
using HediffGraphic = AlienRace.AlienPartGenerator.BodyAddonHediffGraphic; 

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
            try
            {
                GiveHashMethod = typeof(ShortHashGiver).GetMethod("GiveShortHash", BindingFlags.NonPublic | BindingFlags.Static);


                InjectGraphics(); 
                NotifySettingsChanged();
                GenerateImplicitRaces();
                TransferPatchesToExplicitRaces();
                CheckForObsoletedComponents();
                CheckForModConflicts();
                try
                {
                    
                    PMImplicitDefGenerator.GenerateImplicitDefs();

                }
                catch (Exception e)
                {
                    
                    throw new ModInitializationException($"while generating genomes caught exception {e.GetType().Name}",e);
                }

            }
           
            catch (Exception e)
            {
                throw new ModInitializationException($"while initializing Pawnmorpher caught exception {e.GetType().Name}",e);
            }
        }

        private static void InjectGraphics()
        {
            try
            {
                var human = (ThingDef_AlienRace) ThingDefOf.Human;

                var bodyAddons = human
                                .alienRace.generalSettings.alienPartGenerator
                                .bodyAddons;

                //make a dict while checking for duplicates 
                Dictionary<string, TaggedBodyAddon> dict = new Dictionary<string, TaggedBodyAddon>();
                foreach (TaggedBodyAddon tAddon in bodyAddons.OfType<TaggedBodyAddon>())
                {
                    if (tAddon.anchorID == null)
                    {
                        Log.Error($"encountered tagged body addon with null anchorID!");
                    }
                    else if (dict.ContainsKey(tAddon.anchorID))
                    {
                        Log.Error($"encountered duplicate tagged body addon with anchor id \"{dict}\"!");
                    }
                    else dict[tAddon.anchorID] = tAddon;
                }

                List<MutationStage> mutationStages = new List<MutationStage>();
                List<string> anchors = new List<string>();
                //now go throw all mutations and any with graphics 
                foreach (MutationDef mutation in MutationDef.AllMutations)
                {
                    var mStages = mutation.stages.MakeSafe().OfType<MutationStage>(); //all mutation stages in this mutation 
                    var lq = mutation.graphics.MakeSafe()
                            .Select(g => g.anchorID)
                            .Concat(mStages.SelectMany(s => s.graphics.MakeSafe().Select(g => g.anchorID))); //all anchor ids in those stages 
                    anchors.Clear();
                    anchors.AddRange(lq.Distinct()); //make sure the list is distinct 

                    foreach (var anchor in anchors)
                    {
                        
                        mutationStages.Clear();
                        mutationStages.AddRange(mutation.stages.MakeSafe() //grab all mutations stages with graphics that pertain to this a
                                                        .OfType<MutationStage>()
                                                        .Where(m => m.graphics.MakeSafe()
                                                                     .Any(s => s.anchorID == anchor)));

                        if (!dict.TryGetValue(anchor, out TaggedBodyAddon addon))
                        {
                            Log.Error($"unable to find body addon on human with anchor id \"{anchor}\"!");
                        }
                        else
                        {
                            HediffGraphic hediffGraphic = GenerateGraphicsFor(mutationStages, mutation, anchor);
                            if (hediffGraphic == null) continue;
                            if (addon.hediffGraphics == null) addon.hediffGraphics = new List<HediffGraphic>();

                            addon.hediffGraphics.Add(hediffGraphic);
                            AppendPools(hediffGraphic, addon); 
                        }
                    }
                }



            }
            catch (Exception e)
            {
                Log.Error($"unable to inject mutation graphics! \n{e}");
            }
        }

        private static void AppendPools(HediffGraphic hediffGraphic, TaggedBodyAddon bodyAddon)
        {
            while (
                ContentFinder<Texture2D>.Get(hediffGraphic.path + (hediffGraphic.variantCount == 0 ? "" : hediffGraphic.variantCount.ToString()) + "_north",
                                             false)
             != null)
                hediffGraphic.variantCount++;
            //Log.Message($"Variants found for {hediffGraphic.path}: {hediffGraphic.variantCount}");
            if (hediffGraphic.variantCount == 0)
                Log.Warning($"No hediff graphics found at {hediffGraphic.path} for hediff {hediffGraphic.hediff} in");

            if (hediffGraphic.severity != null)
                foreach (AlienPartGenerator.BodyAddonHediffSeverityGraphic bahsg in hediffGraphic.severity)
                {
                    while (
                        ContentFinder<Texture2D>
                           .Get(bahsg.path + (bahsg.variantCount == 0 ? "" : bahsg.variantCount.ToString()) + "_north", false)
                     != null)
                        bahsg.variantCount++;
                    //Log.Message($"Variants found for {bahsg.path} at severity {bahsg.severity}: {bahsg.variantCount}");
                    if (bahsg.variantCount == 0)
                        Log.Warning($"No hediff graphics found at {bahsg.path} at severity {bahsg.severity} for hediff {hediffGraphic.hediff} in ");
                }
        }

        private static HediffGraphic GenerateGraphicsFor([NotNull] List<MutationStage> mutationStages, [NotNull] MutationDef mutation, string anchorID)
        {
            List<MutationGraphicsData> mainData = mutation.graphics.MakeSafe().Where(g => g.anchorID == anchorID).ToList();
            List<(string path, float minSeverity)> stageData = mutationStages.Where(s => !s.graphics.NullOrEmpty())
                                                                             .SelectMany(s => s.graphics.Where(g => g.anchorID
                                                                                                                 == anchorID)
                                                                                               .Select(g => (g.path,
                                                                                                             s.minSeverity)))
                                                                             .ToList();

            string mainPath = mainData.FirstOrDefault()?.path ?? stageData.FirstOrDefault().path; //get the main path 
            //either the path in the main data or the fist severity graphic 
            if (mainPath == null)
            {
                Log.Error($"found invalid graphic data in {mutation.defName}! unable to find data for anchor \"{anchorID}\"");
                return null;
            }


            var hGraphic = new HediffGraphic
            {
                hediff = mutation,
                path = mainPath
            };

            var severityLst =
                new List<AlienPartGenerator.BodyAddonHediffSeverityGraphic>();
            for (var index = mutationStages.Count - 1; index >= 0; index--)
            {
                MutationStage stage = mutationStages[index];
                MutationGraphicsData
                    graphic = stage.graphics.MakeSafe()
                                   .FirstOrDefault(s => s.anchorID
                                                     == anchorID); //fill the severity graphics if they are present in descending order 
                if (graphic != null)
                    severityLst.Add(new AlienPartGenerator.BodyAddonHediffSeverityGraphic
                    {
                        path = graphic.path,
                        severity = stage.minSeverity
                    });
            }

            hGraphic.severity = severityLst;
            return hGraphic; 
        }

        private static void SetupInjectors()
        {
            throw new NotImplementedException();
        }

        private static void CheckForModConflicts()
        {
            var androidsIsLoaded = LoadedModManager.RunningMods.Any(m => m.PackageId == "Atlas.AndroidTiers");

            //TODO make this a pop up like HugsLib's checker
            if (androidsIsLoaded)
            {
                Log.Error("Android Tiers + Pawnmorpher detected. Please disable the AT 'hide inactive surrogates' mod option if former humans don't appear in your colonist bar or menu.");
            }
        }


        private static void TransferPatchesToExplicitRaces()
        {
            List<AlienPartGenerator.BodyAddon> allAddons = GetAllAddonsToAdd().ToList();

            var explicitRaces =
                MorphDef.AllDefs.Where(m => m.ExplicitHybridRace != null && m.raceSettings?.transferHumanBodyAddons == true)
                        .Select(m => m.ExplicitHybridRace);

            foreach (var explicitRace in explicitRaces)
            {
                var aRace = explicitRace as ThingDef_AlienRace;
                if (aRace == null)
                {
                    Log.Warning($"could not transfer mutation graphics to {explicitRace.defName} because it is not a {nameof(ThingDef_AlienRace)}");
                    continue;
                }

                try
                {
                    AddAddonsToRace(aRace, allAddons);
                }
                catch (Exception e)
                {
                    Log.Error($"caught {e.GetType().Name} while trying to add mutation body graphics to {aRace.defName}!\n{e}");
                }

            }



        }

        [NotNull]
        private static IEnumerable<AlienPartGenerator.BodyAddon> GetAllAddonsToAdd()
        {
            var addons =
                ((ThingDef_AlienRace) ThingDefOf.Human).alienRace.generalSettings.alienPartGenerator.bodyAddons.MakeSafe();

            foreach (AlienPartGenerator.BodyAddon bodyAddon in addons)
            {
                if(bodyAddon.hediffGraphics == null || bodyAddon.hediffGraphics.Count == 0) continue;
                bool found = false; 
                foreach (var hDef in bodyAddon.hediffGraphics.Select(h=> h.hediff))
                {
                    if(hDef == null) continue;
                    if (hDef is MutationDef) //make sure we only grab addons that are mutations 
                    {

                        found = true;
                        break;
                    }
                }

                if (found) yield return bodyAddon; 
            }

        }

        private static void AddAddonsToRace([NotNull] ThingDef_AlienRace race, [NotNull] List<AlienPartGenerator.BodyAddon> allAddons)
        {
            if (race.alienRace == null)
            {
                race.alienRace = new ThingDef_AlienRace.AlienSettings();
            }

            if (race.alienRace.generalSettings == null)
            {
                race.alienRace.generalSettings = new GeneralSettings();
            }

            if (race.alienRace.generalSettings.alienPartGenerator == null)
            {
                race.alienRace.generalSettings.alienPartGenerator = new AlienPartGenerator();
                
            }

            var partGen = race.alienRace.generalSettings.alienPartGenerator;
            if (partGen.bodyAddons == null)
            {
                partGen.bodyAddons = new List<AlienPartGenerator.BodyAddon>(); 
            }

            foreach (AlienPartGenerator.BodyAddon bodyAddon in allAddons) 
            {
                var cpy = CloneAddon(bodyAddon);
                partGen.bodyAddons.Add(cpy); 
            }

        }

        [NotNull]
        static AlienPartGenerator.BodyAddon CloneAddon([NotNull] AlienPartGenerator.BodyAddon addon)
        {
            var pType = typeof(AlienPartGenerator.BodyAddon);
            var shaderField = pType.GetField("shaderType", BindingFlags.Instance | BindingFlags.NonPublic);
            var prioritizationField = pType.GetField("prioritization", BindingFlags.Instance | BindingFlags.NonPublic);
            var colorChannelField = pType.GetField("colorChannel", BindingFlags.Instance | BindingFlags.NonPublic);
            var copy = new AlienPartGenerator.BodyAddon()
            { 
                angle = addon.angle,
                backstoryGraphics =  addon.backstoryGraphics.MakeSafe().ToList(),
                backstoryRequirement = addon.backstoryRequirement,
                bodyPart = addon.bodyPart,
                debug = addon.debug,
                drawForFemale = addon.drawForFemale,
                drawForMale =  addon.drawForMale,
                drawnInBed = addon.drawnInBed,
                drawnOnGround = addon.drawnOnGround,
                drawSize = addon.drawSize,
                hiddenUnderApparelFor = addon.hiddenUnderApparelFor.MakeSafe().ToList(),
                path = addon.path,
                offsets = addon.offsets,
                linkVariantIndexWithPrevious = addon.linkVariantIndexWithPrevious,
                inFrontOfBody = addon.inFrontOfBody,
                layerInvert = addon.layerInvert,
                variantCount = addon.variantCount,
                hediffGraphics = addon.hediffGraphics.MakeSafe().ToList(),
                
                hiddenUnderApparelTag = addon.hiddenUnderApparelTag,
            };

            shaderField.SetValue(copy, addon.ShaderType);
            prioritizationField.SetValue(copy, addon.Prioritization.ToList());
            colorChannelField.SetValue(copy, addon.ColorChannel); 
            return copy;
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
                    hediffDef.GetAllHediffGivers().Where(g => g?.GetType().HasAttribute<ObsoleteAttribute>() == true);
                var builder = new StringBuilder();

                builder.AppendLine($"in {hediffDef.defName}");
                foreach (HediffGiver obsoleteGiver in obsoleteGivers)
                    builder.AppendLine($"obsolete hediff giver: {obsoleteGiver.GetType().Name}".Indented());
                IEnumerable<HediffGiver> giversGivingBadHediffs = hediffDef
                                                                 .GetAllHediffGivers() //find hediff giver that are giving obsolete hediffs 
                                                                 .Where(g => g?.hediff?.GetType().HasAttribute<ObsoleteAttribute>()
                                                                          ?? false);

                foreach (HediffGiver giversGivingBadHediff in giversGivingBadHediffs)
                    tmp.Add($"giver {giversGivingBadHediff.GetType().Name} is giving obsolete hediff {giversGivingBadHediff.hediff.defName}");


                if (tmp.Count > 0)
                {
                    builder.Append(string.Join("\n", tmp.ToArray()).Indented());
                    tmp.Clear();
                    DebugLogUtils.Warning(builder.ToString());
                }
            }
        }

        private static void GenerateImplicitRaces()
        {
            try
            {
                
                List<ThingDef> genRaces = new List<ThingDef>();

                foreach (ThingDef_AlienRace thingDefAlienRace in RaceGenerator.ImplicitRaces)
                {
                    var race = (ThingDef)thingDefAlienRace;
                    genRaces.Add(race);
                    DefGenerator.AddImpliedDef(race);
                    DefGenerator.AddImpliedDef(thingDefAlienRace); 
                }

                object[] tmpArr = new object[2];

                tmpArr[1] = typeof(ThingDef); 
                foreach (ThingDef thingDef in genRaces)
                {
                    tmpArr[0] = thingDef;
                    GiveHashMethod.Invoke(null, tmpArr); 
                }
            }
            catch (MissingMethodException e)
            {
                throw new
                    ModInitializationException($"caught missing method exception while generating implicit races! is HAR up to date?",
                                               e);
            }
        }


        private static readonly MethodInfo GiveHashMethod; 


        /// <summary>called when the settings are changed</summary>
        public static void NotifySettingsChanged()
        {
            PawnmorpherSettings settings = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>();
            IncidentDef mutagenIncident = IncidentDef.Named("MutagenicShipPartCrash");
            

            if (!settings.enableMutagenShipPart)
                mutagenIncident.baseChance = 0.0f;
            else
                mutagenIncident.baseChance = 2.0f;

            if (!settings.enableFallout)
                PMIncidentDefOf.MutagenicFallout.baseChance = 0;

        }
    }
}
