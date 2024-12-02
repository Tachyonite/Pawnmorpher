using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hediffs.Composable;
using Pawnmorph.Hediffs.Utility;
using Pawnmorph.Interfaces;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// Abstract base class for all hediffs that cause mutations and transformation
	/// 
	/// </summary>
	/// <seealso cref="Verse.Hediff" />
	/// <seealso cref="Pawnmorph.Hediffs.Hediff_Descriptive" />
	public class Hediff_MutagenicBase : Hediff_StageChanges, IMutagenicHediff, ICaused
	{
		// Used to track what kind of stage we're in, so we don't have to check
		// every tick
		enum StageType
		{
			None,
			Mutation,
			Transformation
		}


		[Unsaved] private StageType cachedStageType;

		// The number of queued up mutations to add over the next few ticks
		private int queuedMutations;

		// A utility class to handle iterating over both body parts and mutations
		private BodyMutationManager bodyMutationManager = new BodyMutationManager();

		// Used to force-remove the hediff
		private bool forceRemove;


		[NotNull] private MutationCauses _causes;

		// Sensitivity stats of the pawn.  Fetched only intermittently because they're expensive to calculate.
		[Unsaved][NotNull] private readonly Cached<float> mutagenSensitivity;
		[Unsaved][NotNull] private readonly Cached<float> transformationSensitivity;
		[Unsaved][NotNull] private readonly Cached<float> _painStatValue;
		[Unsaved][NotNull] private readonly Cached<MutagenDef> _bestMutagenCause;


		// The list of observer comps
		[Unsaved] private readonly Lazy<List<ITfHediffObserverComp>> observerComps;


		/// <summary>
		///     Gets the mutagen sensitivity sensitivity of the pawn
		/// </summary>
		/// <value>The mutagen sensitivity.</value>
		public virtual float MutagenSensitivity => mutagenSensitivity.Value;

		/// <summary>
		/// Gets the causes of this hediff 
		/// </summary>
		/// <value>
		/// The causes.
		/// </value>
		[NotNull]
		public MutationCauses Causes => _causes;


		/// <summary>
		/// Gets the transformation sensitivity of the pawn.
		/// </summary>
		/// <value>The transformation sensitivity.</value>
		public virtual float TransformationSensitivity => transformationSensitivity.Value;

		/// <summary>
		/// Gets the observer comps.
		/// </summary>
		/// <value>The observer comps.</value>
		[NotNull]
		public IEnumerable<ITfHediffObserverComp> ObserverComps => observerComps?.Value ?? Enumerable.Empty<ITfHediffObserverComp>();

		/// <summary>
		/// Whether or not this hediff is currently blocking race checks
		/// </summary>
		/// <value><c>true</c> if blocks race check; otherwise, <c>false</c>.</value>
		public bool BlocksRaceCheck => CurrentStageHasMutations;

		/// <summary>
		/// Gets a value indicating whether there are any mutations in the current stage.
		/// </summary>
		/// <value>
		///   <c>true</c> if there are any mutations in the current stage; otherwise, <c>false</c>.
		/// </value>
		public bool CurrentStageHasMutations => cachedStageType == StageType.Mutation;

		/// <summary>
		/// Gets a value indicating whether there are any transformations in the current stage.
		/// </summary>
		/// <value>
		///   <c>true</c> if there are any transformations in the current stage; otherwise, <c>false</c>.
		/// </value>
		public bool CurrentStageHasTransformation => cachedStageType == StageType.Transformation;

		/// <summary>
		/// Controls whether or not this hediff gets removed 
		/// </summary>
		public override bool ShouldRemove => forceRemove || base.ShouldRemove || mutagenSensitivity.Value <= 0;

		/// <summary>
		/// Gets the pain offset for this hediff 
		/// </summary>
		/// <value>
		/// The pain offset.
		/// </value>
		public override float PainOffset => base.PainOffset * _painStatValue.Value; //using pain offset instead of factor as the pain stat should affect the pain from only this hediff not the pain from all hediffs  



		/// <summary>
		/// Initializes a new instance of the <see cref="T:Pawnmorph.Hediffs.Hediff_MutagenicBase"/> class.
		/// </summary>
		public Hediff_MutagenicBase()
		{
			mutagenSensitivity = new Cached<float>(() => pawn.GetStatValue(PMStatDefOf.MutagenSensitivity));
			transformationSensitivity = new Cached<float>(() => pawn.GetStatValue(PMStatDefOf.TransformationSensitivity));
			_painStatValue = new Cached<float>(() => pawn.GetStatValue(PMStatDefOf.PM_MutagenPainSensitivity), 1);
			observerComps = new Lazy<List<ITfHediffObserverComp>>(() => comps.MakeSafe().OfType<ITfHediffObserverComp>().ToList());
			_causes = new MutationCauses();
			_bestMutagenCause = new Cached<MutagenDef>(GetBestMutagenCause);

		}



		/// <summary>
		/// Gets the best mutagen cause.
		/// </summary>
		/// <returns></returns>
		MutagenDef GetBestMutagenCause()
		{
			return _causes.GetAllCauses<MutagenDef>().FirstOrDefault()?.causeDef;
		}


		/// <summary>
		///     Called after this hediff is added to the pawn
		/// </summary>
		/// <param name="dinfo">The damage info.</param>
		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);

			// If we somehow got a pawn that can't be mutated just remove the hediff
			// This is because AndroidTiers was giving android mutations because reasons
			if (!def.GetMutagenDef().CanInfect(pawn))
				MarkForRemoval();
			ThingDef weaponSource = dinfo?.Weapon;

			ResetMutationList();
			ResetSpreadList();

			if (weaponSource != null && _causes.Contains(MutationCauses.WEAPON_PREFIX) == false)
			{
				MutagenDef mutSource = weaponSource.GetModExtension<MutagenExtension>()?.mutagen;
				if (mutSource != null)
					_causes.TryAddMutagenCause(mutSource);

				_causes.Add(MutationCauses.WEAPON_PREFIX, weaponSource);
			}

			_causes.SetLocation(pawn);
			if (def.stages != null && def.stages.Count > 0)
				CheckCurrentStage(null, def.stages[base.CurStageIndex]);
		}

		/// <summary>
		/// Called when afte the hediff is removed.
		/// </summary>
		public override void PostRemoved()
		{
			base.PostRemoved();
		}

		/// <summary>
		/// Ticks this instance.
		/// </summary>
		public override void Tick()
		{
			base.Tick();

			if (pawn.IsHashIntervalTick(60))
			{
				mutagenSensitivity.Recalculate();
				_painStatValue.Recalculate();
				if (cachedStageType == StageType.Mutation && !this.IsImmune())
					CheckAndAddMutations();
			}
		}

		/// <summary>
		/// Clears the caches in this instance 
		/// </summary>
		public void ClearCaches()
		{
			_painStatValue.Recalculate();
			mutagenSensitivity.Recalculate();
			_bestMutagenCause.Recalculate();

			cachedStageType = StageType.None;
			if (def.stages != null)
			{
				var cStage = def.stages[base.CurStageIndex];

				CheckCurrentStage(null, cStage);
			}
		}

		/// <summary>
		/// Checks if we should add mutations, and if so does
		/// Mutations are queued up and added one at a time to smooth out mutation rate when there are
		/// large spikes (e.g. severity-based MutationRates)
		/// </summary>
		protected virtual void CheckAndAddMutations()
		{
			if (!(CurStage is HediffStage_Mutation stage))
			{
				Log.Error($"Hediff {def.defName} tried to mutate {pawn.Name} but stage {CurStageIndex} ({CurStage.label}) is not a mutation stage");
				return;
			}

			// MutationRates can request multiple muations be added at once,
			// but we'll queue them up so they only happen once a second
			if (stage.mutationRate != null)
				QueueUpMutations(stage.mutationRate.GetMutationsPerSecond(this));
			else
			{
				Log.ErrorOnce($"{def.defName} has stage {CurStageIndex} with no mutation rate!", GetHashCode());
			}

			// Add a queued mutation, if any are waiting
			if (queuedMutations > 0)
			{
				var result = TryMutate();
				queuedMutations--;
			}
		}

		/// <summary>
		/// Gets the correct mutagen to use for this instance, this should take into account things like the weapon that caused the hediff if present 
		/// </summary>
		/// <returns></returns>
		[NotNull]
		protected virtual MutagenDef GetMutagen()
		{
			return _bestMutagenCause.Value ?? def?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;
		}

		/// <summary>
		/// Tries to apply the current mutation to the current body part.
		/// If it succeeds, or the mutation is non-blocking, advances the list of
		/// mutations. If all mutations have been applied, advanceds the list of
		/// body parts and resets the mutation list.
		/// </summary>
		/// <returns>A mutation result describing the mutation(s) added, if any</returns>
		protected MutationResult TryMutate()
		{
			do
			{
				var bodyPart = bodyMutationManager.BodyPart;
				if (bodyPart != null)
				{
					if (!pawn.RaceProps.body.AllParts.Contains(bodyPart))
					{
						// If the pawn's race changes the mutation order may no longer be valid 
						// Reset it and try again later
						ResetSpreadList(); 
						return MutationResult.Empty;
					}
				}
				else
				{
					// If mutation manager does not have a body part, select the first random body part from the next mutation that the pawn actually has.
					List<BodyPartDef> mutationParts = bodyMutationManager.Mutation.mutation.parts;
					for (int x = mutationParts.Count - 1; x >= 0; x--)
					{
						List<BodyPartRecord> parts = pawn.RaceProps.body.GetPartsWithDef(mutationParts[x]);
						if (parts.Count == 0)
							break;

						bodyPart = parts.RandomElement();
						if (bodyPart != null)
							break;
					}

					if (bodyPart == null)
					{
						bodyMutationManager.NextMutation();
						return MutationResult.Empty;
					}
				}

				// Notify the observers first, since they may add/remove/change mutations
				foreach (var observer in ObserverComps)
					observer.Observe(bodyPart);

				// Skip this body part if it has no mutations
				if (!bodyMutationManager.HasMutations())
					continue;

				if (this.CurStage is HediffStage_Mutation mutationStage)
				{
					// If completely random provide a small chance to skip a limb
					if (mutationStage.mutationTypes is MutTypes_All)
					{
						// Only provide a chance to completely skip if there is 25% or less chance to get identical mutations.
						if (bodyMutationManager.AvailableMutations <= 4)
						{
							float skipChance = 1F / (bodyMutationManager.AvailableMutations + 1);
							if (Rand.Chance(skipChance))
							{
#if DEBUG
								Log.Message($"Skipped mutating limb {bodyMutationManager.BodyPart.Label} for {pawn.Name} - {bodyMutationManager.AvailableMutations} mutations available. ({skipChance * 100}% chance)");
#endif
								continue;
							}
						}
					}
				}

				// Check all mutations in order until we add one
				do
				{
					var mutation = bodyMutationManager.Mutation;

					// Check if the mutation can actually be added 
					if (!mutation.mutation.CanApplyMutations(pawn, bodyPart))
						continue;

					// Add the mutation (and aspects) if we succeed in the random chance
					if (Rand.Value < mutation.addChance)
					{
						MutagenDef mutagen = GetMutagen();
						MutationResult mutationResult = mutagen.AddMutationAndAspects(pawn, mutation.mutation, bodyPart, this);

						foreach (Hediff_AddedMutation res in mutationResult) //make sure the mutation knows where it came from 
						{                                                   //should this be a part of AddMutationAndAspects? so many overloads already, need a good solution 
							res.sourceHediffDef = def;
							res.Causes.Add(_causes);
							res.Causes.Add(MutationCauses.HEDIFF_PREFIX, def);

							if (Causes.Location.HasValue)
								res.Causes.SetLocation(Causes.Location.Value);
						}

						// Notify the observers of any added mutations
						foreach (var observer in ObserverComps)
							foreach (var added in mutationResult)
								observer.MutationAdded(added);

						// Move to the next mutation for next time
						bodyMutationManager.NextMutation();

						return mutationResult;
					}

					// If the mutation blocks, bail now so we can try to add it again next time.
					if (mutation.blocks)
						return MutationResult.Empty;
				} while (bodyMutationManager.NextMutation());
			} while (bodyMutationManager.NextBodyPart());

			// If we iterate through the entire body and mutation list, we're done
			return MutationResult.Empty;
		}

		/// <summary>
		/// Checks if we should transform the pawn, and if so does
		/// </summary>
		protected virtual void CheckAndDoTransformation()
		{
			if (!(CurStage is HediffStage_Transformation stage))
			{
				Log.Error($"Hediff {def.defName} tried to transform {pawn.Name} but stage {CurStageIndex} ({CurStage.label}) is not a transformation stage");
				return;
			}

			if (stage.tfChance.ShouldTransform(this))
			{
				TryTransform();
			}
		}

		/// <summary>
		/// Triggers transformation.
		/// </summary>
		public virtual void TryTransform()
		{
			if (CurStage is HediffStage_Transformation stage)
			{
				PawnKindDef pawnKind = stage.tfTypes.GetTF(this);
				TFGender gender = stage.tfGenderSelector.GetGender(this);

				TryTransformPawn(pawnKind, gender, stage.tfSettings);
			}
		}

		/// <summary>
		/// Attempts to transform the pawn.
		/// </summary>
		/// <param name="pawnKind">The pawnkind to transform the pawn into</param>
		/// <param name="gender">The gender of the post-transformation pawn</param>
		/// <param name="tfSettings">additional miscellaneous transformation settings</param>
		/// <returns>
		///   <c>true</c> if the transformation succeeded, <c>false</c> otherwise.
		/// </returns>
		/// <exception cref="ArgumentNullException">tfSettings</exception>
		protected bool TryTransformPawn(PawnKindDef pawnKind, TFGender gender, [NotNull] TFMiscSettings tfSettings)
		{
			if (tfSettings == null) throw new ArgumentNullException(nameof(tfSettings));

			var mutagen = def.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;
			var request = new TransformationRequest(pawnKind, pawn)
			{
				forcedGender = gender,
				forcedGenderChance = 100f,
				cause = this,
				tale = tfSettings.tfTale,
				manhunterSettingsOverride = tfSettings.manhunterSettings,
				forcedSapienceLevel = tfSettings.forcedSapience,
				transformedTick = Find.TickManager?.TicksAbs
			};

			var transformedPawn = mutagen.MutagenCached.Transform(request);

			if (transformedPawn != null)
			{
				var comp = Find.World.GetComponent<PawnmorphGameComp>();
				comp.AddTransformedPawn(transformedPawn);
				//TODO callbacks
				return true;
			}

			return false;
		}


		/// <summary>
		/// Updates the cached stage values
		/// </summary>
		protected override void OnStageChanged(HediffStage oldStage, HediffStage newStage)
		{
			if (oldStage == null) throw new ArgumentNullException(nameof(oldStage));
			if (newStage == null) throw new ArgumentNullException(nameof(newStage));


			if (newStage is HediffStage_MutagenicBase mBase) mBase.alert?.SendAlert(this);

			CheckCurrentStage(oldStage, newStage, true);

			foreach (var comp in ObserverComps)
				comp.StageChanged();

		}

		private void CheckCurrentStage([CanBeNull] HediffStage oldStage, [NotNull] HediffStage currentStage, bool doTf = false)
		{
			if (currentStage is HediffStage_Mutation newMutStage)
			{
				cachedStageType = StageType.Mutation;

				// Reset the spread manager and mutation cache, but only if the
				// ones in the new stage are different
				if (oldStage is HediffStage_Mutation oldMutStage)
				{
					if (newMutStage.spreadOrder == null || !newMutStage.spreadOrder.EquivalentTo(oldMutStage.spreadOrder))
						ResetSpreadList();
					if (newMutStage.mutationTypes == null || !newMutStage.mutationTypes.EquivalentTo(oldMutStage.mutationTypes))
						ResetMutationList();
				}
				else
				{
					ResetSpreadList();
					ResetMutationList();
				}
			}
			else if (currentStage is HediffStage_Transformation)
			{
				cachedStageType = StageType.Transformation;

				// Only try to transform the pawn when entering a transformation stage
				// NOTE: This triggers regardless of whether the stages are increasing or decreasing.
				if (!this.IsImmune() && doTf)
					CheckAndDoTransformation();
			}
			else
			{
				cachedStageType = StageType.None;
			}
		}



		/// <summary>
		/// Queues up a number of mutations to be added to the pawn.  Negative amounts
		/// can cancel out queued up mutations but won't remove already-existing mutations.
		/// </summary>
		/// <param name="mutations">Mutations.</param>
		protected void QueueUpMutations(int mutations)
		{
			queuedMutations += mutations;
			// Negative mutation counts can cancel already-queued mutations but should never go below 0
			queuedMutations = Math.Max(queuedMutations, 0);
		}

		/// <summary>
		/// Resets the spread list because something caused the current one to be invalid.
		/// Call this when SpreadOrder changes (usually due to a stage change).
		/// </summary>
		protected void ResetSpreadList()
		{
			if (CurStage is HediffStage_Mutation mutStage)
			{
				var spreadList = mutStage.spreadOrder?.GetSpreadList(this);
				if (spreadList == null)
					return;

				bodyMutationManager.ResetSpreadList(spreadList);

				// Let the observers know we've reset our spreading
				foreach (var comp in ObserverComps)
					comp.Init();
			}
		}

		/// <summary>
		/// Resets the mutation list because something caused the current one to be invalid.
		/// Call this when MutationTypes changes, or something it relies on does.
		/// </summary>
		protected void ResetMutationList()
		{
			if (CurStage is HediffStage_Mutation mutStage)
			{
				var mutations = mutStage.mutationTypes?.GetMutations(this);
				if (mutations == null) return;
				bodyMutationManager.ResetMutationList(mutations);

				// Let the observers know we've reset our mutation types
				foreach (var comp in ObserverComps)
					comp.Init();
			}
		}

		/// <summary>
		/// The severity of this hediff 
		/// </summary>
		/// <value>The severity.</value>
		public override float Severity
		{
			get => base.Severity;
			set
			{
				// Severity changes can potentially queue up mutations
				// Note that CurStage can be null if this hediff does not have stages, but the 'is' will return false in that case
				if (CurStage is HediffStage_Mutation mutStage)
				{
					float diff = value - base.Severity;
					int mutations = mutStage.mutationRate?.GetMutationsPerSeverity(this, diff) ?? 0;
					QueueUpMutations(mutations);
				}
				base.Severity = value;
			}
		}

		/// <summary>
		/// Controls the severity label that gets rendered in the health menu
		/// </summary>
		/// <value>The severity label.</value>
		public override string SeverityLabel
		{
			get
			{
				if (base.SeverityLabel != null)
					return base.SeverityLabel;

				// Render based on max severity if there's no lethal severity, since
				// many mutagenic hediffs cause a full TF on max severity
				if (def.maxSeverity > 0f)
					return (Severity / def.maxSeverity).ToStringPercent();

				return null;
			}
		}

		/// <summary>
		/// Marks this hediff for removal.
		/// 
		/// This is needed because Rimworld is touchy about removing hediffs. Rather than doing
		/// it manually, you should call this instead. The HediffTracker will safely remove this
		/// hediff at the beginning of the next tick.
		/// </summary>
		public void MarkForRemoval()
		{
			forceRemove = true;
		}



		/// <summary>
		/// Exposes data to be saved/loaded from XML upon saving the game
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref forceRemove, nameof(forceRemove));
			Scribe_Values.Look(ref queuedMutations, nameof(queuedMutations));
			Scribe_Deep.Look(ref bodyMutationManager, nameof(bodyMutationManager));
			Scribe_Deep.Look(ref _causes, "causes");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				_causes = _causes ?? new MutationCauses();
				ClearCaches();
			}
		}

		/// <summary>
		/// Creates a debug string for this hediff 
		/// </summary>
		/// <returns></returns>
		public override string DebugString()
		{
			StringBuilder builder = new StringBuilder(base.DebugString());
			builder.AppendLine($"{nameof(Hediff_MutagenicBase)}:cached stage type {cachedStageType}");



			if (CurStage is HediffStage_Mutation mutationStage)
			{
				builder.AppendLine("  Mutation Stage");
				builder.AppendLine("  MutagenSensitivity: " + MutagenSensitivity.ToStringPercent());
				builder.Append(bodyMutationManager.DebugString());
				builder.Append(mutationStage.DebugString(this));
			}
			else if (CurStage is HediffStage_Transformation transformationStage)
			{
				builder.AppendLine("  Transformation Stage");
				builder.AppendLine("  TransformationSensitivity: " + TransformationSensitivity);
				builder.Append(transformationStage.DebugString(this));
			}
			else
			{
				builder.AppendLine("  Other Stage");
			}

			if (queuedMutations > 0)
			{
				builder.AppendLine($"queued mutations:{queuedMutations}");
			}

			return builder.ToString();
		}
	}
}