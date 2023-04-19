using System;
using System.Collections.Generic;
using Pawnmorph.ThingComps;
using Verse;

namespace Pawnmorph
{
	// Obsoleted because they were merged together and moving to the appropriate namespace

	/// <summary>
	/// Obsolete, use Comp_CanBeFormerHuman instead
	/// </summary>
	[Obsolete("Use " + nameof(Comp_CanBeFormerHuman) + " instead.")]
	public class CompFormerHumanChance : Comp_CanBeFormerHuman
	{
		/// <summary>
		/// Initialize the comp with the specific props.
		/// </summary>
		/// <param name="props">Properties.</param>
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			Log.Error("Pawnmorph.CompAlwaysFormerHuman is obsolete." +
				" Use " + nameof(Comp_CanBeFormerHuman) + " instead." +
				" This comp may be removed in the future." +
				" Note to players:  This error is harmless, your game should work fine.");
		}
	}

	/// <summary>
	/// Obsolete, use Comp_CanBeFormerHuman instead
	/// </summary>
	[Obsolete("Use " + nameof(Comp_CanBeFormerHuman) + " instead")]
	public class CompAlwaysFormerHuman : Comp_CanBeFormerHuman
	{
		/// <summary>
		/// Initialize the comp with the specific props.
		/// </summary>
		/// <param name="props">Properties.</param>
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			Log.Error("Pawnmorph.CompAlwaysFormerHuman is obsolete." +
				" Use " + nameof(Comp_CanBeFormerHuman) + " instead." +
				" This comp may be removed in the future." +
				" Note to players:  This error is harmless, your game should work fine.");
		}
	}

	/// <summary>
	/// Obsolete, use CompProperties_CanBeFormerHuman instead
	/// </summary>
	[Obsolete("Use " + nameof(CompProperties_CanBeFormerHuman) + " instead.")]
	public class CompProperties_FormerHumanChance : CompProperties_CanBeFormerHuman
	{
		/// <summary>
		/// If true, the animal will always be a former human, regardless of the mod settings
		/// </summary>
		public override bool Always => false;

		/// <summary>
		/// Returns any config errors in this comp property
		/// </summary>
		/// <returns>The errors.</returns>
		/// <param name="parentDef">Parent def.</param>
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (var err in base.ConfigErrors(parentDef))
				yield return err;
			yield return "Pawnmorph.CompProperties_FormerHumanChance is obsolete." +
				" Use " + nameof(CompProperties_CanBeFormerHuman) + " instead." +
				" This comp may be removed in the future." +
				" Note to players:  This error is harmless, your game should work fine.";
		}
	}

	/// <summary>
	/// Obsolete, use CompProperties_CanBeFormerHuman instead
	/// </summary>
	[Obsolete("Use " + nameof(CompProperties_CanBeFormerHuman) + " instead.")]
	public class CompProperties_AlwaysFormerHuman : CompProperties_CanBeFormerHuman
	{
		/// <summary>
		/// The hediff.
		/// </summary>
		public HediffDef hediff; // An old field, not used for anything

		/// <summary>
		/// If true, the animal will always be a former human, regardless of the mod settings
		/// </summary>
		public override bool Always => true;

		/// <summary>
		/// Returns any config errors in this comp property
		/// </summary>
		/// <returns>The errors.</returns>
		/// <param name="parentDef">Parent def.</param>
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (var err in base.ConfigErrors(parentDef))
				yield return err;
			yield return "Pawnmorph.CompProperties_AlwaysFormerHuman is obsolete." +
				" Use " + nameof(CompProperties_CanBeFormerHuman) + " instead." +
				" This comp may be removed in the future." +
				" Note to players:  This error is harmless, your game should work fine.";
		}
	}
}