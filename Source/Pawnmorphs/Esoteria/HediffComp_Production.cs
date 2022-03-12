using System;
using System.Linq;
using System.Text;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.GraphicSys;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
    /// <summary>
    /// hediff comp for producing resources over time 
    /// </summary>
    /// <seealso cref="Verse.HediffComp" />
    public class HediffComp_Production : HediffComp, IDebugString
    {
        private const float SEVERITY_LERP = 0.1f;
        private const int PRODUCTION_MULT_UPDATE_PERIOD = 60;
        private const int TICKS_PER_DAY = 60000;


        /// <summary>The hatching ticker</summary>
        public float HatchingTicker = 0;
        /// <summary>The total amount produced by this pawn</summary>
        public int totalProduced = 0;

        private float _brokenChance = 0f;
        private float _bondChance = 0f;
        private float _severityTarget;
        private float _severity;
        private bool _canProduceNow;
        private Cached<bool> _canProduce;
        private HediffComp_Staged _currentStage;

        /// <summary>
        /// Gets a value indicating whether this instance can produce.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can produce; otherwise, <c>false</c>.
        /// </value>
        public bool CanProduce => _canProduce.Value;

        /// <summary>
        /// if this instance can produce a product now 
        /// </summary>
        public bool CanProduceNow => _canProduceNow;

        /// <summary>
        /// Gets the current stage.
        /// </summary>
        public HediffComp_Staged CurStage => _currentStage;
        public int Stage { get; private set; }


        /// <summary>Gets the properties of this comp</summary>
        /// <value>The props.</value>
        public HediffCompProperties_Production Props => (HediffCompProperties_Production)props;


        /// <summary>
        /// Initializes a new instance of the <see cref="HediffComp_Production"/> class.
        /// </summary>
        public HediffComp_Production()
        {
            _canProduce = new Cached<bool>(GetCanProduce);
        }

        private bool GetCanProduce()
        {
            var mInfused = Pawn.GetAspectTracker()?.GetAspect(AspectDefOf.MutagenInfused);
            return mInfused?.StageIndex != 2; //dry is the third stage 
                                                //TODO make this a stat? 
        }


        /// <summary>
        /// Recalculates current stage.
        /// </summary>
        private void UpdateCurrentStage()
        {
            _currentStage = null;
            if (Props.stages != null && Props.stages.Count > 0)
            {
                HediffComp_Staged stage;
                for (int i = 0; i < Props.stages.Count; i++)
                {
                    stage = Props.stages[i];
                    if (stage.minSeverity > _severity)
                        break;

                    _currentStage = stage;
                    Stage = Props.stages.IndexOf(stage);
                }
            }
        }

        /// <summary>called every tick after it's parent is updated</summary>
        /// <param name="severityAdjustment">The severity adjustment.</param>
        public override void CompPostTick(ref float severityAdjustment)
        {
            TryProduce();
            TickHediffGivers();
        }

        private void TickHediffGivers()
        {
            if (_currentStage == null)
                return;

            if (_currentStage.hediffGivers != null && parent.pawn.IsHashIntervalTick(60))
            {
                for (int j = 0; j < _currentStage.hediffGivers.Count; j++)
                {
                    _currentStage.hediffGivers[j].OnIntervalPassed(Pawn, parent);
                }
            }
        }


        /// <summary>exposes the data of this comp. Called after it's parent ExposeData is called</summary>
        public override void CompExposeData()
        {
            Scribe_Values.Look(ref HatchingTicker, "hatchingTicker");
            Scribe_Values.Look(ref _brokenChance, "brokenChance");
            Scribe_Values.Look(ref _bondChance, "bondChance");
            Scribe_Values.Look(ref totalProduced, "totalProduced");
            Scribe_Values.Look(ref _canProduceNow, nameof(CanProduceNow));
            Scribe_Values.Look(ref _severity, "severity");
            base.CompExposeData();

            UpdateCurrentStage();
        }

        void TryProduce()
        {
            float daysToProduce = _currentStage?.daysToProduce ?? Props.daysToProduce;
            if (HatchingTicker < daysToProduce * TICKS_PER_DAY)
            {
                HatchingTicker++;

                if (parent.pawn.IsHashIntervalTick(PRODUCTION_MULT_UPDATE_PERIOD))
                {
                    _severityTarget = (Pawn.GetAspectTracker() ?? Enumerable.Empty<Aspect>()).GetProductionBoost(parent.def); // Update the production multiplier only occasionally for performance reasons. 

                    float oldSeverity = _severity;
                    _severity = Mathf.Lerp(oldSeverity, _severityTarget, SEVERITY_LERP); // Have the severity increase gradually.
                    
                    // Recalculate current stage if severity is different.
                    if (oldSeverity != _severity)
                        UpdateCurrentStage();

                    _canProduce.Recalculate();
                }
            }
            else if (Pawn.Spawned) //(Pawn.Map != null)
            {
                if (!CanProduce)
                {
                    HatchingTicker = 0;
                    return;//it's here so we don't check for the aspect every tick 
                }

                _canProduceNow = true; 
                if (Props.JobGiver != null && !Pawn.Downed)
                {
                    GiveJob();
                }
                else
                {
                    Produce();
                }
            }
        }

        private void GiveJob()
        {
            HatchingTicker = 0;
            var jobPkg = Props.JobGiver.TryIssueJobPackage(Pawn, default); // Caller already checked this.

            if (jobPkg.Job == null)
            {
                Produce();
            }
            else
            {
                Pawn.jobs.StartJob(jobPkg.Job, JobCondition.InterruptForced, resumeCurJobAfterwards: true);
            }
        }

        /// <summary> Spawns in the products at the parent's current location. </summary>
        public void Produce()
        {
            int amount = _currentStage?.amount ?? Props.amount;
            float chance = _currentStage?.chance ?? Props.chance;
            ThingDef resource = _currentStage?.Resource ?? Props.Resource;
            ThingDef rareResource = _currentStage?.RareResource ?? Props.RareResource;
            ThoughtDef thought = _currentStage?.thought;
            _canProduceNow = false; 
            Produce(amount, chance, resource, rareResource, thought);
        }


        private void Produce(int amount, float chance, ThingDef resource, ThingDef rareResource, ThoughtDef stageThought)
        {
            MemoryThoughtHandler thoughts = Pawn.needs?.mood?.thoughts?.memories;
            EtherState etherState = Pawn.GetEtherState();
            HatchingTicker = 0;
            var thingCount = 0;
            var rareThingCount = 0;
            Aspect infusedAspect = Pawn.GetAspectTracker()?.GetAspect(AspectDefOf.MutagenInfused);

            int? sIndex = infusedAspect?.StageIndex;


            for (var i = 0; i < amount; i++)
            {
                bool shouldProduceRare;
                switch (sIndex)
                {
                    case null:
                        shouldProduceRare = Rand.RangeInclusive(0, 100) <= chance;
                        break;
                    case 0:
                        shouldProduceRare = true;
                        break;
                    case 1:
                        shouldProduceRare = false;
                        break;
                    case 2:
                        return; //produce nothing 
                    default:
                        throw new ArgumentOutOfRangeException(sIndex.Value.ToString());
                }

                if (shouldProduceRare && rareResource != null)
                    rareThingCount++;
                else
                    thingCount++;
            }

            Thing thing = ThingMaker.MakeThing(resource);
            thing.stackCount = thingCount;

            Color? skinColor = Pawn.GetHighestInfluence()?.GetSkinColorOverride(Pawn); //dont want wool thats mostly human-skin colored

            if (resource.thingCategories.Contains(PMThingCategoryDefOf.Textiles) && resource.CompDefFor<CompColorable>() != null && skinColor.HasValue)
                thing.SetColor(skinColor.Value);
            if (thing.stackCount > 0)
                GenPlace.TryPlaceThing(thing, Pawn.PositionHeld, Pawn.Map, ThingPlaceMode.Near);

            if (rareResource != null)
            {
                Thing rareThing = ThingMaker.MakeThing(rareResource);
                rareThing.stackCount = rareThingCount;
                if (rareResource.thingCategories.Contains(PMThingCategoryDefOf.Textiles) && resource.CompDefFor<CompColorable>() != null && skinColor.HasValue)
                    thing.SetColor(skinColor.Value);
                if (rareThing.stackCount > 0)
                    GenPlace.TryPlaceThing(rareThing, Pawn.PositionHeld, Pawn.Map, ThingPlaceMode.Near);
            }

            if (etherState == EtherState.None)
            {
                if (Rand.RangeInclusive(0, 100) <= _bondChance)
                {
                    GiveEtherState(EtherState.Bond);
                    etherState = EtherState.Bond;
                    Find.LetterStack.ReceiveLetter(
                                                   "LetterHediffFromEtherBondLabel".Translate(Pawn).CapitalizeFirst(),
                                                   "LetterHediffFromEtherBond".Translate(Pawn).CapitalizeFirst(),
                                                   LetterDefOf.NeutralEvent, Pawn);
                }
                else if (Rand.RangeInclusive(0, 100) <= _brokenChance)
                {
                    GiveEtherState(EtherState.Broken);
                    etherState = EtherState.Broken;
                    Find.LetterStack.ReceiveLetter(
                                                   "LetterHediffFromEtherBrokenLabel".Translate(Pawn).CapitalizeFirst(),
                                                   "LetterHediffFromEtherBroken".Translate(Pawn).CapitalizeFirst(),
                                                   LetterDefOf.NeutralEvent, Pawn);
                }
            }

            if (stageThought != null) thoughts?.TryGainMemory(stageThought);

            ThoughtDef addThought;
            switch (etherState)
            {
                case EtherState.None:
                    addThought = Props.genderAversion == Pawn.gender ? Props.wrongGenderThought ?? Props.thought : Props.thought;
                    break;
                case EtherState.Broken:
                    addThought = Props.etherBrokenThought;
                    break;
                case EtherState.Bond:
                    addThought = Props.etherBondThought;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (addThought != null) thoughts?.TryGainMemory(addThought);

            if (etherState == EtherState.None)
            {
                _brokenChance += 0.5f;
                _bondChance += 0.2f;
            }
            totalProduced += rareThingCount + thingCount;
        }

        private void GiveEtherState(EtherState state)
        {
            var aspectTracker = Pawn.GetAspectTracker();
            if (aspectTracker != null)
            {
                int stageNum;

                switch (state)
                {
                    case EtherState.Broken:
                        stageNum = 0;
                        break;
                    case EtherState.Bond:
                        stageNum = 1;
                        break;
                    case EtherState.None:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }

                aspectTracker.Add(AspectDefOf.EtherState, stageNum);
            }
           
        }

        public string GetDescription()
        {
            string description = "";

            var currentStage = CurStage;
            if (currentStage.statOffsets != null)
            {
                for (int i = 0; i < currentStage.statOffsets.Count; i++)
                {
                    StatModifier statModifier = currentStage.statOffsets[i];
                    string valueToStringAsOffset = statModifier.ValueToStringAsOffset;
                    string value2 = "    " + statModifier.stat.LabelCap + " " + valueToStringAsOffset;
                    if (i < currentStage.statOffsets.Count - 1)
                    {
                        description += value2;
                        description += Environment.NewLine;
                    }
                    else
                    {
                        description += value2;
                    }
                }
            }
            return description;
        }

        public string ToStringFull()
        {
            StringBuilder debugString = new StringBuilder();
            debugString.AppendLine("Severity: " + _severity);
            debugString.AppendLine("Target severity: " + _severityTarget);
            debugString.AppendLine("Can produce: " + CanProduce);
            debugString.AppendLine("Broken chance: " + _brokenChance);
            debugString.AppendLine("Bond chance: " + _bondChance);

            return debugString.ToString();
        }
    }
}
