// MorphTracker.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/09/2019 9:03 AM
// last updated 09/09/2019  9:03 AM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// map component for tracking morphs by type and group on a map 
    /// </summary>
    public class MorphTracker : MapComponent
    {
        /// <summary>
        /// delegate for notifying when the morph count changed 
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="morph">The morph.</param>
        public delegate void MorphCountChangedHandle(MorphTracker sender, MorphDef morph);

        /// <summary> Event that is raised every time the morph count on the attached map changes. </summary>
        public event MorphCountChangedHandle MorphCountChanged;

        private Dictionary<MorphDef, int> _counterDict = new Dictionary<MorphDef, int>();
        /// <summary>
        /// Initializes a new instance of the <see cref="MorphTracker"/> class.
        /// </summary>
        /// <param name="map">The map.</param>
        public MorphTracker(Map map) : base(map)
        {
        }

        /// <summary> Notify this tracker that the pawn has spawned. </summary>
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

        /// <summary> Get the number of morphs belonging to the given group active in the map. </summary>
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
        
        /// <summary> Get the number of morphs active on this map. </summary>
        public int this[MorphDef morph]
        {
            get { return _counterDict.TryGetValue(morph);  }
        }

        /// <summary> Notify the map that the pawn has despawned from the map. </summary>
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

        /// <summary> Notify this tracker that the pawn race has changed. </summary>
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
}