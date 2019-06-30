using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using Verse.AI;
using RimWorld;

namespace Pawnmorph
{
    /*
    public abstract class JobDriver_GatherMorphBodyResources : JobDriver
    {
        private float gatherProgress;

        protected const TargetIndex AnimalInd = TargetIndex.A;

        protected abstract float WorkTotal
        {
            get;
        }

        protected abstract CompHasGatherableBodyResource GetComp(Pawn animal);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref gatherProgress, "gatherProgress", 0f);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = base.pawn;
            LocalTargetInfo target = base.job.GetTarget(TargetIndex.A);
            Job job = base.job;
            bool errorOnFailed2 = errorOnFailed;
            return pawn.Reserve(target, job, 1, -1, null, errorOnFailed2);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnDowned(TargetIndex.A);
            this.FailOnNotCasualInterruptible(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil wait = new Toil();
            wait.initAction = delegate
            {
                Pawn actor2 = wait.actor;
                Pawn pawn2 = (Pawn)job.GetTarget(TargetIndex.A).Thing;
                actor2.pather.StopDead();
                PawnUtility.ForceWait(pawn2, 15000, null, maintainPosture: true);
            };
            wait.tickAction = delegate
            {
                Pawn actor = wait.actor;
                actor.skills.Learn(SkillDefOf.Animals, 0.13f);
                gatherProgress += actor.GetStatValue(StatDefOf.AnimalGatherSpeed);
                if (gatherProgress >= WorkTotal)
                {
                    GetComp((Pawn)(Thing)job.GetTarget(TargetIndex.A)).Gathered(pawn);
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            };
            wait.AddFinishAction(delegate
            {
                Pawn pawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
                if (pawn != null && pawn.CurJobDef == JobDefOf.Wait_MaintainPosture)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            });
            wait.FailOnDespawnedOrNull(TargetIndex.A);
            wait.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            wait.AddEndCondition(() => GetComp((Pawn)(Thing)job.GetTarget(TargetIndex.A)).ActiveAndFull ? JobCondition.Ongoing : JobCondition.Incompletable);
            wait.defaultCompleteMode = ToilCompleteMode.Never;
            wait.WithProgressBar(TargetIndex.A, () => gatherProgress / WorkTotal);
            wait.activeSkill = (() => SkillDefOf.Animals);
            yield return wait;
        }
    }

    public class JobDriver_MilkHediff : JobDriver_GatherMorphBodyResources
    {
        protected override float WorkTotal => 400f;

        protected override PM_CompHasGatherableBodyResource GetComp(Hediff hediff)
        {
            return hediff.TryGetComp<CompMilkableMorph>();
        }
    }

    public class JobDriver_ShearHediff : JobDriver_GatherMorphBodyResources
    {
        protected override float WorkTotal => 1700f;

        protected override PM_CompHasGatherableBodyResource GetComp(Hediff hediff)
        {
            return hediff.TryGetComp<CompShearable>();
        }
    }


    public abstract class PM_CompHasGatherableBodyResource : HediffComp
    {
        protected float fullness;

        protected abstract int GatherResourcesIntervalDays
        {
            get;
        }

        protected abstract int ResourceAmount
        {
            get;
        }

        protected abstract ThingDef ResourceDef
        {
            get;
        }

        protected abstract string SaveKey
        {
            get;
        }

        public float Fullness => fullness;

        protected virtual bool Active
        {
            get
            {
                if (parent.pawn.Faction == null)
                {
                    return false;
                }
                return true;
            }
        }

        public bool ActiveAndFull
        {
            get
            {
                if (!Active)
                {
                    return false;
                }
                return fullness >= 1f;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref fullness, SaveKey, 0f);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Active)
            {
                float num = 1f / (float)(GatherResourcesIntervalDays * 60000);
                Pawn pawn = parent.pawn as Pawn;
                if (pawn != null)
                {
                    num *= PawnUtility.BodyResourceGrowthSpeed(pawn);
                }
                fullness += num;
                if (fullness > 1f)
                {
                    fullness = 1f;
                }
            }
        }

        public void Gathered(Pawn doer)
        {
            if (!Active)
            {
                Log.Error(doer + " gathered body resources while not Active: " + parent);
            }
            if (!Rand.Chance(doer.GetStatValue(StatDefOf.AnimalGatherYield)))
            {
                Vector3 loc = (doer.DrawPos + parent.pawn.DrawPos) / 2f;
                MoteMaker.ThrowText(loc, parent.pawn.Map, "TextMote_ProductWasted".Translate(), 3.65f);
            }
            else
            {
                int num = GenMath.RoundRandom((float)ResourceAmount * fullness);
                while (num > 0)
                {
                    int num2 = Mathf.Clamp(num, 1, ResourceDef.stackLimit);
                    num -= num2;
                    Thing thing = ThingMaker.MakeThing(ResourceDef);
                    thing.stackCount = num2;
                    GenPlace.TryPlaceThing(thing, doer.Position, doer.Map, ThingPlaceMode.Near);
                }
            }
            fullness = 0f;
        }
    }


    public class CompMilkableMorph : PM_CompHasGatherableBodyResource
    {
        protected override int GatherResourcesIntervalDays => Props.milkIntervalDays;

        protected override int ResourceAmount => Props.milkAmount;

        protected override ThingDef ResourceDef => Props.milkDef;

        protected override string SaveKey => "milkFullness";

        public CompProperties_MilkableMorph Props => (CompProperties_MilkableMorph)props;

        protected override bool Active
        {
            get
            {
                if (!base.Active)
                {
                    return false;
                }
                Pawn pawn = parent.pawn as Pawn;
                if (Props.milkFemaleOnly && pawn != null && pawn.gender != Gender.Female)
                {
                    return false;
                }
                if (pawn != null && !pawn.ageTracker.CurLifeStage.milkable)
                {
                    return false;
                }
                return true;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (!Active)
            {
                return null;
            }
            return "MilkFullness".Translate() + ": " + base.Fullness.ToStringPercent();
        }
    }

    public class CompProperties_MilkableMorph : HediffCompProperties
    {
        public int milkIntervalDays;

        public int milkAmount = 1;

        public ThingDef milkDef;

        public bool milkFemaleOnly = true;

        public CompProperties_MilkableMorph()
        {
            compClass = typeof(CompMilkable);
        }
    }
    */
}
