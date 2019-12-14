// PMPlantUtilities.cs modified by Iron Wolf for Pawnmorph on 12/14/2019 1:27 PM
// last updated 12/14/2019  1:27 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// static class containing various plant related utility functions 
    /// </summary>
    public static class PMPlantUtilities
    {
        [NotNull] private static readonly LinkedList<MPlantEntry> _plantEntries;

        [NotNull]
        private static readonly List<ThingDef> _mutantPlants;

        /// <summary>
        /// Gets all mutant plants.
        /// </summary>
        /// <value>
        /// The mutant plants.
        /// </value>
        [NotNull] public static IEnumerable<ThingDef> MutantPlants => _mutantPlants;


        /// <summary>
        /// Determines whether this is a mutant plant .
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <returns>
        ///   <c>true</c> if this is a mutant plant; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMutantPlant([NotNull] this ThingDef def)
        {
            return _mutantPlants.Contains(def); 
        }

        [NotNull]
        private static readonly List<ThingDef> _scratchList = new List<ThingDef>();

        /// <summary>
        /// Gets the mutant version of the given plant
        /// </summary>
        /// <param name="plant">The plant.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">plant</exception>
        [CanBeNull]
        public static ThingDef GetMutantVersionOf([NotNull] Plant plant)
        {
            if (plant == null) throw new ArgumentNullException(nameof(plant));

            foreach (MPlantEntry mPlantEntry in _plantEntries) //go in order of the priorities 
            {

                _scratchList.Clear();
                foreach (ThingDef thingDef in mPlantEntry.plants) //grab each entry that can result from plant mutating 
                {
                    var mProps = thingDef.GetModExtension<MutantPlantExtension>();
                    if (mProps == null)
                    {
                        Log.Error($"{thingDef.defName} has no mutant plant extension but was added to the mutant plants entry list somehow");
                        continue;
                    }

                    if (mProps.CanMutateFrom(plant))
                    {
                        _scratchList.Add(thingDef); 
                    }
                }

                if(_scratchList.Count == 0) continue;

                return _scratchList.RandElement();
            }

            return null; //no valid plants found 
        }


        class MPlantEntry
        {
            public int priority;
            public List<ThingDef> plants; 
        }


        static PMPlantUtilities()
        {
            _plantEntries = new LinkedList<MPlantEntry>();
            _mutantPlants = new List<ThingDef>(); 
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                var mPlantProps = thingDef.GetModExtension<MutantPlantExtension>(); 

                if(mPlantProps == null) continue;
                if(thingDef.plant == null) continue;

                var node = _plantEntries.First;
                while (node != null && node.Value.priority < mPlantProps.priority)
                    node = node.Next; //get the correct place to insert 

                MPlantEntry entry; 
                
                if (node == null) //insert at end 
                {
                    entry = new MPlantEntry()
                    {
                        priority = mPlantProps.priority,
                        plants = new List<ThingDef>()
                    };
                    _plantEntries.AddLast(entry); 
                }else if (node.Value.priority == mPlantProps.priority)
                {
                    entry = node.Value; //don't insert at all
                }
                else //insert before 
                {
                    entry = new MPlantEntry()
                    {
                        priority = mPlantProps.priority,
                        plants = new List<ThingDef>()
                    };
                    _plantEntries.AddBefore(node, entry); 
                }

                entry.plants.Add(thingDef);
                _mutantPlants.Add(thingDef); 

            }

        }

    }
}