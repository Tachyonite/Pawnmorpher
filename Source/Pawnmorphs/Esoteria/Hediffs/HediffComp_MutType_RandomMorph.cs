using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// A HediffComp_MutationType that picks a random morph def from a list, and
	/// then returns mutations and TFs from that def
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.Composable.MutTypes_FromComp"/>
	/// <seealso cref="Pawnmorph.Hediffs.Composable.TFTypes_FromComp"/>
	public class HediffComp_MutType_RandomMorph : HediffComp_MutTypeBase_Dynamic
	{
		/// <summary>
		/// Gets the hediff comp properties.
		/// </summary>
		/// <value>The properties.</value>
		public HediffCompProperties_MutType_RandomMorph Props => (HediffCompProperties_MutType_RandomMorph)props;

		/// <summary>
		/// Gets the morph def.
		/// </summary>
		/// <returns>The morph def.</returns>
		protected override MorphDef GetMorphDef()
		{
			return Props.morphDefs.RandomElement();
		}
	}

	/// <summary>
	/// Hediff comp properties for HediffComp_MutType_RandomClassMorph
	/// </summary>
	public class HediffCompProperties_MutType_RandomMorph : HediffCompPropertiesBase<HediffComp_MutType_RandomMorph>
	{
		/// <summary>
		/// The list of possible morph defs to choose from
		/// </summary>
		[UsedImplicitly] public List<MorphDef> morphDefs;

		/// <summary>
		/// Returns any config errors in the def
		/// </summary>
		/// <returns>The errors.</returns>
		/// <param name="parentDef">Parent def.</param>
		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (var error in base.ConfigErrors(parentDef))
				yield return error;
			if (morphDefs == null)
				yield return "HediffComp_MutationType_RandomMorph morphDefs is null!";
		}
	}
}
