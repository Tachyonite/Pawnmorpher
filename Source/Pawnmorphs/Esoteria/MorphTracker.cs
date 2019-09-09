// MorphTracker.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/09/2019 9:03 AM
// last updated 09/09/2019  9:03 AM

using System;
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

        public MorphTracker(Map map) : base(map)
        {
        }

        /// <summary>
        ///     notify this tracker that the pawn has spawned
        /// </summary>
        /// <param name="pawn"></param>
        public void NotifySpawned(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// notify the map that the pawn has despawned from the map 
        /// </summary>
        /// <param name="pawn"></param>
        public void NotifyDespawned(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// notify this tracker that the pawn race has changed 
        /// </summary>
        /// <param name="pawn"></param>
        public void NotifyPawnRaceChanged(Pawn pawn)
        {
            throw new NotImplementedException();
        }

    }
}