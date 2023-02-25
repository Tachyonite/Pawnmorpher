using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// capacity impactor for aspects 
	/// </summary>
	public class AspectCapacityImpactor : PawnCapacityUtility.CapacityImpactor
	{
		/// <summary>
		/// create a new aspect impactor instance 
		/// </summary>
		/// <param name="aspect"></param>
		public AspectCapacityImpactor(Aspect aspect)
		{
			Aspect = aspect;
		}

		/// <summary>
		/// the aspect that is impacting the pawn 
		/// </summary>
		public Aspect Aspect { get; }

		/// <summary>
		/// if this impactor is direct or not 
		/// </summary>
		public override bool IsDirect => false;

		/// <summary>
		/// return a string describing what is impacting the capacity of the pawn 
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		public override string Readable(Pawn pawn)
		{
			return Aspect.Label;
		}
	}
}
