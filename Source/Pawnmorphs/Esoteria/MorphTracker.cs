// MorphTracker.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/09/2019 9:03 AM
// last updated 09/09/2019  9:03 AM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// map component for tracking morphs by type & group on a map 
    /// </summary>
    public class MorphTracker : MapComponent
    {
        public delegate void MorphCountChangedHandle(MorphTracker sender, MorphDef morph);

        /// <summary>
        /// event that is raised every time the morph count on the attached map changes
        /// </summary>
        public event MorphCountChangedHandle MorphCountChanged;

        private Dictionary<MorphDef, int> _counterDict = new Dictionary<MorphDef, int>(); 

        public MorphTracker(Map map) : base(map)
        {
        }

        /// <summary>
        ///     notify this tracker that the pawn has spawned
        /// </summary>
        /// <param name="pawn"></param>
        public void NotifySpawned(Pawn pawn)
        {
            var morph = pawn.def.GetMorphOfRace();
            if (morph != null)
            {
                var i = _counterDict.TryGetValue(morph);
                i++;
                _counterDict[morph] = i; 
                MorphCountChanged?.Invoke(this, morph); 
            }


        }


        /// <summary>
        /// get the number of morphs belonging to the given group active in the map
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public int GetGroupCount([NotNull] MorphGroupDef group)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));
            int counter = 0;
            foreach (MorphDef morphDef in group.MorphsInGroup)
            {
                counter += this[morphDef];
            }

            return counter; 

        }


        /// <summary>
        /// get the number of morphs active on this map 
        /// </summary>
        /// <param name="morph"></param>
        /// <returns></returns>
        public int this[MorphDef morph]
        {
            get { return _counterDict.TryGetValue(morph);  }
        }

        /// <summary>
        /// notify the map that the pawn has despawned from the map 
        /// </summary>
        /// <param name="pawn"></param>
        public void NotifyDespawned(Pawn pawn)
        {
            var morph = pawn.def.GetMorphOfRace();
            if (morph != null)
            {
                var i = _counterDict.TryGetValue(morph) - 1;
                i = Mathf.Max(0, i); 
                _counterDict[morph] = i;
                MorphCountChanged?.Invoke(this, morph); 
            }
        }

        /// <summary>
        /// notify this tracker that the pawn race has changed 
        /// </summary>
        /// <param name="pawn"></param>
        public void NotifyPawnRaceChanged(Pawn pawn, [CanBeNull] MorphDef oldMorph)
        {
            if (oldMorph != null)
            {
                var i = _counterDict.TryGetValue(oldMorph) - 1;
                i = Mathf.Max(0, i);
                _counterDict[oldMorph] = i;

                MorphCountChanged?.Invoke(this, oldMorph); 
            }

            var morph = pawn.def.GetMorphOfRace();
            if (morph != null)
            {
                var i = _counterDict.TryGetValue(morph) + 1;
                _counterDict[morph] = i;
                MorphCountChanged?.Invoke(this, morph); 
            }
        }

    }


    public class MorphTrackingCompProperties : CompProperties
    {
        public MorphTrackingCompProperties()
        {
            compClass = typeof(MorphTrackingComp);
        }
    } 

}