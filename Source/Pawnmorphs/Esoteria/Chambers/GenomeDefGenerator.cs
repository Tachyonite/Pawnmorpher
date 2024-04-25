// GenomeGenerator.cs created by Iron Wolf for Pawnmorph on 08/07/2020 1:50 PM
// last updated 08/07/2020  1:50 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Hediffs;
using Pawnmorph.ThingComps;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Chambers
{
	/// <summary>
	///     static class for generating and storing all implicit genome items
	/// </summary>
	public static class GenomeDefGenerator
	{
		private const string GENOME_PREAMBLE = "PM_Genome_";
		private const string LABEL_TTAG = "GenomeLabel";

		/// <summary>The genome set maker tag</summary>
		public const string GENOME_SET_MAKER_TAG = "Genome";

		/// <summary>The genome trader tags</summary>
		public const string GENOME_TRADER_TAGS = "Genome";

		private const string GENOME_DESC_TAG = "GenomeDesc";
		private const string GENOME_PK_PREAMBLE = "AnimalGenome_";


		private const string ANIMAL_GENOME_DESC_TAG = "PMAnimalGenomeDescription";

		private static List<ThingDef> _allImplicitGenomes;

		[NotNull] private static readonly Dictionary<PawnKindDef, ThingDef> _genomeDict = new Dictionary<PawnKindDef, ThingDef>();

		/// <summary>
		///     Gets all implied genomes.
		/// </summary>
		/// <value>
		///     All implied genomes.
		/// </value>
		[NotNull]
		public static IReadOnlyList<ThingDef> AllImpliedGenomes
		{
			get
			{
				return _allImplicitGenomes;
			}
		}

		[NotNull]
		private static IEnumerable<MutationCategoryDef> AllImplicitGenomeMutations =>
			DefDatabase<MutationCategoryDef>.AllDefs.Where(m => m.genomeProvider
															 && m.AllMutations.Any(mm => mm.isTaggable)
															 && m.explicitGenomeDef == null);

		[NotNull] private static MethodInfo GiveHashMethod { get; }

		[NotNull]
		private static
			IEnumerable<PawnKindDef> AllPKsWithGenomes
		{
			get
			{
				return MorphDef.AllDefs.Where(m => m.categories == null || m.categories?.Contains(MorphCategoryDefOf.Chimera) == false)
							   .SelectMany(x => x.FeralPawnKinds)
							   .Distinct();
			}
		}

		/// <summary>
		///     Tries to get the genome for the given pawnKindDef
		/// </summary>
		/// <param name="pawnKindDef">The pawn kind definition.</param>
		/// <returns></returns>
		[CanBeNull]
		public static ThingDef TryGetGenomeFor([NotNull] this PawnKindDef pawnKindDef)
		{
			return _genomeDict.TryGetValue(pawnKindDef);
		}

		/// <summary>
		///     Generates the genomes.
		/// </summary>
		internal static void GenerateGenomes()
		{
			if (_allImplicitGenomes != null)
			{
				Log.Error("trying to generate genomes more then once!");

				return;
			}

			_allImplicitGenomes = new List<ThingDef>();
			var catDef = PMThingCategoryDefOf.PM_MutationGenome;
			catDef.childThingDefs = catDef?.childThingDefs ?? new List<ThingDef>();

			foreach (MutationCategoryDef mDef in AllImplicitGenomeMutations)
			{
				ThingDef tDef = GenerateMutationGenome(mDef);
				mDef.implicitGenomeDef = tDef;
				_allImplicitGenomes.Add(tDef);
			}

			//foreach (PawnKindDef pk in AllPKsWithGenomes)
			//{
			//	ThingDef tDef = GenerateAnimalGenome(pk);
			//	_genomeDict[pk] = tDef;
			//	_allImplicitGenomes.Add(tDef);
			//}


			foreach (ThingDef allImplicitGenome in _allImplicitGenomes)
			{
				try
				{
					Init(allImplicitGenome);
					DefDatabase<ThingDef>.Add(allImplicitGenome);
					catDef.childThingDefs.Add(allImplicitGenome);
				}
				catch (Exception e)
				{
					Log.Error($"could not initialize genome {allImplicitGenome.defName ?? "NO DEF NAME"}\n{e}");
				}

			}

			if (DebugLogUtils.ShouldLog(LogLevel.Messages))
			{
				var builder = new StringBuilder();
				builder.Append($"Generated {_allImplicitGenomes.Count} genomes!:");
				builder.AppendLine(_allImplicitGenomes.Join(t => t.defName, "\n"));
				Log.Message(builder.ToString());
			}

		}



		private static void Init(ThingDef allImplicitGenome)
		{
			HashGiverUtils.GiveShortHash(allImplicitGenome);
			// GiveShortHash(allImplicitGenome);
			allImplicitGenome.ResolveReferences();
			_configErrorCache.Clear();
			_configErrorCache.AddRange(allImplicitGenome.ConfigErrors().MakeSafe());
			if (_configErrorCache.Count > 0)
			{
				StringBuilder builder = new StringBuilder();
				builder.AppendLine($"errors in {allImplicitGenome.defName}!");
				foreach (string error in _configErrorCache)
				{
					builder.AppendLine(error);
				}
				Log.Error(builder.ToString());
			}

		}

		private static void AddComps([NotNull] ThingDef tDef, [NotNull] MutationCategoryDef mDef)
		{
			var comps = new List<CompProperties>
			{
				new CompProperties_Forbiddable(),
				new MutationGenomeStorageProps {mutation = mDef}
			};

			tDef.comps = comps;
		}


		private static void AddComps([NotNull] ThingDef tDef, [NotNull] PawnKindDef mDef)
		{
			var comps = new List<CompProperties>
			{
				new CompProperties_Forbiddable(),
				new AnimalGenomeStorageCompProps {animal = mDef}
			};

			tDef.comps = comps;
		}

		[NotNull]
		private static readonly List<string> _configErrorCache = new List<string>();

		[NotNull]
		private static ThingDef GenerateAnimalGenome([NotNull] PawnKindDef mDef)
		{

			string desc = GetGenomeDesc(mDef);
			string label = LABEL_TTAG.Translate(mDef.Named("MUTATION"));
			GraphicData gData = GenerateGenomeGraphicData(mDef);
			ThingDef tDef = GenerateNewDef(mDef, desc, label, gData);

			SetGenomeStats(tDef, mDef);
			AddComps(tDef, mDef);




			return tDef;
		}

		private static ThingDef GenerateNewDef(Def mDef, string desc, string label, GraphicData gData)
		{
			return new ThingDef
			{
				defName = GENOME_PREAMBLE + mDef.defName + "_Implicit",
				label = label,
				description = desc,
				resourceReadoutPriority = ResourceCountPriority.Middle,
				category = ThingCategory.Item,
				thingClass = typeof(ThingWithComps),
				alwaysHaulable = true,
				thingCategories = new List<ThingCategoryDef> { PMThingCategoryDefOf.PM_MutationGenome },
				graphicData = gData,
				useHitPoints = true,
				selectable = true,
				thingSetMakerTags = new List<string> { GENOME_SET_MAKER_TAG },
				altitudeLayer = AltitudeLayer.Item,
				tickerType = TickerType.Never,
				rotatable = false,
				pathCost = DefGenerator.StandardItemPathCost,
				drawGUIOverlay = true,

				modContentPack = mDef.modContentPack,
				tradeTags = new List<string> { GENOME_TRADER_TAGS },

			};
		}

		[NotNull]
		private static GraphicData GenerateGenomeGraphicData([NotNull] MutationCategoryDef mDef)
		{
			return new GraphicData
			{
				graphicClass = typeof(Graphic_Single),
				texPath = "Things/Item/Genecard"
			};
		}

		private static GraphicData GenerateGenomeGraphicData([NotNull] PawnKindDef mDef)
		{
			return new GraphicData
			{
				graphicClass = typeof(Graphic_Single),
				texPath = "Things/Item/Genecard"
			};
		}

		[NotNull]
		private static ThingDef GenerateMutationGenome([NotNull] MutationCategoryDef mDef)
		{
			string desc = GetGenomeDesc(mDef);
			string label = LABEL_TTAG.Translate(mDef.Named("MUTATION"));
			GraphicData gData = GenerateGenomeGraphicData(mDef);
			ThingDef tDef = GenerateNewDef(mDef, desc, label, gData);

			SetGenomeStats(tDef, mDef);
			AddComps(tDef, mDef);

			return tDef;
		}


		private static string GetGenomeDesc([NotNull] MutationCategoryDef mDef)
		{
			if (!string.IsNullOrEmpty(mDef.customGenomeDescription)) return mDef.customGenomeDescription;
			return GENOME_DESC_TAG.Translate(mDef.Named("MUTATION"));
		}

		private static string GetGenomeDesc([NotNull] PawnKindDef pKDef)
		{
			return ANIMAL_GENOME_DESC_TAG.Translate(pKDef.Named("ANIMAL"));
		}


		private static float GetGenomeMarketValue([NotNull] MutationCategoryDef mDef)
		{
			float averageMkValue = 0;
			var counter = 0;
			foreach (MutationDef mutationDef in mDef.AllMutations.Where(m => m.isTaggable))
			{
				averageMkValue += mutationDef.GetMarketValueFor();
				counter++;
			}


			return Mathf.Max(100, 100 + averageMkValue / counter); //don't go below 100 silver for any mutations 
		}

		private static float GetGenomeMarketValue([NotNull] PawnKindDef pkDef)
		{
			return Mathf.Max(100, 100 + pkDef.race.BaseMarketValue); //don't go below 100 silver for any pawnKindDef  
		}

		private static bool IsDepricated([NotNull] MutationDef def)
		{
			if (string.Compare("depricated", def.label, StringComparison.InvariantCultureIgnoreCase) == 0) return true;
			return string.Compare("obsolete", def.label, StringComparison.InvariantCultureIgnoreCase) == 0;
		}


		private static void SetGenomeStats([NotNull] ThingDef tDef, [NotNull] MutationCategoryDef mDef)
		{
			tDef.SetStatBaseValue(StatDefOf.MaxHitPoints, 100);
			tDef.SetStatBaseValue(StatDefOf.Flammability, 1);
			tDef.SetStatBaseValue(StatDefOf.MarketValue, GetGenomeMarketValue(mDef));
			tDef.SetStatBaseValue(StatDefOf.Mass, 0.03f);
			tDef.SetStatBaseValue(StatDefOf.SellPriceFactor, 0.1f);
		}

		private static void SetGenomeStats([NotNull] ThingDef tDef, [NotNull] PawnKindDef pkDef)
		{
			tDef.SetStatBaseValue(StatDefOf.MaxHitPoints, 100);
			tDef.SetStatBaseValue(StatDefOf.Flammability, 1);
			tDef.SetStatBaseValue(StatDefOf.MarketValue, GetGenomeMarketValue(pkDef));
			tDef.SetStatBaseValue(StatDefOf.Mass, 0.03f);
			tDef.SetStatBaseValue(StatDefOf.SellPriceFactor, 0.1f);
		}
	}
}