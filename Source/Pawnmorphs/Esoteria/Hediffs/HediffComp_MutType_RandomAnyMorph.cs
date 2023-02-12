using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// A HediffComp_MutationType that picks a completely random morph def and
	/// then returns mutations and TFs from that def.
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.HediffComp_MutTypeBase"/>
	/// <seealso cref="Pawnmorph.Hediffs.HediffComp_MutTypeBase_Dynamic"/>
	public class HediffComp_MutType_RandomAnyMorph : HediffComp_MutTypeBase_Dynamic
	{
		/// <summary>
		/// Gets the hediff comp properties.
		/// </summary>
		/// <value>The properties.</value>
		public HediffCompProperties_MutType_RandomAnyMorph Props => (HediffCompProperties_MutType_RandomAnyMorph)props;

		/// <summary>
		/// Gets the morph def.
		/// </summary>
		/// <returns>The morph def.</returns>
		protected override MorphDef GetMorphDef()
		{
			return DefDatabase<MorphDef>.AllDefs
					.Where(d => Props.allowRestricted || !d.Restricted)
					.RandomElement();
		}
	}

	/// <summary>
	/// Hediff comp properties for HediffComp_MutType_RandomAnyMorph
	/// </summary>
	public class HediffCompProperties_MutType_RandomAnyMorph : HediffCompPropertiesBase<HediffComp_MutType_RandomAnyMorph>
	{
		/// <summary>
		/// Whether or not restricted morph defs can be selected
		/// </summary>
		[UsedImplicitly] public bool allowRestricted = false;
	}
}
