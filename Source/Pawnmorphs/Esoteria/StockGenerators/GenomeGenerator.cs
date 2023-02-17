// GenomeGenerator.cs created by Iron Wolf for Pawnmorph on 08/07/2020 6:01 PM
// last updated 08/07/2020  6:01 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.ThingComps;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.StockGenerators
{
    /// <summary>
    /// stock generator for generating genomes for sale 
    /// </summary>
    /// <seealso cref="RimWorld.StockGenerator" />
    public class GenomeGenerator : StockGenerator
    {
        /// <summary>
        /// if this should generate genomes for 'restricted' mutations 
        /// </summary>
        public bool allowRestricted;

        /// <summary>
        /// The category filter
        /// </summary>
        public Filter<MutationCategoryDef> categoryFilter;

        private List<ThingDef> _mutationPool;

        [NotNull]
        IReadOnlyList<ThingDef> GenomePool
        {
            get
            {
                if (_mutationPool == null)
                {
                    _mutationPool = new List<ThingDef>(); 
                    foreach (var mDef in DefDatabase<MutationCategoryDef>.AllDefs)
                    {
                        if(mDef.GenomeDef == null) continue;
                        if (categoryFilter?.PassesFilter(mDef) != false)
                        {
                            _mutationPool.Add(mDef.GenomeDef); 
                        }
                    }

                }

                return _mutationPool; 
            }
        }



        /// <summary>
        /// Generates the things.
        /// </summary>
        /// <param name="forTile">For tile.</param>
        /// <param name="faction">The faction.</param>
        /// <returns></returns>
        public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
        {
            if(GenomePool.Count == 0) yield break;

            List<ThingDef> scratchList = new List<ThingDef>();

            int counter = 0;
            var total = countRange.RandomInRange;
            while (scratchList.Count < total && counter < 100) //counter to prevent infinite loops 
            {
                counter++;
                var rGenome = GenomePool.RandomElement(); 
                if(scratchList.Contains(rGenome)) continue;
                var thing = ThingMaker.MakeThing(rGenome);
                scratchList.Add(rGenome); 
                yield return thing; 
            }

        }

        [Pure]
        bool CheckComp([CanBeNull] MutationGenomeStorageProps props)
        {
            var mDef = props?.mutation; 
            if (mDef == null) return false;
            if (categoryFilter?.PassesFilter(mDef) == false) return false; 

            return allowRestricted || mDef.restrictionLevel == RestrictionLevel.UnRestricted; 
        }

        /// <summary>
        /// checks if this generator handles the given props.
        /// </summary>
        /// <param name="thingDef">The thing definition.</param>
        /// <returns></returns>
        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return CheckComp(thingDef?.GetCompProperties<MutationGenomeStorageProps>());
        }
    }
}