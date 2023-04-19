// ChaomorphExtension.cs created by Iron Wolf for Pawnmorph on 09/26/2020 5:27 PM
// last updated 09/26/2020  5:27 PM

using JetBrains.Annotations;
using RimWorld;
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

		/// <summary>
		/// if this chaomorph can be stored in the genebank 
		/// </summary>
		public bool storable;

		/// <summary>
		/// if this chaomorph can be tagged 
		/// </summary>
		public bool taggable;

		/// <summary>
		/// The pawn kind definition
		/// </summary>
		[CanBeNull]
		public PawnKindDef pawnKindDef;


		/// <summary>
		/// optional tale for when a pawn tf into this kind of chaomorph 
		/// </summary>
		[CanBeNull] public TaleDef customTfTale;
	}
}