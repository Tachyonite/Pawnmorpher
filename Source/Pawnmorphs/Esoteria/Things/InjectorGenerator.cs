// InjectorGenerator.cs created by Iron Wolf for Pawnmorph on 09/13/2021 7:24 AM
// last updated 09/13/2021  7:24 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.Things
{
    /// <summary>
    ///     static class responsible for generating implicitly defined morph injectors
    /// </summary>
    public static class InjectorGenerator
    {
        private const string DEFAULT_INJECTOR_LABEL = "PMInjectorLabel";

        private const string INGEST_COMMAND_STR = "PMInjectorIngestCommand";
        private const string INGEST_REPORT_STRING = "PMInjectorReport";

        [NotNull] private static readonly StatModifier[] _defaultStatModifiers;

        [NotNull] private static readonly List<ThingDef> _generatedInjectorDefs = new List<ThingDef>();

        static InjectorGenerator()
        {
            _defaultStatModifiers = new[]
            {
                new StatModifier {stat = StatDefOf.Mass, value = 0.01f},
                new StatModifier {stat = StatDefOf.WorkToMake, value = 4000},
                new StatModifier {stat = StatDefOf.MarketValue, value = 400}
            };
        }

        /// <summary>
        ///     Gets all generated injector defs.
        /// </summary>
        /// <value>
        ///     The generated injector defs.
        /// </value>
        [NotNull]
        public static IReadOnlyList<ThingDef> GeneratedInjectorDefs => _generatedInjectorDefs;

        /// <summary>
        ///     Generates the injector defs.
        /// </summary>
        public static void GenerateInjectorDefs()
        {
            foreach (MorphDef morphDef in DefDatabase<MorphDef>.AllDefsListForReading)
                if (morphDef.injectorDef == null)
                {
                    morphDef.injectorDef = CreateInjectorDefFor(morphDef);
                    if(morphDef.injectorDef != null)
                        _generatedInjectorDefs.Add(morphDef.injectorDef);
                }
        }

        private static ThingDef CreateInjectorDefFor([NotNull] MorphDef mDef)
        {
            if (mDef == null) throw new ArgumentNullException(nameof(mDef));
            MorphInjectorProperties props = mDef.injectorProperties;
            if (props == null)
            {
                Log.Error($"{mDef.defName} has null {nameof(MorphDef.injectorProperties)}!");
                return null;
            }

            var comps = new List<CompProperties>
            {
                new CompProperties_Drug {listOrder = 1000}
            };

            var ingestProps = new IngestibleProperties
            {
                foodType = FoodTypeFlags.Processed,
                baseIngestTicks = 120,
                chairSearchRadius = 0,
                ingestSound = PMSoundDefOf.Ingest_Inject,
                ingestHoldUsesTable = false,
                ingestCommandString =
                    INGEST_COMMAND_STR.Translate()
                                      .Replace("<", "{")
                                      .Replace(">", "}"), //hacky way to get "nested" translation strings 
                ingestReportString = INGEST_REPORT_STRING.Translate().Replace("<", "{").Replace(">", "}"),
                outcomeDoers = GetOutcomeDooers(mDef)
            };
            var tDef = new ThingDef
            {
                defName = CreateInjectorDefName(mDef),
                label = GetInjectorLabel(mDef),
                modContentPack = mDef.modContentPack,
                graphicData = props.graphicData,
                statBases = GetStatModifiers(props),
                thingCategories = new List<ThingCategoryDef> {PMThingCategoryDefOf.Injector, ThingCategoryDefOf.Drugs},
                rotatable = false,
                techLevel = props.techLevel,
                ingestible = ingestProps,
                tradeTags = props.traderTags,
                costList = props.CostList.ToList(),
                recipeMaker = props.RecipeMaker,
                socialPropernessMatters = true,
                comps = comps
            };

            return tDef;
        }

        private static string CreateInjectorDefName(MorphDef mDef)
        {
            return mDef.defName + "Transformer";
        }

        private static string GetInjectorLabel(MorphDef mDef)
        {
            if (!string.IsNullOrEmpty(mDef.injectorProperties?.label)) return mDef.injectorProperties.label;
            return DEFAULT_INJECTOR_LABEL.Translate(mDef.race.label.Named("Animal"));
        }

        private static List<IngestionOutcomeDoer> GetOutcomeDooers(MorphDef mDef)
        {
            List<IngestionOutcomeDoer> baseOutcomes = mDef.injectorProperties?.outcomeDoers ?? new List<IngestionOutcomeDoer>();

            if (baseOutcomes.OfType<IngestionOutcomeDoer_GiveHediff>().All(d => d.hediffDef != mDef.fullTransformation))
                baseOutcomes.Add(new IngestionOutcomeDoer_GiveHediff {hediffDef = mDef.fullTransformation, severity = 1});

            return baseOutcomes;
        }

        private static List<StatModifier> GetStatModifiers([NotNull] MorphInjectorProperties props)
        {
            List<StatModifier> baseStats = props.statBases ?? new List<StatModifier>();
            foreach (StatModifier defaultStatModifier in _defaultStatModifiers)
                if (!baseStats.Any(s => s.stat == defaultStatModifier.stat))
                    baseStats.Add(defaultStatModifier);

            return baseStats;
        }
    }
}