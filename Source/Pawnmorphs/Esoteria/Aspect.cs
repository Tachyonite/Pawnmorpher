using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary> 
	/// Base class for all "mutation affinities". <br />
	/// Affinities are things that are more global than hediffs but more temporary than traits.
	/// </summary>
	public class Aspect : IExposable
	{
		private const string MENTAL_BREAK_TRANSLATION_LABEL = "MentalStateReason_Aspect";

		/// <summary>
		/// the def of this aspect 
		/// </summary>
		[NotNull]
		public AspectDef def;

		private int _stage = 0;
		private Pawn _pawn;
		private bool _shouldRemove;
		private bool _wasStarted;
		private Dictionary<SkillDef, float> _addedSkillsActualAmount;
		private Dictionary<SkillDef, Passion> _originalPassions;
		/// <summary>
		/// priority of this aspect, lower priorities should show up first in the Mutation ITab
		/// </summary>
		public int Priority => def?.priority ?? 0;
		private AspectTracker _tracker;


		/// <summary>
		/// Gets a value indicating whether this instance is bad.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is bad; otherwise, <c>false</c>.
		/// </value>
		public bool IsBad => CurrentStage.isBad;

		/// <summary>
		/// the color of the label in the mutation ITab 
		/// </summary>
		public Color LabelColor => CurrentStage.labelColor ?? def.labelColor;

		/// <summary>
		/// all cap mods that are active on this aspect (changes when the aspect's stage changes) 
		/// </summary>
		public IEnumerable<PawnCapacityModifier> CapMods => CurrentStage.capMods ?? Enumerable.Empty<PawnCapacityModifier>();

		/// <summary>
		/// if this aspect has any CapMods (may change when the aspects stage changes) 
		/// </summary>
		public bool HasCapMods => CurrentStage.capMods != null && CurrentStage.capMods.Count != 0;

		/// <summary> The current stage index. </summary>
		public int StageIndex
		{
			get => _stage == -1 ? 0 : _stage; //-1 can happen if the aspect has not been added yet 
			set
			{
				int st = Mathf.Clamp(value, 0, Stages.Count - 1);
				if (_stage != st)
				{
					int last = _stage;
					_stage = st;
					if (Scribe.mode == LoadSaveMode.Inactive) //make sure not to call this if we're loading from a save 
						StageChanged(last);
				}
			}
		}

		/// <summary> The current stage. </summary>
		[NotNull]
		public AspectStage CurrentStage => Stages[StageIndex];

		AspectTracker Tracker => _tracker ?? (_tracker = Pawn?.GetAspectTracker());


		void IExposable.ExposeData()
		{
			Scribe_References.Look(ref _pawn, nameof(Pawn));
			Scribe_Values.Look(ref _shouldRemove, nameof(ShouldRemove));
			Scribe_Defs.Look(ref def, nameof(def));
			Scribe_Values.Look(ref _stage, nameof(StageIndex));
			Scribe_Collections.Look(ref _addedSkillsActualAmount, nameof(_addedSkillsActualAmount), LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref _originalPassions, nameof(_originalPassions), LookMode.Def, LookMode.Value);
			ExposeData();
		}

		/// <summary>
		/// the label of this aspect 
		/// </summary>
		public string Label
		{
			get
			{
				string lBase = string.IsNullOrEmpty(CurrentStage.label) ? def.label : CurrentStage.label;
				if (!string.IsNullOrEmpty(CurrentStage.modifier)) lBase = $"{lBase} ({CurrentStage.modifier})";

				return lBase;
			}
		}

		/// <summary> The description of the aspect, taking into account it's current stage </summary>
		public string Description =>
			string.IsNullOrEmpty(CurrentStage.description)
				? string.IsNullOrEmpty(def.description) ? "NO DESCRIPTION " : def.description
				: CurrentStage.description;

		/// <summary> The pawn this is attached to. </summary>
		public Pawn Pawn => _pawn;

		/// <summary> If this affinity should be removed or not. </summary>
		public bool ShouldRemove
		{
			get => _shouldRemove;
			protected set => _shouldRemove = value;
		}


		/// <summary>
		/// Gets all thoughts nullified by this aspect.
		/// </summary>
		/// <value>
		/// The nullified thoughts.
		/// </value>
		[NotNull]
		public IEnumerable<ThoughtDef> NullifiedThoughts
		{
			get
			{
				foreach (ThoughtDef defNullifiedThought in def.nullifiedThoughts)
				{
					yield return defNullifiedThought;
				}

				foreach (ThoughtDef currentStageNullifiedThought in CurrentStage.nullifiedThoughts)
				{
					yield return currentStageNullifiedThought;
				}
			}
		}

		/// <summary>
		/// Determines whether the specified thought definition is nullified.
		/// </summary>
		/// <param name="thoughtDef">The thought definition.</param>
		/// <returns>
		///   <c>true</c> if the specified thought definition is nullified; otherwise, <c>false</c>.
		/// </returns>
		public bool IsNullified(ThoughtDef thoughtDef)
		{
			if (def.nullifiedThoughts.Contains(thoughtDef))
				return true;

			if (CurrentStage.nullifiedThoughts.Contains(thoughtDef))
				return true;

			return false;
		}


		/// <summary>
		/// list of all stages in this Aspect 
		/// </summary>
		[NotNull]
		protected List<AspectStage> Stages => def.stages;

		/// <summary>
		/// all skillMods that are active for this aspect 
		/// </summary>
		public IEnumerable<SkillMod> SkillMods => CurrentStage?.skillMods ?? Enumerable.Empty<SkillMod>();

		/// <summary>
		/// get the offset to the hediff's severity 
		/// </summary>
		/// <param name="hediff"></param>
		/// <returns></returns>
		public float GetBoostOffset(Hediff hediff)
		{
			return GetBoostOffset(hediff.def);
		}

		/// <summary> The production boosts of the current stage. </summary>
		public IEnumerable<ProductionBoost> ProductionBoosts =>
			CurrentStage.productionBoosts ?? Enumerable.Empty<ProductionBoost>();


		/// <summary> Get the production boost for the given mutation hediff. </summary>
		public float GetBoostOffset(HediffDef hediff)
		{
			float accum = 0;
			foreach (ProductionBoost productionBoost in ProductionBoosts)
			{
				accum += productionBoost.GetBoost(hediff);
			}

			return accum;

		}

		/// <summary> Called after this affinity is added to the pawn. </summary>
		public void Added(Pawn pawn, int startStage = 0)
		{
			_pawn = pawn;
			if (!_wasStarted)
			{
				_wasStarted = true;
				Start();
			}

			PostAdd();
			StageIndex = startStage;
		}

		/// <summary> Called during startup to initialize all affinities. </summary>
		public void Initialize()
		{
			PostInit();
			if (!_wasStarted)
			{
				_wasStarted = true;
				Start();
			}
		}

		/// <summary> Called after the pawn is despawned. </summary>
		public virtual void PostDeSpawn()
		{
		}

		/// <summary> Called after transfer to/from animal </summary>
		/// <param name="newAspect">Aspect on destination pawn</param>
		public virtual void PostTransfer(Aspect newAspect)
		{
		}

		/// <summary> Called when the pawn's race changes. </summary>
		public virtual void PostRaceChange(ThingDef oldRace)
		{
		}

		/// <summary> Called after this affinity is removed from the pawn. </summary>
		public virtual void PostRemove()
		{
			if (CurrentStage != null) UndoEffectsOfStage(CurrentStage);
		}

		/// <summary> Called after the pawn is spawned. </summary>
		public virtual void PostSpawnSetup(bool respawningAfterLoad)
		{
		}

		/// <summary> Called every tick. </summary>
		public virtual void PostTick()
		{
			if (CurrentStage.mentalStateGivers != null && Pawn.IsHashIntervalTick(60) && !Pawn.InMentalState)
				DoMentalStateChecks();
		}

		private void DoMentalStateChecks()
		{
			var mentalStateHandler = Pawn.mindState.mentalStateHandler;
			// ReSharper disable once PossibleNullReferenceException
			foreach (MentalStateGiver giver in CurrentStage.mentalStateGivers)
			{
				if (Rand.MTBEventOccurs(giver.mtbDays, 60000f, 60))
				{
					if (mentalStateHandler.TryStartMentalState(giver.mentalState,
																MENTAL_BREAK_TRANSLATION_LABEL.Translate(Label)))
						return; //only give one mental state
				}
			}
		}

		/// <summary> Call to set ShouldRemove to true. </summary>
		public void StageToRemove()
		{
			ShouldRemove = true;
		}

		/// <summary> Called during IExposable's ExposeData to serialize data. </summary>
		protected virtual void ExposeData() // Want this hidden from the public interface of the class 
		{
		}


		/// <summary> Called after this instance is added to the pawn. </summary>
		protected virtual void PostAdd()
		{
			CalculateSkillChanges();
		}


		/// <summary> Called after the base instance is initialize. </summary>
		protected virtual void PostInit()
		{
		}

		/// <summary>
		/// called after this aspect stage changes 
		/// </summary>
		/// <param name="lastStage"></param>
		protected virtual void PostStageChanged(int lastStage)
		{
			if (lastStage >= 0)
				UndoEffectsOfStage(def.stages[lastStage]);

			CalculateSkillChanges();
			Tracker?.Notify_StageChanged(this, lastStage);
		}

		/// <summary>
		/// an enumerable collection of all stat modifiers currently active for this stage 
		/// </summary>
		public IEnumerable<StatModifier> StatOffsets => CurrentStage?.statOffsets ?? Enumerable.Empty<StatModifier>();

		/// <summary> Called once during the startup of this instance, either after initialization or after being added to the pawn. </summary>
		protected virtual void Start()
		{

		}

		/// <summary>
		/// called when the effects of the given stage must be undone 
		/// </summary>
		/// <param name="lastStage"></param>
		protected virtual void UndoEffectsOfStage(AspectStage lastStage)
		{
			UndoSkillChanges();
		}

		private void CalculateSkillChanges()
		{
			IEnumerable<SkillMod> skillMods = CurrentStage.skillMods ?? Enumerable.Empty<SkillMod>();
			Pawn_SkillTracker skills = Pawn.skills;

			_addedSkillsActualAmount = new Dictionary<SkillDef, float>();
			_originalPassions = new Dictionary<SkillDef, Passion>();

			foreach (SkillMod skillMod in skillMods)
			{
				SkillRecord skR = skills.GetSkill(skillMod.skillDef);
				Passion oldPassion = skR.passion;
				_originalPassions[skR.def] = oldPassion;
				float oldXp = skR.XpTotalEarned; //store the original total xp 

				skR.passion = skillMod.GetNewPassion(skR.passion);

				skR.Learn(skillMod.addedXp, true);

				float dXp = skR.XpTotalEarned - oldXp; //now get the delta value 
				_addedSkillsActualAmount[skR.def] = dXp;
			}
		}

		private void StageChanged(int lastStage)
		{
			Tracker?.Notify_AspectChanged(this);
			PostStageChanged(lastStage);
		}

		private void UndoSkillChanges()
		{
			IEnumerable<KeyValuePair<SkillDef, float>> addedSkills =
				_addedSkillsActualAmount ?? Enumerable.Empty<KeyValuePair<SkillDef, float>>();
			IEnumerable<KeyValuePair<SkillDef, Passion>> skillPassions =
				_originalPassions ?? Enumerable.Empty<KeyValuePair<SkillDef, Passion>>();

			Pawn_SkillTracker skills = Pawn.skills;

			foreach (KeyValuePair<SkillDef, Passion> skillPassion in skillPassions) //undo passions first 
			{
				SkillRecord skR = skills.GetSkill(skillPassion.Key);
				skR.passion = skillPassion.Value;
			}

			foreach (KeyValuePair<SkillDef, float> keyValuePair in addedSkills) //now undo the added exp 
			{
				SkillDef sk = keyValuePair.Key;
				float v = keyValuePair.Value;
				SkillRecord skR = skills.GetSkill(sk);
				skR.Learn(-v, true);
			}
		}
		/// <summary>
		/// generate a tip string for this aspect to be displayed in the mutation ITab 
		/// </summary>
		/// <param name="pawn"></param>
		/// <returns></returns>
		public virtual string TipString(Pawn pawn)
		{
			StringBuilder stringBuilder = new StringBuilder();
			AspectStage currentStage = CurrentStage;

			stringBuilder.Append(Description.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN"));
			int count = CurrentStage.skillMods?.Count ?? 0;
			if (count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
			}

			int num = 0;
			foreach (SkillMod skillMod in SkillMods)
			{
				string value = "    " //skill mods might still do something if they don't add xp 
							 + skillMod.skillDef.skillLabel.CapitalizeFirst()
							 + ": "
							 + skillMod.addedXp.ToString("+##;-##")
							 + " XP";
				if (skillMod.passionOffset != 0)
					value += ", " + skillMod.passionOffset.ToString("+##;-##") + " " + "Passion".Translate();
				if (num < count - 1)
					stringBuilder.AppendLine(value);
				else
					stringBuilder.Append(value);
				num++;
			}

			if (GetPermaThoughts().Any<ThoughtDef>())
			{
				stringBuilder.AppendLine();
				foreach (ThoughtDef thoughtDef in GetPermaThoughts())
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("    " + "PermanentMoodEffect".Translate() + " " + thoughtDef.stages[0].baseMoodEffect.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset));
				}
			}

			if (currentStage.statOffsets != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				for (int i = 0; i < currentStage.statOffsets.Count; i++)
				{
					StatModifier statModifier = currentStage.statOffsets[i];
					string valueToStringAsOffset = statModifier.ValueToStringAsOffset;
					string value2 = "    " + statModifier.stat.LabelCap + " " + valueToStringAsOffset;
					if (i < currentStage.statOffsets.Count - 1)
					{
						stringBuilder.AppendLine(value2);
					}
					else
					{
						stringBuilder.Append(value2);
					}
				}
			}

			if (currentStage.capMods != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				for (int i = 0; i < currentStage.capMods.Count; i++)
				{
					PawnCapacityModifier capMods = currentStage.capMods[i];
					string capacityString = "    " + capMods.capacity.ToString() + ": " + capMods.offset.ToStringPercent("+##;-##");
					if (i < currentStage.capMods.Count - 1)
					{
						stringBuilder.AppendLine(capacityString);
					}
					else
					{
						stringBuilder.Append(capacityString);
					}
				}
			}
			return stringBuilder.ToString();
		}

		private IEnumerable<ThoughtDef> GetPermaThoughts()
		{
			AspectStage degree = CurrentStage;
			List<ThoughtDef> allThoughts = DefDatabase<ThoughtDef>.AllDefsListForReading;
			for (int i = 0; i < allThoughts.Count; i++)
			{
				// To-Do

				//if (allThoughts[i].IsSituational)
				//{
				//    if (allThoughts[i].Worker is ThoughtWorker_AlwaysActive)
				//    {
				//        if (allThoughts[i].requiredTraits != null && allThoughts[i].requiredTraits.Contains(def))
				//        {
				//            if (!allThoughts[i].RequiresSpecificTraitsDegree || allThoughts[i].requiredTraitsDegree == degree.degree)
				//            {
				//                yield return allThoughts[i];
				//            }
				//        }
				//    }
				//}
			}
			yield break;
		}
	}
}
