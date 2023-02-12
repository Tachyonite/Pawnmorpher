// AspectDef.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 12:24 PM
// last updated 09/22/2019  12:24 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary> Def for all affinities. </summary>
	public class AspectDef : Def
	{
		/// <summary>
		///     the Type of the aspect
		/// </summary>
		public Type aspectType;

		/// <summary>
		///     the aspect stages, must be at least one
		/// </summary>
		[NotNull] public List<AspectStage> stages = new List<AspectStage>();

		/// <summary>
		///     the color of the aspect's label
		/// </summary>
		public Color labelColor = Color.white;

		/// <summary>
		///     if this aspect should be removed by a reverter or not
		/// </summary>
		public bool removedByReverter = true;

		/// <summary>Whether or not this aspect can be added by the scenario editor.</summary>
		public bool scenarioCanAdd = false;

		/// <summary>
		///     the priority of this aspect
		///     lower priorities come first
		/// </summary>
		public int priority = 1;

		/// <summary>
		///     if true, this aspect should be transferred to the new animal pawn if the original pawn has this aspect
		/// </summary>
		public bool transferToAnimal;

		/// <summary>
		///     list of thoughts this aspect nullifies
		/// </summary>
		[NotNull] public List<ThoughtDef> nullifiedThoughts = new List<ThoughtDef>();


		/// <summary>
		///     The conflicting aspects
		/// </summary>
		[NotNull] public List<AspectDef> conflictingAspects = new List<AspectDef>();

		/// <summary>
		///     The required traits
		/// </summary>
		public List<TraitDef> requiredTraits = new List<TraitDef>();

		/// <summary>
		///     The conflicting traits
		/// </summary>
		public List<TraitDef> conflictingTraits = new List<TraitDef>();

		/// <summary>
		///     get all configuration errors with this def
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors()) yield return configError;

			if ((stages?.Count ?? 0) == 0) yield return "no stages";
		}

		/// <summary>
		///     create a new aspect instance
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public Aspect CreateInstance()
		{
			var affinity = (Aspect)Activator.CreateInstance(aspectType);
			affinity.def = this;
			return affinity;
		}

		/// <summary> Get the affinity def with the given defName. </summary>
		public static AspectDef Named(string defName)
		{
			return DefDatabase<AspectDef>.GetNamed(defName);
		}

		/// <summary>
		///     resolve all def references in this def, called after DefOfs are loaded
		/// </summary>
		public override void ResolveReferences()
		{
			base.ResolveReferences();
			aspectType = aspectType ?? typeof(Aspect);
			if (!typeof(Aspect).IsAssignableFrom(aspectType))
				Log.Error($"in {defName}: affinityType {aspectType.Name} can not be converted to type {nameof(Aspect)}");
		}
	}
}