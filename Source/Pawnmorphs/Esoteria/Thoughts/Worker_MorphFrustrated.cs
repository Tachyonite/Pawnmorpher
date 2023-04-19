// Worker_MorphFrustraited.cs modified by Iron Wolf for Pawnmorph on 01/05/2020 3:45 PM
// last updated 01/05/2020  3:45 PM

using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// thought worker for the 'morph' frustrated thought
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker" />
	public class Worker_MorphFrustrated : ThoughtWorker
	{
		/// <summary>
		/// gets the current thought state of the given pawn.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.IsFormerHuman()) return false; //disable this for former humans 

			var hediffs = p.health?.hediffSet?.hediffs;
			foreach (Hediff hediff in hediffs.MakeSafe())
			{
				if (hediff is Hediff_AddedMutation) return false;
			} //only true if there are no mutations 
			  //avoiding LINQ for performance reasons 

			return true;
		}
	}
}