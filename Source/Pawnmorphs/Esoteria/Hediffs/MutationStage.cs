// DescriptiveStage.cs modified by Iron Wolf for Pawnmorph on 12/14/2019 7:32 PM
// last updated 12/14/2019  7:32 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.GraphicSys;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// hediff stage with an extra description field  
	/// </summary>
	/// <seealso cref="Verse.HediffStage" />
	public class MutationStage : HediffStage, IDescriptiveStage, IExecutableStage
	{
		/// <summary>
		/// Optional key that can be used to reference back to this specific stage.
		/// </summary>
		[CanBeNull]
		public string key;

		/// <summary>
		/// list of all aspect givers in this stage 
		/// </summary>
		[CanBeNull]
		public List<AspectGiver> aspectGivers;

		/// <summary>
		/// optional description override for a hediff in this stage 
		/// </summary>
		public string description;

		/// <summary>
		/// the max health offset of this particular part 
		/// </summary>
		public float healthOffset = 0;

		/// <summary>
		/// the max health modifier of this pawn's bodyparts.
		/// </summary>
		public float globalHealthMultiplier = 0;

		/// <summary>
		/// The label override
		/// </summary>
		public string labelOverride;

		/// <summary>
		/// the base chance that the mutation will stop progressing at this stage  
		/// </summary>
		/// this should be in [0,1]
		public float stopChance;

		/// <summary>
		/// memory to add when this stage is entered 
		/// </summary>
		public ThoughtDef memory;

		/// <summary>
		/// The skip aspects
		/// </summary>
		public List<AspectEntry> skipAspects;

		/// <summary>
		/// The graphic for this stage 
		/// </summary>
		public List<MutationGraphicsData> graphics;

		/// <summary>
		/// Overrides to allow changing values of mutation verbs.
		/// </summary>
		[CanBeNull]
		public List<VerbToolOverride> verbOverrides;

		/// <summary>
		/// Any abilities added by the stage
		/// </summary>
		[CanBeNull]
		public List<Abilities.MutationAbilityDef> abilities;

		/// <summary>
		/// Gets the skip aspects.
		/// </summary>
		/// <value>
		/// The skip aspects.
		/// </value>
		[NotNull]
		public IReadOnlyList<AspectEntry> SkipAspects => ((IReadOnlyList<AspectEntry>)skipAspects) ?? Array.Empty<AspectEntry>();


		/// <summary>
		/// Indicates whether there is a reason to run vanilla hediff base logic or not.
		/// </summary>
		public bool RunBaseLogic = false;


		string IDescriptiveStage.DescriptionOverride => description;
		string IDescriptiveStage.LabelOverride => labelOverride;

		/// <summary>called when the given hediff enters this stage</summary>
		/// <param name="hediff">The hediff.</param>
		public void EnteredStage(Hediff hediff)
		{
			if (aspectGivers != null)
			{
				foreach (AspectGiver aspectGiver in aspectGivers)
				{
					aspectGiver.TryGiveAspects(hediff.pawn);
				}
			}

			ApplyVerbOverrides(hediff);

			if (memory != null)
			{
				hediff.pawn.TryAddMutationThought(memory);
			}

			RunBaseLogic = ShouldRunBaseLogic() ? true : RunBaseLogic;
		}

		/// <summary>
		/// Called once when the hediff stage is first loaded, for any one-time initialization
		/// </summary>
		/// <param name="hediff"></param>
		public void OnLoad(Hediff hediff)
		{
			ApplyVerbOverrides(hediff);
			RunBaseLogic = ShouldRunBaseLogic() ? true : RunBaseLogic;
		}

		/// <summary>
		/// Called during initialization when deciding if rimworld hediff base logic should be executed on tick.
		/// </summary>
		protected virtual bool ShouldRunBaseLogic()
		{
			if (hediffGivers != null && hediffGivers.Count > 0)
				return true;

			if (mentalStateGivers != null && mentalStateGivers.Count > 0)
				return true;

			if (mentalBreakMtbDays > 0)
				return true;

			if (vomitMtbDays > 0)
				return true;

			if (forgetMemoryThoughtMtbDays > 0)
				return true;

			if (destroyPart)
				return true;

			if (deathMtbDays > 0)
				return true;

			return false;
		}


		private void ApplyVerbOverrides(Hediff hediff)
		{

			var verbGiver = hediff.TryGetComp<HediffComp_VerbGiver>();
			if (verbGiver != null && verbOverrides != null)
			{
				foreach (Tool tool in verbGiver.Tools)
				{
					foreach (VerbToolOverride toolOverride in verbOverrides)
					{
						if (tool.label == toolOverride.label)
						{
							if (toolOverride.power.HasValue)
								tool.power = toolOverride.power.Value;

							if (toolOverride.cooldownTime.HasValue)
								tool.cooldownTime = toolOverride.cooldownTime.Value;

							if (toolOverride.chanceFactor.HasValue)
								tool.chanceFactor = toolOverride.chanceFactor.Value;

							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public class VerbToolOverride
		{
			/// <summary>
			/// The label of the verb to override.
			/// </summary>
			public string label;

			/// <summary>
			/// Value to set verb power to.
			/// </summary>
			public float? power;

			/// <summary>
			/// Value to set verb cooldown time to.
			/// </summary>
			public float? cooldownTime;

			/// <summary>
			/// Value to set verb chance factor to. Verb chance is multiplied by this value. Default is 1.
			/// </summary>
			public float? chanceFactor;
		}

		/// <summary>
		/// 
		/// </summary>
		public class AspectEntry
		{
			/// <summary>
			/// The aspect the pawn must have 
			/// </summary>
			public AspectDef aspect;
			/// <summary>
			/// The stage the aspect must be in to satisfy this entry, if null any stage will do 
			/// </summary>
			public int? stage;

			/// <summary>
			/// checks if the given pawn satisfies this entry 
			/// </summary>
			/// <param name="pawn">The pawn.</param>
			/// <returns></returns>
			/// <exception cref="ArgumentNullException">pawn</exception>
			public bool Satisfied([NotNull] Pawn pawn)
			{
				if (pawn == null) throw new ArgumentNullException(nameof(pawn));
				var asTracker = pawn.GetAspectTracker();
				var pAspect = asTracker?.GetAspect(this.aspect);
				if (pAspect == null) return false;
				return stage == null || stage == pAspect.StageIndex;
			}
		}
	}
}