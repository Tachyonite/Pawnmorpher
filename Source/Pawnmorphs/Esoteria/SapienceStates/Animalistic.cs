// Animalistic.cs created by Iron Wolf for Pawnmorph on 04/25/2020 2:17 PM
// last updated 04/25/2020  2:17 PM

using System;
using Verse;

namespace Pawnmorph.SapienceStates
{
    /// <summary>
    /// sapience state for 'animalistic' humanoids 
    /// </summary>
    /// <seealso cref="Pawnmorph.SapienceState" />
    public class Animalistic : SapienceState
    {
        /// <summary>
        ///     Gets the current intelligence.
        /// </summary>
        /// <value>
        ///     The current intelligence.
        /// </value>
        public override Intelligence CurrentIntelligence
        {
            get
            {
                switch (CurrentSapience)
                {
                    case SapienceLevel.Sapient:
                    case SapienceLevel.MostlySapient:
                    case SapienceLevel.Conflicted:
                        return Intelligence.Humanlike;
                    case SapienceLevel.MostlyFeral:
                        return Intelligence.ToolUser;
                    case SapienceLevel.Feral:
                    case SapienceLevel.PermanentlyFeral:
                        return Intelligence.Animal;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///     called after every tick
        /// </summary>
        public override void Tick()
        {
        }

        /// <summary>
        ///     called to save/load all data.
        /// </summary>
        protected override void ExposeData()
        {
            
        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        /// this is always called before enter and after loading a pawn
        protected override void Init()
        {
            
        }
    }
}