// ChaomorphExtension.cs created by Iron Wolf for Pawnmorph on 09/26/2020 5:27 PM
// last updated 09/26/2020  5:27 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.DefExtensions
{
    /// <summary>
    /// def extension to add to a ThingDef to mark the race as a chaomorph 
    /// </summary>
    /// <seealso cref="Verse.DefModExtension" />
    public class ChaomorphExtension : DefModExtension
    {

        /// <summary>
        /// the type of chaomorph 
        /// </summary>
        public ChaomorphType chaoType;

        /// <summary>
        /// The selection weight, used to determine how 'rare' a chaomorph is, higher values are more common. negative values make them never show up under normal means 
        /// </summary>
        public float selectionWeight; 

    }
}