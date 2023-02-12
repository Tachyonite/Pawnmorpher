// InstinctEffector.cs modified by Iron Wolf for Pawnmorph on 12/09/2019 5:50 PM
// last updated 12/09/2019  5:50 PM

using RimWorld;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// def extension to make a def affect a sapient animal's instinct/sapience level 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class InstinctEffector : DefModExtension
	{
		/// <summary>
		/// The base instinct offset
		/// </summary>
		public int baseInstinctOffset;

		/// <summary>
		/// The thought to add to the sapient animal 
		/// </summary>
		public ThoughtDef thought;

		/// <summary>
		/// The tale definition to add 
		/// </summary>
		public TaleDef taleDef;
	}
}