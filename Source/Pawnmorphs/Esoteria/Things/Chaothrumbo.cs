// Chaothrumbo.cs created by Iron Wolf for Pawnmorph on 06/26/2021 10:09 AM
// last updated 06/26/2021  10:09 AM

using System;
using Pawnmorph.Thoughts;
using RimWorld;
using Verse;

namespace Pawnmorph.Things
{
    /// <summary>
    /// class for the chaothrumbo with observation thoughts 
    /// </summary>
    /// <seealso cref="Verse.Pawn" />
    /// <seealso cref="RimWorld.IThoughtGiver" />
    public class Chaothrumbo : Pawn, IThoughtGiver
    {
        private static ThoughtDef _observationDef;


        /// <summary>
        /// Gives the observed thought.
        /// </summary>
        /// <returns></returns>
        public Thought_Memory GiveObservedThought()
        {
            var mem = (Memory_FactionObservation) ThoughtMaker.MakeThought(ObservationDef);

            mem.ObservedThing = this;
            return mem; 

        }


        private static ThoughtDef ObservationDef
        {
            get
            {
                if (_observationDef == null) _observationDef = DefDatabase<ThoughtDef>.GetNamed("PM_CThrumboAmbient");

                return _observationDef;
            }
        }
    }
}