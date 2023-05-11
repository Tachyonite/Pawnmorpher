using Verse;

namespace Pawnmorph.Hediffs.Utility
{
	/// <summary>
	/// Various extension methods for hediffs
	/// </summary>
	public static class HediffExtensions
	{
		/// <summary>
		/// Checks whether this hediff has immunity built up
		/// </summary>
		/// <returns><c>true</c>, if immune was ised, <c>false</c> otherwise.</returns>
		/// <param name="hediff">Hediff.</param>
		public static bool IsImmune(this HediffWithComps hediff)
		{
			bool immune = false;
			immune |= hediff.TryGetComp<Comp_ImmunizableMutation>()?.FullyImmune ?? false;
			immune |= hediff.TryGetComp<HediffComp_Immunizable>()?.FullyImmune ?? false;
			return immune;
		}

		/// <summary>
		/// How much the severity of this hediff is changing per day(used for certain components)
		/// This is somewhat expensive to calculate, so call sparingly.
		/// </summary>
		/// <value>The severity label.</value>
		public static float GetSeverityChangePerDay(this HediffWithComps hediff)
		{
			float gainRate = 0f;
			gainRate += hediff.TryGetComp<Comp_ImmunizableMutation>()?.SeverityChangePerDay() ?? 0f;
			gainRate += hediff.TryGetComp<HediffComp_Immunizable>()?.SeverityChangePerDay() ?? 0f;
			gainRate += hediff.TryGetComp<HediffComp_SeverityPerDay>()?.SeverityChangePerDay() ?? 0f;
			gainRate += hediff.TryGetComp<HediffComp_TendDuration>()?.SeverityChangePerDay() ?? 0f;
			return gainRate;
		}
	}
}
