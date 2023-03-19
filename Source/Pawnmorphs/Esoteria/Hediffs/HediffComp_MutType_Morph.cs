using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// A simple HediffComp_MutType that selects mutations and TFs from a specific
	/// morph def defined in the XML.
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.HediffComp_MutTypeBase"/>
	public class HediffComp_MutType_Morph : HediffComp_MutTypeBase
	{
		/// <summary>
		/// Gets the hediff comp properties.
		/// </summary>
		/// <value>The properties.</value>
		public HediffCompProperties_MutType_Morph Props => (HediffCompProperties_MutType_Morph)props;

		/// <summary>
		/// Returns a list of mutations all MutTypes_FromComp stages will use
		/// </summary>
		/// <returns>The mutations.</returns>
		public override IEnumerable<MutationDef> GetMutations()
		{
			return Props.morphDef.AllAssociatedMutations;
		}

		/// <summary>
		/// Gets the TF.
		/// </summary>
		/// <returns>The TF.</returns>
		public override IEnumerable<PawnKindDef> GetTFs()
		{
			var animals = Props.morphDef.AllAssociatedAnimals;
			return DefDatabase<PawnKindDef>.AllDefs
					.Where(p => animals.Contains(p.race));
		}

		/// <summary>
		/// Generates a debug string indicating the status of the comp
		/// </summary>
		/// <returns>The debug string.</returns>
		public override string CompDebugString()
		{
			StringBuilder builder = new StringBuilder(base.CompDebugString());
			builder.AppendLine("MutType_Morph");
			builder.AppendLine($"  Morph Def: {Props.morphDef.defName}");
			return builder.ToString();
		}
	}

	/// <summary>
	/// Hediff comp properties for HediffComp_MutType_Morph
	/// </summary>
	public class HediffCompProperties_MutType_Morph : HediffCompPropertiesBase<HediffComp_MutType_Morph>
	{
		/// <summary>
		/// The morph def to use for mutations.
		/// </summary>
		[UsedImplicitly] public MorphDef morphDef;

		/// <summary>
		/// Returns any config errors in the def
		/// </summary>
		/// <returns>The errors.</returns>
		/// <param name="parentDef">Parent def.</param>
		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (var error in base.ConfigErrors(parentDef))
				yield return error;
			if (morphDef == null)
				yield return "HediffComp_MutType_Morph morphDef is null!";
		}
	}
}
