// IMorphGraphicsController.cs created by Iron Wolf for Pawnmorph on 04/29/2020 5:32 PM
// last updated 04/29/2020  5:32 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace Pawnmorph.GraphicSys
{
    /// <summary>
    /// interface for some controller object that gets new colors for a pawn from a specific morph 
    /// </summary>
    public interface IMorphGraphicsGenerator
    {
        /// <summary>
        /// Gets all available channels in this .
        /// </summary>
        /// <value>
        /// The available channels.
        /// </value>
        IEnumerable<string> AvailableChannels { get; }

        /// <summary>
        /// Gets a generated color channel for a specific pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="channelID">The channel identifier.</param>
        /// <returns>the generated channel if possible, else null</returns>
        ColorChannel? GetChannel([NotNull] Pawn pawn, string channelID);

    }

    /// <summary>
    /// struct that represents a single 'color channel' as used by HAR, containing 2 separate 'sub channels' first, and second 
    /// </summary>
    public struct ColorChannel
    {
        Color first;
        private Color? second;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorChannel"/> struct.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        public ColorChannel(Color first, Color? second = null)
        {
            this.first = first;
            this.second = second; 
        }


        /// <summary>
        /// Gets the first 'sub channel' of this channel
        /// </summary>
        /// <value>
        /// The first.
        /// </value>
        public Color First => first;

        /// <summary>
        /// Gets the second.
        /// </summary>
        /// <value>
        /// The second.
        /// </value>
        public Color Second => second ?? first; 



    }

}