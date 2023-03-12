using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// hediff type for mutagenic buildup
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.MorphTf" />
	/// should add more and more mutations as severity increases, with a full tf at a severity of 1
	public class MutagenicBuildup : MorphTf
	{
		/// <summary>
		/// Tries the merge with the other hediff
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public override bool TryMergeWith(Hediff other)
		{
			if (other is MutagenicBuildup buildup)
			{

				Severity += other.Severity;
				foreach (HediffComp hediffComp in comps)
				{
					hediffComp.CompPostMerged(other);
				}

				ResetMutationOrder();
				return true;
			}

			return false;
		}
	}


}