// InjectorGenerator.cs created by Iron Wolf for Pawnmorph on 09/13/2021 7:24 AM
// last updated 09/13/2021  7:24 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities;
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
				if (morphDef.injectorDef == null && morphDef.noInjector == false)
				{
					morphDef.injectorDef = CreateInjectorDefFor(morphDef);
					if (morphDef.injectorDef != null)
						_generatedInjectorDefs.Add(morphDef.injectorDef);
				}

			ResolveThingCategories();
		}

		private static ThingDef CreateInjectorDefFor([NotNull] MorphDef mDef)
		{
			if (mDef == null) throw new ArgumentNullException(nameof(mDef));

			//check for an explicitly made injector that isn't linked 
			string defName = CreateInjectorDefName(mDef);

			ThingDef collision = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
			if (collision != null)
			{
				Log.Warning($"found predefined def {defName} that wasn't listed in {mDef.defName}'s {nameof(MorphDef.injectorDef)} field!");
				mDef.injectorDef = collision;
				return null;
			}

			MorphInjectorProperties props = mDef.injectorProperties;
			if (props == null)
			{
				return null;
			}

			var comps = new List<CompProperties>
			{
				new CompProperties_Drug {listOrder = 1000},
				new CompProperties_UseEffectPlaySound() { soundOnUsed = SoundDefOf.MechSerumUsed},
				new CompProperties_Forbiddable()
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
				outcomeDoers = GetOutcomeDooers(mDef),
				preferability = FoodPreferability.NeverForNutrition,
				drugCategory = DrugCategory.None
			};
			var tDef = new ThingDef
			{
				defName = defName,
				label = GetInjectorLabel(mDef),
				modContentPack = mDef.modContentPack,
				graphicData = props.graphicData,
				description = props.description,
				thingClass = typeof(ThingWithComps),
				useHitPoints = true,
				resourceReadoutPriority = ResourceCountPriority.Middle,
				category = ThingCategory.Item,
				statBases = GetStatModifiers(props),
				thingCategories = new List<ThingCategoryDef> { PMThingCategoryDefOf.Injector },
				rotatable = false,
				techLevel = props.techLevel,
				alwaysHaulable = true,
				pathCost = 14,
				allowedArchonexusCount = -1,
				stackLimit = 75,
				selectable = true,
				altitudeLayer = AltitudeLayer.Item,
				drawGUIOverlay = true,
				drawerType = DrawerType.MapMeshOnly,
				ingestible = ingestProps,
				uiIconPath = props.graphicData.texPath,
				tradeTags = props.traderTags,
				costList = props.CostList.ToList(),
				recipeMaker = props.RecipeMaker,
				smallVolume = true,
				orderedTakeGroup = PMThingDefOf.MechaniteSlurry.orderedTakeGroup,


				socialPropernessMatters = true,
				comps = comps
			};

			AddMutagenCause(tDef);

			return tDef;
		}

		private static void AddMutagenCause(ThingDef tDef)
		{
			if (tDef.modExtensions == null)
				tDef.modExtensions = new List<DefModExtension>();

			tDef.modExtensions.Add(new MutationCauseExtension()
			{
				rulePackDef = PMRulePackDefOf.InjectorCauseLogPack
			});
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
				baseOutcomes.Add(new IngestionOutcomeDoer_GiveHediff { hediffDef = mDef.fullTransformation, severity = 1 });

			return baseOutcomes;
		}

		static void ResolveThingCategories()
		{

			List<ThingCategoryDef> lst = new List<ThingCategoryDef>();
			//manually put the generated thing defs into their categories 
			foreach (ThingDef injectorDef in GeneratedInjectorDefs)
			{
				foreach (ThingCategoryDef cat in injectorDef.thingCategories.MakeSafe())
				{

					cat.childThingDefs?.Add(injectorDef);
					if (!lst.Contains(cat)) lst.Add(cat);
				}
			}

			//now call resolve references again on these, should be fine 
			foreach (ThingCategoryDef cat in lst)
			{
				cat.ResolveReferences();
			}


			foreach (var item in DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.building?.fixedStorageSettings != null))
				item.building.fixedStorageSettings.filter.ResolveReferences();
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