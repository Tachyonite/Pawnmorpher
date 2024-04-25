// ConditionalSapientAnimal.cs created by Iron Wolf for Pawnmorph on 03/01/2020 11:00 AM
// last updated 03/01/2020  11:00 AM

using Verse;
using Verse.AI;

namespace Pawnmorph.ThinkNodes
{
	/// <summary>
	///     conditional think nodes for sapient animals
	/// </summary>
	/// <seealso cref="Verse.AI.ThinkNode_Conditional" />
	public class ConditionalSapientAnimal : ThinkNode_Conditional
	{
		/// <summary>
		///     if the pawn must be a colonist to
		/// </summary>
		public bool mustBeColonist = true;

		/// <summary>
		///  if true, the pawn must be a sapient or mostly sapient former human to qualify for this node 
		/// </summary>
		public bool mustBeFullySapient = false;

		/// <summary>
		///     checks if this think node is satisfied or not
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override bool Satisfied(Pawn pawn)
		{
			//if mustBeFullySapient is true, make sure we use IsSapientFormerHuman, other use SapientOrFeral 
			if (!pawn.RaceProps.Animal) return false;
			var sapienceLevel = pawn.GetQuantizedSapienceLevel() ?? SapienceLevel.PermanentlyFeral;
			var cutoff = mustBeFullySapient ? SapienceLevel.MostlySapient : SapienceLevel.MostlyFeral;
			bool cond = sapienceLevel <= cutoff;
			if (!cond) return false;
			if (mustBeColonist) return pawn.IsColonist;
			return true;
		}
	}
}