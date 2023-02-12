// InstinctInfluenceExtension.cs modified by Iron Wolf for Pawnmorph on 12/07/2019 2:54 PM
// last updated 12/07/2019  2:54 PM

using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// def extension that adds information about changing the instinct level of a sapient animal 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	/// exactly what this does depends on what kind of def it is attached to 
	public class InstinctInfluenceExtension : DefModExtension
	{
		/// <summary>
		/// The instinct offset to add 
		/// </summary>
		public int instinctOffset;

		/// <summary>
		/// The tale to add for the sapient animal 
		/// </summary>
		[CanBeNull]
		public TaleDef tale;

		/// <summary>
		/// The thought to add 
		/// </summary>
		public ThoughtDef thought;
	}
}