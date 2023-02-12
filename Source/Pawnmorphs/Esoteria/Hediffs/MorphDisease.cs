// MorphDisease.cs modified by Iron Wolf for Pawnmorph on 11/24/2019 3:43 PM
// last updated 11/24/2019  3:43 PM

using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// hediff for morph diseases
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.MorphTf" />
	public class MorphDisease : MorphTf
	{
		private HediffComp_Immunizable ImmunizableComp => this.TryGetComp<HediffComp_Immunizable>();

		/// <summary>
		/// Gets the severity label.
		/// </summary>
		/// <value>
		/// The severity label.
		/// </value>
		public override string SeverityLabel
		{
			get
			{
				if (def.maxSeverity <= 0) return null;

				return (Severity / def.maxSeverity).ToStringPercent();
			}
		}



		/// <summary>
		/// returns true if there are ny mutations in this stage 
		/// </summary>
		/// <param name="stage"></param>
		/// <returns></returns>
		protected override bool AnyMutationsInStage(HediffStage stage)
		{

			if (ImmunizableComp?.FullyImmune == true) return false; //stop giving mutations after they are immune 
			return base.AnyMutationsInStage(stage);
		}
	}
}