using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// A HediffComp_MutType that picks a random morph def from a class and then
	/// returns mutations and TFs from that def.
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.HediffComp_MutTypeBase"/>
	/// <seealso cref="Pawnmorph.Hediffs.HediffComp_MutTypeBase_Dynamic"/>
	public class HediffComp_MutType_RandomClassMorph : HediffComp_MutTypeBase_Dynamic
	{
		/// <summary>
		/// Gets the hediff comp properties.
		/// </summary>
		/// <value>The properties.</value>
		public HediffCompProperties_MutType_RandomClassMorph Props => (HediffCompProperties_MutType_RandomClassMorph)props;

		/// <summary>
		/// Gets the morph def.
		/// </summary>
		/// <returns>The morph def.</returns>
		protected override MorphDef GetMorphDef()
		{
			return Props.animalClassDef.GetAllMorphsInClass()
					.Where(d => Props.allowRestricted || !d.Restricted)
					.RandomElement();
		}
	}

	/// <summary>
	/// Hediff comp properties for HediffComp_MutType_RandomClassMorph
	/// </summary>
	public class HediffCompProperties_MutType_RandomClassMorph : HediffCompPropertiesBase<HediffComp_MutType_RandomAnyMorph>
	{
		/// <summary>
		/// The animal class def to use
		/// </summary>
		[UsedImplicitly] public AnimalClassDef animalClassDef;

		/// <summary>
		/// Whether or not restricted morph defs can be selected
		/// </summary>
		[UsedImplicitly] public bool allowRestricted;

		/// <summary>
		/// Returns any config errors in the def
		/// </summary>
		/// <returns>The errors.</returns>
		/// <param name="parentDef">Parent def.</param>
		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (var error in base.ConfigErrors(parentDef))
				yield return error;
			if (animalClassDef == null)
				yield return "HediffComp_MutType_RandomClassMorph animalClassDef is null!";
		}
	}
}
