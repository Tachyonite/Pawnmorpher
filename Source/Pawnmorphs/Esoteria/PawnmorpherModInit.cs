﻿using System;
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
#if DEBUG
        private const string MOD_BUILD_TYPE = "DEBUG";
#else
        private const string MOD_BUILD_TYPE = "RELEASE";
#endif

        static PawnmorpherModInit() // The one true constructor.
        {
            
            Log.Message($"initializing {MOD_BUILD_TYPE} version of Pawnmorpher");



            try
            {
                GiveHashMethod = typeof(ShortHashGiver).GetMethod("GiveShortHash", BindingFlags.NonPublic | BindingFlags.Static);

                VerifyMorphDefDatabase();

                InjectGraphics(); 
                NotifySettingsChanged();
                GenerateImplicitRaces();
                PatchExplicitRaces();
                AddMutationsToWhitelistedRaces();
                EnableDisableOptionalPatches();
                CheckForObsoletedComponents();
                try
                {


                    CheckForModConflicts();

#if DEBUG
                    // Only show configuration errors in debug mode.
                    DisplayGroupedModIssues();
#endif



                }
                catch (Exception e) // just logging, ok to catch and swallow the exception 
                {
                    Log.Error($"unable to display grouped mod issues due to caught exception:{e.GetType().Name}\n{e}");
                }
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

        private static void EnableDisableOptionalPatches()
        {
            Dictionary<string, bool> optionalPatches = PawnmorpherMod.Settings.optionalPatches;
            if (optionalPatches == null)
                return;

            foreach (var item in optionalPatches)
            {
                Type patch = Assembly.GetExecutingAssembly().GetType(item.Key);
                if (patch != null)
                {
                    var attribute = patch.GetCustomAttribute<HPatches.Optional.OptionalPatchAttribute>(false);
                    if (attribute != null)
                    {
                        if (attribute.DefaultEnabled == item.Value)
                            continue;

                        MemberInfo member = patch.GetMember(attribute.EnableMemberName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty).FirstOrDefault();
                        if (member != null)
                        {
                            switch (member)
                            {
                                case FieldInfo field:
                                    field.SetValue(null, item.Value);
                                    break;

                                case PropertyInfo property:
                                    property.SetValue(null, item.Value);
                                    break;
                            }
                            continue;
                        }
                    }
                }
#if DEBUG
                Log.Warning("[Pawnmorpher] Failed toggling optional patch: " + item.Key);
#endif
            }

        }

        private static void VerifyMorphDefDatabase()
        {
            // Handle morph def config errors.
            List<MorphDef> morphs = DefDatabase<MorphDef>.AllDefs.Where(x => x.ConfigErrors().Any() == false).ToList();
            if (morphs.Count < DefDatabase<MorphDef>.DefCount)
            {
                // If error-free collection is smaller than total amount of defs, clear DefDatabase and only insert error-free defs.
                // DefDatabase does not have a function to remove individual entries - probably due to internal caching and indexing
                // So we have to completely clear and repopulate it.
                // This is to ensure the rest of the mod relying on the DefDatabase directly keeps functioning.
                Log.Warning($"{DefDatabase<MorphDef>.DefCount - morphs.Count} MorphDefs was removed due to config errors.");

                DefDatabase<MorphDef>.Clear();
                DefDatabase<MorphDef>.Add(morphs);
            }
        }

        private static void DisplayGroupedModIssues()
        {
            CheckForMorphInjectorDefs(); 

        }

        private static void CheckForMorphInjectorDefs()
        {
            IEnumerable<IGrouping<ModContentPack, MorphDef>> mDefs = MorphDef
                                                                    .AllDefs.Where(md => md?.modContentPack != null && md.injectorProperties == null
                                                                                      && md.injectorDef == null && !md.noInjector)
                                                                    .GroupBy(m => m.modContentPack);


            foreach (IGrouping<ModContentPack, MorphDef> morphDefs in mDefs)
            {
                ModContentPack key = morphDefs.Key;
                if (key == null) continue; 
                List<MorphDef> lst = morphDefs.ToList();
                if (lst.Count > 0)
                {
                    var builder = new StringBuilder();
                    builder.AppendLine($"in {key.Name} found {lst.Count} morph defs missing {nameof(MorphDef.injectorDef)} and {nameof(MorphDef.injectorProperties)}:");
                    foreach (MorphDef morphDef in lst) builder.AppendLine(morphDef.defName);

                    Log.Warning(builder.ToString());
                }
            }
        }

        private static void InjectGraphics()
        {
            try
            {
                // Get all body-addons from all species to initialize any TaggedBodyAddon
                IEnumerable<ThingDef_AlienRace> humanoidRaces = DefDatabase<ThingDef>.AllDefs.OfType<ThingDef_AlienRace>();

                List<TaggedBodyAddon> taggedAddons = new List<TaggedBodyAddon>();
                foreach (ThingDef_AlienRace race in humanoidRaces)
                {
                    var taggedBodyAddons = race.alienRace.generalSettings.alienPartGenerator.bodyAddons.OfType<TaggedBodyAddon>();
                    foreach (TaggedBodyAddon bodyAddon in taggedBodyAddons)
                    {
                        if (bodyAddon.anchorID == null)
                        {
                            Log.Error($"Encountered tagged body addon with null anchorID in RaceDef {race.defName}!");
                        }
                        else
                            taggedAddons.Add(bodyAddon);
                    }
                }

                ILookup<string, TaggedBodyAddon> dict = taggedAddons.ToLookup(x => x.anchorID);

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
                                                        .OfType<MutationStage>());
                        if (!dict.Contains(anchor))
                        {
                            Log.Error($"unable to find body addon on human with anchor id \"{anchor}\"!");
                            continue;
                        }

                        HediffGraphic hediffGraphic = GenerateGraphicsFor(mutationStages, mutation, anchor);
                        if (hediffGraphic == null) 
                            continue;

                        foreach (TaggedBodyAddon addon in dict[anchor])
                        {
                            if (addon.hediffGraphics == null) 
                                addon.hediffGraphics = new List<HediffGraphic>();

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
                                                                             .SelectMany(s => s.graphics
                                                                                .Where(g => g.anchorID == anchorID)
                                                                                .Select(g => (g.path, s.minSeverity)))
                                                                             .ToList();

            string mainPath = mainData.LastOrDefault()?.path ?? stageData.LastOrDefault().path; //get the main path 

            
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
                //if (mainData.Count > 0 || stageData.Count > 0)
                //{
                //    Log.Warning($"Found multiple graphics entries for {mutation.defName}-{anchorID} ({stage.label}): {{{String.Join(", ", mainData.Select(x => x.path))}}} {{{String.Join(", ", stage.graphics?.Select(x => x.path) ?? new List<string>())}}}");
                //}


                string path = null;
                if (stage.graphics != null)
                {
                    // Hide anchor graphics from mutation layer if not defined on specific stage.
                    path = stage.graphics.LastOrDefault(s => s.anchorID == anchorID)?.path ?? "Parts/None/None";
                }
                else
                {
                    // If no graphics are defined on stage then default to whatever is set on mutation.
                    path = mainPath;
                }

                //Log.Warning($"Selected {path} for stage {stage.label}({stage.minSeverity})");

                // fill the severity graphics if they are present in descending order
                if (string.IsNullOrWhiteSpace(path) == false)
                    severityLst.Add(new AlienPartGenerator.BodyAddonHediffSeverityGraphic
                    {
                        path = path,
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


        private static void PatchExplicitRaces()
        {
            List<AlienPartGenerator.BodyAddon> allAddons = GetAllAddonsToAdd().ToList();

            var explicitMorphs = MorphDef.AllDefs.Where(m => m.ExplicitHybridRace != null);

            foreach (var morph in explicitMorphs)
            {
                var aRace = morph.ExplicitHybridRace as ThingDef_AlienRace;
                if (aRace == null)
                {
                    Log.Warning($"could not transfer mutation graphics to {morph.ExplicitHybridRace.defName} because it is not a {nameof(ThingDef_AlienRace)}");
                    continue;
                }

                try
                {
                    if (morph.raceSettings?.transferHumanBodyAddons == true)
                    {
                        AddAddonsToRace(aRace, allAddons);
                        CheckDefaultOffsets(aRace, (ThingDef_AlienRace) ThingDefOf.Human); 
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"caught {e.GetType().Name} while trying to add mutation body graphics to {aRace.defName}!\n{e}");
                }

                if (aRace.modExtensions == null)
                    aRace.modExtensions = new List<DefModExtension>();

                aRace.modExtensions.Add(new RaceMutationSettingsExtension()
                {
                    mutationRetrievers = new List<IRaceMutationRetriever>()
                        {
                            new Hediffs.MutationRetrievers.AnimalClassRetriever()
                            {
                                animalClass = morph,
                            }
                        }
                });


                if (aRace.comps == null)
                    aRace.comps = new List<CompProperties>();

                aRace.comps.Add(new MorphTrackingCompProperties());
                aRace.comps.Add(new CompProperties(typeof(InitialGraphicsComp)));
                aRace.comps.Add(new CompProperties(typeof(GraphicsUpdaterComp)));
            }
        }

        private static void AddMutationsToWhitelistedRaces()
        {
            if (PawnmorpherMod.Settings.visibleRaces == null)
                return;

            List<AlienPartGenerator.BodyAddon> allAddons = GetAllAddonsToAdd().ToList();

            foreach (string raceDefName in PawnmorpherMod.Settings.visibleRaces)
            {
                ThingDef_AlienRace race = DefDatabase<ThingDef>.GetNamedSilentFail(raceDefName) as ThingDef_AlienRace;
                if (race == null)
                    continue;

                try
                {
                    AddAddonsToRace(race, allAddons);
                    CheckDefaultOffsets(race, (ThingDef_AlienRace)ThingDefOf.Human);
                }
                catch (Exception e)
                {
                    Log.Error($"caught {e.GetType().Name} while trying to add mutation body graphics to {race.defName}!\n{e}");
                }
            }
        }


        //make sure the implicit race has all default offset listed 
        private static void CheckDefaultOffsets([NotNull] ThingDef_AlienRace aRace, [NotNull] ThingDef_AlienRace human)
        {
            var aSettings = aRace.alienRace.generalSettings.alienPartGenerator;
            var hSettings = human.alienRace.generalSettings.alienPartGenerator;
            if (hSettings?.offsetDefaults == null) return; 
            if (aSettings.offsetDefaults == null)
            {
                aSettings.offsetDefaults = hSettings.offsetDefaults;
                return;
            }

            foreach (AlienPartGenerator.OffsetNamed hDefaultOffset in hSettings.offsetDefaults)
            {
                if (aSettings.offsetDefaults.All(a => a.name != hDefaultOffset.name))
                {
                    aSettings.offsetDefaults.Add(hDefaultOffset); 
                }
            }

        }

        [NotNull]
        private static IEnumerable<AlienPartGenerator.BodyAddon> GetAllAddonsToAdd()
        {
            var addons = ((ThingDef_AlienRace) ThingDefOf.Human).alienRace.generalSettings.alienPartGenerator.bodyAddons.MakeSafe();
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
                if (bodyAddon is TaggedBodyAddon tagged)
                {
                    // Skip if anchor already exists.
                    if (partGen.bodyAddons.Any(x => (x as TaggedBodyAddon)?.anchorID == tagged.anchorID))
                        continue;
                }

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
            string aID = (addon as TaggedBodyAddon)?.anchorID; 
            var copy = new TaggedBodyAddon()
            { 
                anchorID = aID,
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
                offsets = addon.offsets ?? new AlienPartGenerator.BodyAddonOffsets(),
                linkVariantIndexWithPrevious = addon.linkVariantIndexWithPrevious,
                inFrontOfBody = addon.inFrontOfBody,
                layerInvert = addon.layerInvert,
                variantCount = addon.variantCount,
                hediffGraphics = addon.hediffGraphics.MakeSafe().ToList(),
                alignWithHead = addon.alignWithHead,
                

                hiddenUnderApparelTag = addon.hiddenUnderApparelTag,
                defaultOffsets = addon.defaultOffsets,
                defaultOffset = addon.defaultOffset,
                drawSizePortrait = addon.drawSizePortrait,
                
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
