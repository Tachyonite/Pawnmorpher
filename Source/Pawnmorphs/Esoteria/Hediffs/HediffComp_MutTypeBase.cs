using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// An abstract base comp for all comps to to be used with MutType_FromComp
	/// and TFType_FromComp.  These comps track mutation state to allow different
	/// hediff stages to share the same mutation/TF types
	/// 
	/// Note: Not using HediffCompBase here because the property types change based
	/// on subclass and that gets awkward when trying to generically reference the
	/// base comp type
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.Composable.MutTypes_FromComp"/>
	/// <seealso cref="Pawnmorph.Hediffs.Composable.TFTypes_FromComp"/>
	public abstract class HediffComp_MutTypeBase : HediffComp
	{
		/// <summary>
		/// Returns a list of mutations all MutTypes_FromComp stages will use
		/// </summary>
		/// <returns>The mutations.</returns>
		public abstract IEnumerable<MutationDef> GetMutations();

		/// <summary>
		/// Gets the TF.
		/// </summary>
		/// <returns>The TF.</returns>
		public abstract IEnumerable<PawnKindDef> GetTFs();
	}

	/// <summary>
	/// An abstract base comp for all HediffComp_MutTypes that select a single
	/// morph def and persist it.
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.Composable.MutTypes_FromComp"/>
	/// <seealso cref="Pawnmorph.Hediffs.Composable.TFTypes_FromComp"/>
	public abstract class HediffComp_MutTypeBase_Dynamic : HediffComp_MutTypeBase
	{
		/// <summary>
		/// The morph def.
		/// </summary>
		protected MorphDef morphDef;

		/// <summary>
		/// Gets the morph def to use for this instance.
		/// </summary>
		/// <returns>The morph def.</returns>
		protected abstract MorphDef GetMorphDef();

		/// <summary>
		/// Returns a list of mutations all MutTypes_FromComp stages will use
		/// </summary>
		/// <returns>The mutations.</returns>
		public override IEnumerable<MutationDef> GetMutations()
		{
			return morphDef.AllAssociatedMutations;
		}

		/// <summary>
		/// Gets the TF.
		/// </summary>
		/// <returns>The TF.</returns>
		public override IEnumerable<PawnKindDef> GetTFs()
		{
			var animals = morphDef.AllAssociatedAnimals;
			return DefDatabase<PawnKindDef>.AllDefs
					.Where(p => animals.Contains(p.race));
		}

		/// <summary>
		/// Called after the base hediff is added
		/// </summary>
		/// <param name="dinfo">Dinfo.</param>
		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			base.CompPostPostAdd(dinfo);
			morphDef = GetMorphDef();
		}

		/// <summary>
		/// Saves/Loads data from XML
		/// </summary>
		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Defs.Look(ref morphDef, nameof(morphDef));
		}

		/// <summary>
		/// Generates a debug string used when inspecting hediffs in debug modew
		/// </summary>
		/// <returns>The debug string.</returns>
		public override string CompDebugString()
		{
			StringBuilder builder = new StringBuilder(base.CompDebugString());
			builder.AppendLine("MutType_Dynamic");
			builder.AppendLine($"  Current Morph Def: {morphDef.defName}");
			return builder.ToString();
		}
	}
}
