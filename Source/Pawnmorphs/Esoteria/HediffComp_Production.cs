using System;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
    public class HediffComp_Production : HediffComp
    {
        private const float SEVERITY_LERP = 0.1f;
        private const int PRODUCTION_MULT_UPDATE_PERIOD = 60;
        private const int TICKS_PER_DAY = 60000;

        public float HatchingTicker = 0;

        private float brokenChance = 0f;
        private float bondChance = 0f;
        private float _severityTarget;

        public HediffCompProperties_Production Props => (HediffCompProperties_Production)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            TryProduce();
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref HatchingTicker, "hatchingTicker");
            Scribe_Values.Look(ref brokenChance, "brokenChance");
            Scribe_Values.Look(ref bondChance, "bondChance");
            base.CompExposeData();
        }

        void TryProduce()
        {
            var curStage = Props.stages?.ElementAt(parent.CurStageIndex);
            float daysToProduce = curStage?.daysToProduce ?? Props.daysToProduce;
            if (HatchingTicker < daysToProduce * TICKS_PER_DAY)
            {
                HatchingTicker++;

                if (parent.pawn.IsHashIntervalTick(PRODUCTION_MULT_UPDATE_PERIOD))
                {
                    _severityTarget = (Pawn.GetAspectTracker() ?? Enumerable.Empty<Aspect>()).GetProductionBoost(parent.def); // Update the production multiplier only occasionally for performance reasons. 
                    var severity = Mathf.Lerp(parent.Severity, _severityTarget, SEVERITY_LERP); // Have the severity increase gradually.
                    parent.Severity = severity;
                }
            }
            else if (Pawn.Map != null)
            {
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
            var curStage = Props.stages?.ElementAt(parent.CurStageIndex);
            int amount = curStage?.amount ?? Props.amount;
            float chance = curStage?.chance ?? Props.chance;
            ThingDef resource = curStage?.Resource ?? Props.Resource;
            ThingDef rareResource = curStage?.RareResource ?? Props.RareResource;
            ThoughtDef thought = curStage?.thought;
            Produce(amount, chance, resource, rareResource, thought);
        }

        private void Produce(int amount, float chance, ThingDef resource, ThingDef rareResource, ThoughtDef stageThought)
        {
            RandUtilities.PushState();

            MemoryThoughtHandler thoughts = Pawn.needs.mood.thoughts.memories;
            EtherState etherState = Pawn.GetEtherState();
            HatchingTicker = 0;
            var thingCount = 0;
            var rareThingCount = 0;

            for (var i = 0; i < amount; i++)
                if (Rand.RangeInclusive(0, 100) <= chance && rareResource != null)
                    rareThingCount++;
                else
                    thingCount++;

            Thing thing = ThingMaker.MakeThing(resource);
            thing.stackCount = thingCount;
            if (thing.stackCount > 0)
                GenPlace.TryPlaceThing(thing, Pawn.PositionHeld, Pawn.Map, ThingPlaceMode.Near);

            if (rareResource != null)
            {
                Thing rareThing = ThingMaker.MakeThing(rareResource);
                rareThing.stackCount = rareThingCount;
                if (rareThing.stackCount > 0)
                    GenPlace.TryPlaceThing(rareThing, Pawn.PositionHeld, Pawn.Map, ThingPlaceMode.Near);
            }

            if (etherState == EtherState.None)
            {
                if (Rand.RangeInclusive(0, 100) <= bondChance)
                {
                    GiveEtherState(EtherState.Bond);
                    etherState = EtherState.Bond;
                    Find.LetterStack.ReceiveLetter(
                                                   "LetterHediffFromEtherBondLabel".Translate(Pawn).CapitalizeFirst(),
                                                   "LetterHediffFromEtherBond".Translate(Pawn).CapitalizeFirst(),
                                                   LetterDefOf.NeutralEvent, Pawn);
                }
                else if (Rand.RangeInclusive(0, 100) <= brokenChance)
                {
                    GiveEtherState(EtherState.Broken);
                    etherState = EtherState.Broken;
                    Find.LetterStack.ReceiveLetter(
                                                   "LetterHediffFromEtherBrokenLabel".Translate(Pawn).CapitalizeFirst(),
                                                   "LetterHediffFromEtherBroken".Translate(Pawn).CapitalizeFirst(),
                                                   LetterDefOf.NeutralEvent, Pawn);
                }
            }

            if (stageThought != null) thoughts.TryGainMemory(stageThought);

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
            if (addThought != null) thoughts.TryGainMemory(addThought);

            if (etherState == EtherState.None)
            {
                brokenChance += 0.5f;
                bondChance += 0.2f;
            }

            RandUtilities.PopState();
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
            else
            {
                Log.Warning($"{Pawn.Name} does not have an aspect tracker! adding the deprecated hediff instead");
                HediffDef hDef;
                switch (state)
                {
                    case EtherState.Broken:
                        hDef = TfHediffDefOf.EtherBroken;
                        break;
                    case EtherState.Bond:
                        hDef = TfHediffDefOf.EtherBond;
                        break;
                    case EtherState.None:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }

                Pawn.health.AddHediff(hDef);
            }
        }
    }
}
