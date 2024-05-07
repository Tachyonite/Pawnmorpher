using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.DefOfs;
using Pawnmorph.GraphicSys;
using Pawnmorph.Hediffs;
using Pawnmorph.Interfaces;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	///     hediff representing a mutation
	/// </summary>
	/// <seealso cref="Verse.HediffWithComps" />
	public class Hediff_AddedMutation : Hediff_StageChanges, ICaused
	{
		private List<Abilities.MutationAbility> abilities = new List<Abilities.MutationAbility>();
		private int _shouldRemoveAgeTicks;
		
		/// <summary>
		///     The mutation description
		/// </summary>
		public string mutationDescription;

		/// <summary>
		/// The severity adjust component. Null if pawn has none.
		/// </summary>
		public Comp_MutationSeverityAdjust SeverityAdjust;

		/// <summary>
		/// The spreading mutation component. Null if pawn has none.
		/// </summary>
		public SpreadingMutationComp SpreadingMutation;

		/// <summary>
		///     if this part should be removed or not
		/// </summary>
		protected bool shouldRemove;

		private MutationDef _mDef;

		[NotNull] private MutationCauses _causes = new MutationCauses();


		private bool _waitingForUpdate;
		private bool _tickComponents = true;

		/// <summary>
		/// Constructor
		/// </summary>
		public Hediff_AddedMutation()
		{
		}

		/// <inheritdoc/>
		public override void PostMake()
		{
			base.PostMake();
		}

		/// <summary>
		///     Gets the definition.
		/// </summary>
		/// <value>
		///     The definition.
		/// </value>
		[NotNull]
		public MutationDef Def
		{
			get
			{
				if (_mDef == null)
					try
					{
						_mDef = (MutationDef)def;
					}
					catch (InvalidCastException e)
					{
						Log.Error($"cannot convert {def.GetType().Name} to {nameof(MutationDef)}!\n{e}");
					}

				return _mDef;
			}
		}

		private void Initialize()
		{
			TickBase = Def.RunBaseLogic || CurrentMutationStage?.RunBaseLogic == true;
			//TODO This should be refactored eventually. Possibly by moving Severity Adjust over to CompBase and then adding a "Requires Ticking" property.
			_tickComponents = comps.Any( x => x is SpreadingMutationComp == false 
										&& x is Comp_MutationSeverityAdjust == false
										&& x is RemoveFromPartComp == false
										&& x is Comp_MutationDependency == false);
			SeverityAdjust = this.TryGetComp<Comp_MutationSeverityAdjust>();
			SpreadingMutation = this.TryGetComp<SpreadingMutationComp>();

			if (pawn.Destroyed == false && pawn.Discarded == false)
				PawnmorpherMod.WorldComp.RegisterMutation(this);

#if DEBUG
			if (_tickComponents)
				Log.Warning($"Ticking comps on {def.defName} for {pawn.Name}: " + string.Join(", ", comps.Select(x => x.GetType().Name)));
#endif
		}

		/// <summary>
		///     Gets the current mutation stage. null if the hediff has no stages or the current stage is not a mutation stage
		/// </summary>
		/// <value>
		///     The current mutation stage.
		/// </value>
		[CanBeNull]
		public MutationStage CurrentMutationStage
		{
			get
			{
				if (Def.stages.NullOrEmpty()) return null;
				return Def.CachedMutationStages[CurStageIndex];
			}
		}

		/// <summary>
		///     Gets the influence this mutation confers
		/// </summary>
		/// <value>
		///     The influence.
		/// </value>
		[NotNull]
		public List<AnimalClassBase> Influence
		{
			get
			{
				if (def is MutationDef mDef)
					return mDef.ClassInfluences;

				Log.Warning($"{def.defName} is a mutation but does not use {nameof(MutationDef)}! this will cause problems!");
				return new List<AnimalClassBase>() { AnimalClassDefOf.Animal };
			}
		}


		/// <summary>
		///     Gets the base label .
		/// </summary>
		/// <value>
		///     The base label .
		/// </value>
		public override string LabelBase
		{
			get
			{
				var label = base.LabelBase;

				if (SeverityAdjust?.Halted == true) label += " (halted)";

				return label;
			}
		}

		/// <summary>
		/// Mutations are always visible, so don't spent time checking comps.
		/// </summary>
		public override bool Visible => true;

		public override string LabelInBrackets
		{
			get
			{
				HediffStage stage = CurStage;
				if (stage != null)
				{
					if (PawnmorpherMod.Settings.enableMutationAdaptedStageLabel == false)
					{
						if (stage.minSeverity == 1)
							return string.Empty;
					}
				}

				return base.LabelInBrackets;
			}
		}

		/// <inheritdoc />
		public override string DebugString()
		{
			string debugString = base.DebugString();

			HediffComp_Production productionComp = this.TryGetComp<HediffComp_Production>();
			if (productionComp != null)
			{
				debugString += Environment.NewLine;
				debugString += "Production Component: " + Environment.NewLine;
				debugString += productionComp.ToStringFull();
			}

			return debugString;
		}

		/// <summary>
		///     Gets the causes of this mutation
		/// </summary>
		/// <value>
		///     The causes.
		/// </value>
		[NotNull]
		public MutationCauses Causes => _causes;


		/// <summary>
		///     Gets a value indicating whether should be removed.
		/// </summary>
		/// <value><c>true</c> if should be removed; otherwise, <c>false</c>.</value>
		public override bool ShouldRemove => shouldRemove;

		/// <summary>Gets the extra tip string .</summary>
		/// <value>The extra tip string .</value>
		public override string TipStringExtra
		{
			get
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.Append(base.TipStringExtra);
				stringBuilder.AppendLine("Efficiency".Translate() + ": " + def.addedPartProps.partEfficiency.ToStringPercent());
				return stringBuilder.ToString();
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance is a core mutation.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is a core mutation; otherwise, <c>false</c>.
		/// </value>
		public bool IsCoreMutation => this.TryGetComp<RemoveFromPartComp>()?.Layer == MutationLayer.Core;

		/// <summary>
		///     Gets or sets a value indicating whether progression is halted or not.
		/// </summary>
		/// <value>
		///     <c>true</c> if progression halted; otherwise, <c>false</c>.
		/// </value>
		public bool ProgressionHalted => SeverityAdjust?.Halted == true;

		/// <summary>
		///     called every tick
		/// </summary>
		public override void Tick()
		{
			base.Tick();

			// Use a for loop here because this is a hot path and creating enumerators is causing too much overhead
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = abilities.Count - 1; i >= 0; i--)
				abilities[i].Tick();

			if (shouldRemove == false && ++_shouldRemoveAgeTicks > 120)
			{
				_shouldRemoveAgeTicks = 0;
				if (comps != null)
				{
					for (int i = comps.Count - 1; i >= 0; i--)
					{
						if (comps[i].CompShouldRemove)
						{
							shouldRemove = true;
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Called when the hediff stage changes.
		/// </summary>
		protected override void OnStageChanged(HediffStage oldStage, HediffStage newStage)
		{
			if (newStage is MutationStage mStage)
			{
				// We don't normally use any of the vanilla functionality so there is no reason to propagate the tick further down
				// unless the hediff specifically requests it
				TickBase = Def.RunBaseLogic || mStage.RunBaseLogic;

				GenerateAbilities(mStage);

				//check for aspect skips 
				if (mStage.SkipAspects.Any(e => e.Satisfied(pawn)))
				{
					SkipStage();
					return;
				}
			}

			if (newStage is IExecutableStage exeStage)
				exeStage.EnteredStage(this);

			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}


		/// <inheritdoc />
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos()) yield return gizmo;

			if (pawn.IsColonistPlayerControlled)
				foreach (Abilities.MutationAbility item in abilities)
					yield return item.Gizmo;
		}

		private void GenerateAbilities(HediffStage stage)
		{
			if (stage is MutationStage mutationStage)
			{
				abilities = new List<Abilities.MutationAbility>();
				if (mutationStage.abilities == null || mutationStage.abilities.Count == 0)
					return;

				//Abilities.MutationAbility ability;
				foreach (Abilities.MutationAbilityDef abilityDef in mutationStage.abilities)
				{
					if (abilityDef.abilityClass.BaseType == typeof(Abilities.MutationAbility))
					{
						Abilities.MutationAbility ability = (Abilities.MutationAbility)Activator.CreateInstance(abilityDef.abilityClass, abilityDef);
						abilities.Add(ability);
						ability.Initialize(pawn);
					}
				}
			}
		}

		/// <summary>
		///     checks if this mutation blocks the addition of a new mutation at the given part
		/// </summary>
		/// <param name="otherMutation">The other mutation.</param>
		/// <param name="addPart">The add part.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">otherMutation</exception>
		public virtual bool Blocks([NotNull] MutationDef otherMutation, [CanBeNull] BodyPartRecord addPart)
		{
			if (otherMutation == null) throw new ArgumentNullException(nameof(otherMutation));
			var mDef = def as MutationDef;
			return mDef?.BlocksMutation(otherMutation, Part, addPart) == true;
		}

		/// <summary>Exposes the data.</summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref shouldRemove, nameof(shouldRemove));
			Scribe_Collections.Look(ref abilities, nameof(abilities));

			Scribe_Deep.Look(ref _causes, "causes");
			if (Scribe.mode == LoadSaveMode.PostLoadInit && Part == null)
			{
				Log.Error($"Hediff_AddedPart [{def.defName},{Label}] has null part after loading.");
				pawn.health.hediffSet.hediffs.Remove(this);
				return;
			}

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				// Null if not previously saved.
				if (abilities == null)
					abilities = new List<Abilities.MutationAbility>();

				GenerateAbilities(base.CurStage);

				MutationStage stage = CurrentMutationStage;
				if (stage != null)
					stage.OnLoad(this);


				Initialize();
			}

		}

		/// <summary>
		///     Marks this mutation for removal.
		/// </summary>
		public void MarkForRemoval()
		{
			shouldRemove = true;
		}

		/// <summary>called after this instance is added to the pawn.</summary>
		/// <param name="dinfo">The dinfo.</param>
		public override void PostAdd(DamageInfo? dinfo)
		// After the hediff has been applied.
		{
			base.PostAdd(dinfo); // Do the inherited method.
			Initialize();
			if (PawnGenerator.IsBeingGenerated(pawn) || !pawn.Spawned
			) //if the pawn is still being generated do not update graphics until it's done 
			{
				_waitingForUpdate = true;
				return;
			}

			UpdatePawnInfo();

			foreach (Hediff_AddedMutation otherMutation in pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>())
				try
				{
					if (Blocks((MutationDef)otherMutation.def, otherMutation.Part))
						otherMutation.shouldRemove = true; //don't actually remove the hediffs, just mark them for removal
				}
				catch (InvalidCastException e) //just pretty up the error message a bit and continue on 
				{
					Log.Error($"could not cast {otherMutation.def.defName} of type {otherMutation.def.GetType().Name} to {nameof(MutationDef)}!\n{e}");
				}

			ApplyVisualAdjustment();

		}

		/// <summary>
		/// Applies the visual adjustments caused by this mutation.
		/// </summary>
		public void ApplyVisualAdjustment()
		{
			if (Def.RemoveComp?.layer == MutationLayer.Core)
			{
				if (Part.def == PM_BodyPartDefOf.Head && pawn.story.hairDef != PMStyleDefOf.PM_HairHidden)
				{
					// Hide hair
					var initialGraphics = pawn.GetComp<InitialGraphicsComp>();
					if (initialGraphics != null && pawn.def == ThingDefOf.Human)
						initialGraphics.HairDef = pawn.story.hairDef;
					pawn.story.hairDef = PMStyleDefOf.PM_HairHidden;
				}
				else if (Part.def == PM_BodyPartDefOf.Jaw && pawn.style.CanWantBeard && pawn.style.beardDef != PMStyleDefOf.PM_BeardHidden)
				{
					// Hide beard
					var initialGraphics = pawn.GetComp<InitialGraphicsComp>();
					if (initialGraphics != null && pawn.def == ThingDefOf.Human)
						initialGraphics.BeardDef = pawn.style.beardDef;
					pawn.style.beardDef = PMStyleDefOf.PM_BeardHidden;
				}
			}
		}

		/// <summary>called after this instance is removed from the pawn</summary>
		public override void PostRemoved()
		{
			base.PostRemoved();

			if (!PawnGenerator.IsBeingGenerated(pawn))
				pawn.GetMutationTracker()?.NotifyMutationRemoved(this);

			// Don't change style if mutation is skin
			if (Def.RemoveComp?.layer == MutationLayer.Core)
			{
				// If no other mutation is blocking the same part then reset style.
				if (pawn.GetMutationTracker()?.AllMutations.Any(x => x.Part == Part && x.Def.RemoveComp?.layer == MutationLayer.Core) == false)
				{
					var initialGraphics = pawn.GetComp<InitialGraphicsComp>();
					if (initialGraphics != null)
					{
						if (Part.def == PM_BodyPartDefOf.Head)
						{
							// Revert hair
							pawn.story.hairDef = initialGraphics.HairDef;
						}
						else if (Part.def == PM_BodyPartDefOf.Jaw)
						{
							// Revert beard
							pawn.style.beardDef = initialGraphics.BeardDef;
						}
					}
				}
			}

			PawnmorpherMod.WorldComp.UnregisterMutation(this);
		}

		/// <summary>
		///     Called after Tick().  The base class ticks Comps here.
		/// </summary>
		public override void PostTick()
		{
			if (_tickComponents)
				base.PostTick();

			if (_waitingForUpdate)
			{
				UpdatePawnInfo();
				_waitingForUpdate = false;
			}
		}

		/// <summary>
		///     Restarts the adaption progression for this mutation if halted, does nothing if the part is fully adapted or not
		///     halted
		/// </summary>
		public void ResumeAdaption()
		{
			SeverityAdjust?.Restart();
		}

		private void SkipStage()
		{
			//make sure to skip in the correct direction 
			float severityAdj = SeverityAdjust?.ChangePerDay ?? 0;

			int nextIndex;

			if (severityAdj < 0) nextIndex = Mathf.Max(0, CurStageIndex - 1);
			else nextIndex = Mathf.Min(def.stages.Count - 1, CurStageIndex + 1);

			if (nextIndex == CurStageIndex) return; //don't skip as there's no stage to skip to 
			HediffStage nextStage = def.stages[nextIndex];

			Severity = nextStage.minSeverity;
		}

		private void UpdatePawnInfo()
		{
			pawn.GetMutationTracker()?.NotifyMutationAdded(this);
			var gUpdater = pawn.GetComp<GraphicsUpdaterComp>();
			if (gUpdater != null)
			{
				//try and defer graphics update to the next tick so we don't lag when adding a bunch of mutations at once 
				gUpdater.IsDirty = true;
				return;
			}

			if (Current.ProgramState == ProgramState.Playing
			 && MutationUtilities.AllMutationsWithGraphics.Contains(def)
			 && pawn.IsColonist)
			{
				pawn.Drawer.renderer.SetAllGraphicsDirty();
			}
		}
	}
}