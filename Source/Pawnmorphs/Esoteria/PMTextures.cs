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

		/// <summary>
		/// Gets the merging icon.
		/// </summary>
		/// <value>
		/// The merging icon.
		/// </value>
		public static Texture2D MergingIcon { get; } = ContentFinder<Texture2D>.Get("UI/Commands/Merge");



		/// <summary>
		/// Gets a purple mutagenic hazard logo.
		/// </summary>
		public static Texture2D MutagenicHazardEther { get; } = ContentFinder<Texture2D>.Get("UI/MutagenicHazardEther");

		/// <summary>
		/// Gets a red mutagenic hazard logo.
		/// </summary>
		public static Texture2D MutagenicHazardHigh { get; } = ContentFinder<Texture2D>.Get("UI/MutagenicHazardHigh");

		/// <summary>
		/// Gets a yellow mutagenic hazard logo.
		/// </summary>
		public static Texture2D MutagenicHazardMid { get; } = ContentFinder<Texture2D>.Get("UI/MutagenicHazardMid");

		/// <summary>
		/// Gets a green mutagenic hazard logo.
		/// </summary>
		public static Texture2D MutagenicHazardLow { get; } = ContentFinder<Texture2D>.Get("UI/MutagenicHazardLow");


		public static Texture2D SquencerStrand { get; } = ContentFinder<Texture2D>.Get("UI/SequencerProgressStrand");
	}
}