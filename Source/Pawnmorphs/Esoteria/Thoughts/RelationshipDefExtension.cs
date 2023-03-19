// RelationshipExtension.cs modified by Iron Wolf for Pawnmorph on 07/30/2019 1:48 PM
// last updated 07/30/2019  1:48 PM

using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// extension info to add onto Relationships 
	/// </summary>
	public class RelationshipDefExtension : DefModExtension
	{
		/// <summary>the thought for when the male variant of this relation ship is tf'd </summary>
		/// ex: when a husband is transformed 
		public ThoughtDef transformThought;
		/// <summary>
		/// the thought for when the female variant of this relationship is transformed 
		/// </summary>
		/// ex: when a wife is transformed 
		public ThoughtDef transformThoughtFemale;
		/// <summary>The reverted thought for the male variant</summary>
		public ThoughtDef revertedThought;
		/// <summary>
		/// The reverted thought for the female variant 
		/// </summary>
		public ThoughtDef revertedThoughtFemale;
		/// <summary>the thought for when the pawn goes permanently feral</summary>
		public ThoughtDef permanentlyFeral;
		/// <summary>
		/// the female variant thought for when the pawn 
		/// </summary>
		public ThoughtDef permanentlyFeralFemale;

		/// <summary>
		/// the thought for when the female variant is merged 
		/// </summary>
		public ThoughtDef mergedThoughtFemale;

		/// <summary>the thought for when the male variant is merged</summary>
		public ThoughtDef mergedThought;


	}
}