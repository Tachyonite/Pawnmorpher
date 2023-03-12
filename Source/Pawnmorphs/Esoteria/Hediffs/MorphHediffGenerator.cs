// MorphHediffGenerator.cs created by Iron Wolf for Pawnmorph on 09/13/2021 7:25 AM
// last updated 09/13/2021  7:25 AM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Composable.Hediffs;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hediffs.Composable;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	///     static class responsible for generating implicit morph injector hediffs
	/// </summary>
	public static class MorphHediffGenerator
	{
		private const string REELING_LABEL = "PMReelingLabel";
		private const string TRANSFORMING_LABEL = "PMTransformingLabel";
		private const string CHANGING_LABEL = "PMChangingLabel";
		private const string FULL_HEDIFF_DESCRIPTION = "PMFullMorphTfDescription";
		private const string PARTIAL_HEDIFF_DESCRIPTION = "PMPartialMorphTfDescription";
		[NotNull] private static readonly List<HediffDef> _allGeneratedHediffs = new List<HediffDef>();


		/// <summary>
		/// Gets all generated hediff defs.
		/// </summary>
		/// <value>
		/// All generated hediff defs.
		/// </value>
		[NotNull]
		public static IReadOnlyList<HediffDef> AllGeneratedHediffDefs => _allGeneratedHediffs;


		/// <summary>
		/// Generates all morph hediffs.
		/// </summary>
		public static void GenerateAllMorphHediffs()
		{
			foreach (MorphDef morphDef in MorphDef.AllDefs)
			{
				TryGenerateHediffs(morphDef);
			}
		}

		/// <summary>
		/// Tries to generate the transformation hediffs for the given morph .
		/// </summary>
		/// <param name="mDef">The m definition.</param>
		/// <exception cref="ArgumentNullException">mDef</exception>
		static void TryGenerateHediffs([NotNull] MorphDef mDef)
		{
			if (mDef == null) throw new ArgumentNullException(nameof(mDef));

			if (mDef.fullTransformation == null && mDef.fullTfHediffProps != null)
			{
				HediffDef fullHDef = GenerateFullHediffFor(mDef);
				if (fullHDef != null)
				{
					_allGeneratedHediffs.Add(fullHDef);
					mDef.fullTransformation = fullHDef;
				}
			}

			if (mDef.partialTransformation == null && mDef.partialTfHediffProps != null)
			{
				HediffDef partial = GeneratePartialHediffFor(mDef);
				if (partial != null)
				{
					_allGeneratedHediffs.Add(partial);
					mDef.partialTransformation = partial;
				}
			}
		}

		private static HediffDef CreateHediffDefBase(string defName, string label, string description,
													 [NotNull] MorphHediffProperties hDiffProps, [NotNull] MorphDef mDef)
		{
			var mods = new List<DefModExtension>
			{
				new MutagenExtension {mutagen = hDiffProps.mutagen ?? MutagenDefOf.defaultMutagen}
			};

			List<AspectGiver> givers = hDiffProps.aspectGivers;
			if (givers != null) mods.Add(new AspectGiverExtension { aspectGivers = givers });

			var comps = new List<HediffCompProperties>
			{
				new HediffCompProperties_SeverityPerDay {severityPerDay = -0.1f},
				new CompProperties_CheckRace {triggerStage = 1},
				new HediffCompProperties_Immunizable(),
				new CompProps_TfStageConfigChecker(),
				new CompProps_RemoveNonMorphPart {removeChance = hDiffProps.removeNonMorphPartChance}
			};

			var hDef = new HediffDef
			{
				defName = defName,
				label = label,
				hediffClass = typeof(Hediff_MutagenicBase),
				description = description,
				defaultLabelColor = hDiffProps.labelColor,
				scenarioCanAdd = true,
				isBad = false,
				maxSeverity = 1,
				initialSeverity = 1,
				comps = comps,
				modExtensions = mods,
				modContentPack = mDef.modContentPack
			};
			return hDef;
		}

		private static HediffStage GenerateChangingStage([NotNull] MorphDef mDef, [NotNull] MorphHediffProperties hediffProps)
		{
			var capMods = new List<PawnCapacityModifier>
			{
				new PawnCapacityModifier {capacity = PawnCapacityDefOf.Moving, offset = -0.05f},
				new PawnCapacityModifier {capacity = PawnCapacityDefOf.Sight, offset = -0.05f},
				new PawnCapacityModifier {capacity = PawnCapacityDefOf.Manipulation, offset = -0.05f}
			};


			var stage = new HediffStage_Mutation
			{
				label = CHANGING_LABEL.Translate(),
				minSeverity = CHANGING_MIN_SEVERITY,
				painOffset = 0.1f,
				vomitMtbDays = 1,
				capMods = capMods,
				mutationTypes = new MutTypes_Morph { morphDef = mDef },
				mutationRate = new MutRate_MutationsPerDay { meanMutationsPerDay = 7.7f }, //should this be in settings 
				spreadOrder = new MutSpreadOrder_RandomSpread()
			};
			return stage;
		}

		static HediffDef GeneratePartialHediffFor([NotNull] MorphDef morphDef)
		{
			if (morphDef == null) throw new ArgumentNullException(nameof(morphDef));
			MorphHediffProperties partialProps = morphDef.partialTfHediffProps;
			if (partialProps == null)
			{
				Log.Error($"{morphDef.defName} has no {nameof(MorphDef.partialTfHediffProps)}!");
				return null;
			}

			HediffDef hDiff = CreateHediffDefBase(MorphHediffDefNamePartial(morphDef.defName),
												  MorphHediffLabel(morphDef, partialProps),
												  MorphHediffDescription(morphDef, partialProps, PARTIAL_HEDIFF_DESCRIPTION),
												  partialProps, morphDef);


			hDiff.stages = GeneratePartialStages(morphDef);
			hDiff.comps = hDiff.comps ?? new List<HediffCompProperties>();
			var partialComp = new HediffCompProperties_Single
			{
				maxStacks = 10 //override somewhere? 
			};

			hDiff.comps.Add(partialComp);
			return hDiff;

		}

		private static HediffDef GenerateFullHediffFor([NotNull] MorphDef morphDef)
		{
			if (morphDef.fullTfHediffProps == null)
			{
				Log.Error($"{morphDef.defName} has null {nameof(MorphDef.fullTfHediffProps)}!");
				return null;
			}

			HediffDef hDiff = CreateHediffDefBase(MorphHediffDefName(morphDef.defName),
												  MorphHediffLabel(morphDef, morphDef.fullTfHediffProps),
												  MorphHediffDescription(morphDef, morphDef.fullTfHediffProps,
																		 FULL_HEDIFF_DESCRIPTION), morphDef.fullTfHediffProps, morphDef);

			hDiff.stages = GenerateFullStages(morphDef);

			return hDiff;
		}

		private static List<HediffStage> GenerateFullStages([NotNull] MorphDef mDef)
		{
			MorphHediffProperties fTfProps = mDef.fullTfHediffProps;
			if (fTfProps == null)
			{
				Log.Error($"unable to find full transformation hediff props in {mDef.defName}!");
				return null;
			}

			var lst = new List<HediffStage>
			{
				GenerateReelingStage(mDef, fTfProps),
				GenerateTransformingStage(mDef, fTfProps),
				GenerateChangingStage(mDef, fTfProps)
			};
			return lst;
		}



		private static List<HediffStage> GeneratePartialStages([NotNull] MorphDef mDef)
		{
			MorphHediffProperties tfProps = mDef.partialTfHediffProps;
			if (tfProps == null)
			{
				Log.Error($"unable to find partial transformation hediff props in {mDef.defName}!");
				return null;
			}

			return new List<HediffStage> { GenerateChangingStage(mDef, tfProps) };
		}

		[NotNull]
		private static HediffStage GenerateReelingStage([NotNull] MorphDef mDef, [NotNull] MorphHediffProperties hediffProps)
		{
			if (hediffProps == null) throw new ArgumentNullException(nameof(hediffProps));

			var capMods = new List<PawnCapacityModifier>
			{
				new PawnCapacityModifier {capacity = PawnCapacityDefOf.Moving, offset = -0.05f}
			};


			var stg = new HediffStage_Transformation
			{
				label = REELING_LABEL.Translate(),
				painOffset = 0.5f,
				hungerRateFactor = 1.6f, //in settings? 
				tfChance = new TFChance_BySetting(),
				tfGenderSelector = new TFGenderSelector_Swap { chance = 0.1f }, //should this be in properties? 
				tfTypes = new TFTypes_Morph { morphDef = mDef },
				capMods = capMods,
				tfSettings = hediffProps.tfSettings ?? new TFMiscSettings() //do we need to make a copy of this? 
			};

			return stg;
		}


		private const float TF_MIN_SEVERITY = 0.4f;
		private const float CHANGING_MIN_SEVERITY = 0.65f;
		private static HediffStage GenerateTransformingStage([NotNull] MorphDef mDef, [NotNull] MorphHediffProperties hediffProps)
		{
			var capMods = new List<PawnCapacityModifier>
			{
				new PawnCapacityModifier {capacity = PawnCapacityDefOf.Moving, offset = -0.05f},
				new PawnCapacityModifier {capacity = PawnCapacityDefOf.Sight, offset = -0.05f},
				new PawnCapacityModifier {capacity = PawnCapacityDefOf.Manipulation, offset = -0.05f}
			};

			var stage = new HediffStage_MutagenicBase
			{
				label = TRANSFORMING_LABEL.Translate(),
				minSeverity = TF_MIN_SEVERITY,
				painOffset = 0.1f,
				vomitMtbDays = 1,
				alert = hediffProps.tfAlert ?? new StageAlert(),
				capMods = capMods
			};
			return stage;
		}

		private static string MorphHediffDefName(string morphName)
		{
			return "Pawnmorph" + morphName + "TF";
		}

		private static string MorphHediffDefNamePartial(string morphName)
		{
			return MorphHediffDefName(morphName) + "Partial";
		}

		private static string MorphHediffDescription([NotNull] MorphDef morphDef, MorphHediffProperties hediffProps,
													 string translationLabel)
		{
			if (!string.IsNullOrEmpty(hediffProps?.description))
				return hediffProps.description;
			return translationLabel.Translate(morphDef.Named("Morph"), morphDef.adjective.Named("MorphAdjective"),
											  morphDef.race.Named("Animal"));
		}

		private static string MorphHediffLabel([NotNull] MorphDef morphDef, MorphHediffProperties hediffProps)
		{
			return string.IsNullOrEmpty(hediffProps?.label) ? morphDef.label : hediffProps.label;
		}
	}
}