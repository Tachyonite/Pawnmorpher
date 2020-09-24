// PMTextures.cs created by Iron Wolf for Pawnmorph on 09/24/2020 4:27 PM
// last updated 09/24/2020  4:27 PM

using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// static class containing use 
    /// </summary>
    [StaticConstructorOnStartup]
    public static class PMTextures
    {
        /// <summary>
        /// Gets the animal selector icon.
        /// </summary>
        /// <value>
        /// The animal selector icon.
        /// </value>
        public static Texture2D AnimalSelectorIcon { get; } = ContentFinder<Texture2D>.Get("UI/Commands/AnimalSelector");
        /// <summary>
        /// Gets the part picker icon.
        /// </summary>
        /// <value>
        /// The part picker icon.
        /// </value>
        public static Texture2D PartPickerIcon { get; } = ContentFinder<Texture2D>.Get("UI/Commands/PartPicker");
        /// <summary>
        /// Gets the tagrifle icon.
        /// </summary>
        /// <value>
        /// The tagrifle icon.
        /// </value>
        public static Texture2D TagrifleIcon { get; } = ContentFinder<Texture2D>.Get("UI/Commands/TagRifleIcon");
    }
}