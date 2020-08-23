// GenomeGenerator.cs created by Iron Wolf for Pawnmorph on 08/07/2020 1:50 PM
// last updated 08/07/2020  1:50 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.ThingComps;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Chambers
{
    /// <summary>
    /// static class for generating and storing all implicit genome items 
    /// </summary>
    public static class GenomeDefGenerator
    {
        [NotNull]
        private static IEnumerable<MutationCategoryDef> AllImplicitGenomeMutations =>
            DefDatabase<MutationCategoryDef>.AllDefs.Where(m => m.genomeProvider &&  m.AllMutations.Any(mm => mm.isTaggable) && m.explicitGenomeDef == null);

        private static List<ThingDef> _allImplicitGenomes; 

        [NotNull]
        private static MethodInfo GiveHashMethod { get; }

        static GenomeDefGenerator()
        {
            GiveHashMethod = typeof(ShortHashGiver).GetMethod("GiveShortHash", BindingFlags.NonPublic | BindingFlags.Static);

        }

        [NotNull]
        private static readonly object[] _tmpArr = new object[] {null, typeof(ThingDef)};

        static void GiveShortHash(ThingDef def)

        {
            _tmpArr[0] = def; 
            GiveHashMethod.Invoke(null, _tmpArr); 
        }

        /// <summary>
        /// Generates the genomes.
        /// </summary>
        internal static void GenerateGenomes()
        {
            if (_allImplicitGenomes != null)
            {
                Log.Error($"trying to generate genomes more then once!");

                return;
            }

            _allImplicitGenomes = new List<ThingDef>();


            foreach (var mDef in AllImplicitGenomeMutations)
            {
                var tDef = GenerateMutationGenome(mDef);
                mDef.implicitGenomeDef = tDef; 
                _allImplicitGenomes.Add(tDef);
            }

            foreach (ThingDef allImplicitGenome in _allImplicitGenomes)
            {
                GiveShortHash(allImplicitGenome);
                DefDatabase<ThingDef>.Add(allImplicitGenome); 
            }


        }

        /// <summary>
        /// Gets all implied genomes.
        /// </summary>
        /// <value>
        /// All implied genomes.
        /// </value>
        [NotNull]
        public static IReadOnlyList<ThingDef> AllImpliedGenomes
        {
            get
            {
                if (_allImplicitGenomes ==null)
                {
                    GenerateGenomes();
                }

                return _allImplicitGenomes; 
            }
        }

        static bool IsDepricated([NotNull] MutationDef def)
        {
            if(string.Compare("depricated", def.label, StringComparison.InvariantCultureIgnoreCase) == 0)return true;
            return string.Compare("obsolete", def.label, StringComparison.InvariantCultureIgnoreCase) == 0; 
        }

        private const string GENOME_PREAMBLE = "PM_Genome_";
        private const string LABEL_TTAG = "GenomeLabel";

        [NotNull]
        static GraphicData GenerateGenomeGraphicData([NotNull] MutationCategoryDef mDef)
        {
            return new GraphicData()
            {
                graphicClass = typeof(Graphic_Single),
                texPath = "Things/Item/Special/TechprintUltratech" //TODO replace with our own graphics 
            };
        }

        /// <summary>The genome set maker tag</summary>
        public const string GENOME_SET_MAKER_TAG = "Genome";

        /// <summary>The genome trader tags</summary>
        public const string GENOME_TRADER_TAGS = "Genome"; 


        static float GetGenomeMarketValue([NotNull] MutationCategoryDef mDef)
        {
            float averageMkValue = 0;
            int counter = 0; 
            foreach (MutationDef mutationDef in mDef.AllMutations.Where(m => m.isTaggable))
            {
                averageMkValue += mutationDef.GetMarketValueFor();
                counter++;
            }
            


            return Mathf.Max(100, 100 + averageMkValue / counter); //don't go below 100 silver for any mutations 
        }


        static void SetGenomeStats([NotNull] ThingDef tDef, [NotNull] MutationCategoryDef mDef)
        {



            tDef.SetStatBaseValue(StatDefOf.MaxHitPoints, 100); 
            tDef.SetStatBaseValue(StatDefOf.Flammability, 1);
            tDef.SetStatBaseValue(StatDefOf.MarketValue, GetGenomeMarketValue(mDef));
            tDef.SetStatBaseValue(StatDefOf.Mass, 0.03f); 
            tDef.SetStatBaseValue(StatDefOf.SellPriceFactor, 0.1f);
        }

        static void AddComps([NotNull] ThingDef tDef, [NotNull] MutationCategoryDef mDef)
        {
            var comps = new List<CompProperties>()
            {
                new CompProperties_Forbiddable(),
                new MutationGenomeStorageProps() {mutation = mDef}
            };

            

            tDef.comps = comps; 

        }

        private const string GENOME_DESC_TAG = "GenomeDesc"; 

        [NotNull]
        static ThingDef GenerateMutationGenome([NotNull] MutationCategoryDef mDef)
        {


            var tDef = new ThingDef()
            {
                defName = GENOME_PREAMBLE + mDef.defName + "_Implicit",
                label = LABEL_TTAG.Translate(mDef.Named("MUTATION")),
                description = GetGenomeDesc(mDef),
                resourceReadoutPriority = ResourceCountPriority.Middle,
                category = ThingCategory.Item,
                thingClass = typeof(ThingWithComps),
                thingCategories = new List<ThingCategoryDef>() { PMThingCategoryDefOf.PM_MutationGenome },
                graphicData = GenerateGenomeGraphicData(mDef),
                useHitPoints = true,
                selectable = true,
                thingSetMakerTags = new List<string>() { GENOME_SET_MAKER_TAG },
                altitudeLayer = AltitudeLayer.Item,
                tickerType = TickerType.Never,
                rotatable = false,
                pathCost = 15,
                drawGUIOverlay = true,
                modContentPack = mDef.modContentPack,
                tradeTags = new List<string>() { GENOME_TRADER_TAGS },

            };

            SetGenomeStats(tDef, mDef);
            AddComps(tDef, mDef);

            return tDef;
        }

        private static string GetGenomeDesc([NotNull] MutationCategoryDef mDef)
        {
            if (!string.IsNullOrEmpty(mDef.customGenomeDescription)) return mDef.customGenomeDescription; 
            return GENOME_DESC_TAG.Translate(mDef.Named("MUTATION"));
        }


    }
}