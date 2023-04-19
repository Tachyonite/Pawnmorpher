using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Verse;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// Properties for the CanBeFormerHuman comp
	/// </summary>
	public class CompProperties_CanBeFormerHuman : CompProperties
	{
		/// <summary>
		/// If true, the animal will always be a former human, regardless of the mod settings
		/// </summary>
		[UsedImplicitly]
		protected bool always;

		/// <summary>
		/// If true, the animal will always be a former human, regardless of the mod settings
		/// </summary>
		public virtual bool Always => always;

		/// <summary>
		/// Returns any config errors in this comp property
		/// </summary>
		/// <returns>The errors.</returns>
		/// <param name="parentDef">Parent def.</param>
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (var error in base.ConfigErrors(parentDef))
				yield return error;

			if (parentDef?.category != ThingCategory.Pawn)
				yield return $"{nameof(Comp_CanBeFormerHuman)} attached to a non-pawn thingdef.";

			bool neverFormerHuman = parentDef.GetModExtension<FormerHumanSettings>()?.neverFormerHuman ?? false;
			if (always && neverFormerHuman)
				yield return $"{nameof(Comp_CanBeFormerHuman)} attached to a def explicitly defined as never being a former human.";
		}

		/// <summary>
		/// create a new instance of this class
		/// </summary>
		public CompProperties_CanBeFormerHuman()
		{
			compClass = typeof(Comp_CanBeFormerHuman);
		}
	}
}
