// Worker_Naked.cs created by Iron Wolf for Pawnmorph on 04/26/2020 6:16 PM
// last updated 04/26/2020  6:16 PM

using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts

{
	/// <summary>
	/// worker for giving sapience specific thought for when a pawn is naked 
	/// </summary>
	/// <seealso cref="Pawnmorph.Thoughts.Worker_Sapience" />
	public class Worker_SapientNaked : Worker_Sapience
	{
		/// <summary>
		/// Currents the state internal.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.apparel?.PsychologicallyNude != true) return false;
			return base.CurrentStateInternal(p);
		}
	}
}