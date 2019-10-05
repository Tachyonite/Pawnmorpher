// Aspect.cs created by Iron Wolf for Pawnmorph on 09/23/2019 8:07 AM
// last updated 09/23/2019  12:39 PM

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     base class for all "mutation affinities"
    /// </summary>
    /// affinities are things that are more global than hediffs but more temporary than traits
    public class Aspect : IExposable
    {

        

        public AspectDef def;

        private int _stage = -1;

        private Pawn _pawn;

        private bool _shouldRemove;
        private bool _wasStarted;

        public Color LabelColor => CurrentStage.labelColor ?? def.labelColor; 

        private Dictionary<SkillDef, float> _addedSkillsActualAmount;
        private Dictionary<SkillDef, Passion> _originalPassions;

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

        public IEnumerable<PawnCapacityModifier> CapMods => CurrentStage.capMods ?? Enumerable.Empty<PawnCapacityModifier>();

        public bool HasCapMods => CurrentStage.capMods != null && CurrentStage.capMods.Count != 0;

        /// <summary>
        ///     the current stage index
        /// </summary>
        public int StageIndex
        {
            get => _stage;
            set
            {
                int st = Mathf.Clamp(value, 0, Stages.Count - 1);
                if (_stage != st)
                {
                    int last = _stage;
                    _stage = st;
                    StageChanged(last);
                }
            }
        }

        /// <summary>
        ///     the current stage
        /// </summary>
        public AspectStage CurrentStage => Stages[StageIndex];

        public string Label
        {
            get
            {
                string lBase = string.IsNullOrEmpty(CurrentStage.label) ? def.label : CurrentStage.label;
                if (!string.IsNullOrEmpty(CurrentStage.modifier)) lBase = $"{lBase} ({CurrentStage.modifier})";

                return lBase;
            }
        }

        /// <summary>
        ///     the description of the aspect, taking into account it's current stage
        /// </summary>
        public string Description =>
            string.IsNullOrEmpty(CurrentStage.description)
                ? string.IsNullOrEmpty(def.description) ? "NO DESCRIPTION " : def.description
                : CurrentStage.description;

        /// <summary>
        ///     the pawn this is attached to
        /// </summary>
        public Pawn Pawn => _pawn;

        /// <summary>
        ///     if this affinity should be removed or not
        /// </summary>
        public bool ShouldRemove
        {
            get => _shouldRemove;
            protected set => _shouldRemove = value;
        }

        protected List<AspectStage> Stages => def.stages;

        public float GetBoostOffset(Hediff hediff)
        {
            return GetBoostOffset(hediff.def);
        }

        /// <summary>
        /// the production boosts of the current stage 
        /// </summary>
        public IEnumerable<ProductionBoost> ProductionBoosts =>
            CurrentStage.productionBoosts ?? Enumerable.Empty<ProductionBoost>();

        
        /// <summary>
        /// get the production boost for the given mutation hediff 
        /// </summary>
        /// <param name="hediff"></param>
        /// <returns></returns>
        public float GetBoostOffset(HediffDef hediff)
        {
            float accum = 0;
            foreach (ProductionBoost productionBoost in ProductionBoosts)
            {
                accum += productionBoost.GetBoost(hediff); 
            }

            return accum; 

        }

        /// <summary>
        ///     called after this affinity is added to the pawn
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="startStage"></param>
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

        /// <summary>
        ///     called during startup to initialize all affinities
        /// </summary>
        public void Initialize()
        {
            PostInit();
            if (!_wasStarted)
            {
                _wasStarted = true;
                Start();
            }
        }

        /// <summary>
        ///     called after the pawn is despawned
        /// </summary>
        public virtual void PostDeSpawn()
        {
        }

        /// <summary>
        ///     called when the pawn's race changes
        /// </summary>
        /// <param name="oldRace"></param>
        public virtual void PostRaceChange(ThingDef oldRace)
        {
        }

        /// <summary>
        ///     called after this affinity is removed from the pawn
        /// </summary>
        public virtual void PostRemove()
        {
            if (CurrentStage != null) UndoEffectsOfStage(CurrentStage);
        }

        /// <summary>
        ///     called after the pawn is spawned
        /// </summary>
        /// <param name="respawningAfterLoad"></param>
        public virtual void PostSpawnSetup(bool respawningAfterLoad)
        {
        }

        /// <summary>
        ///     called every tick
        /// </summary>
        public virtual void PostTick()
        {
            if(CurrentStage.mentalStateGivers != null && Pawn.IsHashIntervalTick(60) && !Pawn.InMentalState)
                DoMentalStateChecks();
        }

        private const string MENTAL_BREAK_TRANSLATION_LABEL = "MentalStateReason_Aspect";

        private void DoMentalStateChecks()
        {
            RandUtilities.PushState();
            try
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
            finally
            {
                RandUtilities.PopState(); //whatever happens we need to pop the rand state
            }
        }

        /// <summary>
        ///     call to set ShouldRemove to true
        /// </summary>
        public void StageToRemove()
        {
            ShouldRemove = true;
        }

        /// <summary>
        ///     called During IExposable's ExposeData to serialize data
        /// </summary>
        protected virtual void ExposeData() //want this hidden from the public interface of the class 
        {
        }


        /// <summary>
        ///     called after this instance is added to the pawn
        /// </summary>
        protected virtual void PostAdd()
        {
        }


        /// <summary>
        ///     called after the base instance is initialize
        /// </summary>
        protected virtual void PostInit()
        {
        }

        protected virtual void PostStageChanged(int lastStage)
        {
            if (lastStage >= 0)
                UndoEffectsOfStage(def.stages[lastStage]);

            CalculateSkillChanges();
        }

        
        public IEnumerable<StatModifier> StatOffsets => CurrentStage?.statOffsets ?? Enumerable.Empty<StatModifier>();

        /// <summary>
        ///     called once during the startup of this instance, either after initialization or after being added to the pawn
        /// </summary>
        protected virtual void Start()
        {

        }

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
            PostStageChanged(lastStage);
            //TODO
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

        public IEnumerable<SkillMod> SkillMods => CurrentStage?.skillMods ?? Enumerable.Empty<SkillMod>();

        public string TipString(Pawn pawn)
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
            if (currentStage.statFactors != null)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                for (int j = 0; j < currentStage.statFactors.Count; j++)
                {
                    StatModifier statModifier2 = currentStage.statFactors[j];
                    string toStringAsFactor = statModifier2.ToStringAsFactor;
                    string value3 = "    " + statModifier2.stat.LabelCap + " " + toStringAsFactor;
                    if (j < currentStage.statFactors.Count - 1)
                    {
                        stringBuilder.AppendLine(value3);
                    }
                    else
                    {
                        stringBuilder.Append(value3);
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