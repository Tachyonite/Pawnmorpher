using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
using Harmony;
using RimWorld.Planet;

namespace Pawnmorph
{
    public class JobDriver_EnterMutagenChamber : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = base.pawn;
            LocalTargetInfo targetA = base.job.targetA;
            Job job = base.job;
            bool errorOnFailed2 = errorOnFailed;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed2);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil prepare = Toils_General.Wait(500);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            prepare.WithProgressBarToilDelay(TargetIndex.A);
            yield return prepare;
            Toil enter = new Toil();
            enter.initAction = delegate
            {
                Pawn actor = enter.actor;
                Building_MutagenChamber pod = (Building_MutagenChamber)actor.CurJob.targetA.Thing;
                Action action = delegate
                {
                    actor.DeSpawn();
                    pod.TryAcceptThing(actor);
                };
                if (!pod.def.building.isPlayerEjectable)
                {
                    int freeColonistsSpawnedOrInPlayerEjectablePodsCount = Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount;
                    if (freeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate(actor.Named("PAWN")).AdjustedFor(actor), action));
                    }
                    else
                    {
                        action();
                    }
                }
                else
                {
                    action();
                }
            };
            enter.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return enter;
        }
    }
    public class JobDriver_CarryToMutagenChamber : JobDriver
    {
        private const TargetIndex TakeeInd = TargetIndex.A;

        private const TargetIndex DropPodInd = TargetIndex.B;

        protected Pawn Takee
        {
            get
            {
                return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Building_MutagenChamber DropPod
        {
            get
            {
                return (Building_MutagenChamber)this.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.Takee;
            Job job = this.job;
            bool result;
            if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                pawn = this.pawn;
                target = this.DropPod;
                job = this.job;
                result = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
            }
            else
            {
                result = false;
            }
            return result;
        }



        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalState(TargetIndex.A);
            this.FailOn(() => !this.DropPod.Accepts(this.Takee));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOn(() => this.DropPod.GetDirectlyHeldThings().Count > 0).FailOn(() => !this.Takee.Downed).FailOn(() => !this.pawn.CanReach(this.Takee, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn)).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            Toil prepare = Toils_General.Wait(500, TargetIndex.None);
            prepare.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
            prepare.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            yield return prepare;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    this.DropPod.TryAcceptThing(this.Takee, true);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }

        public override object[] TaleParameters()
        {
            return new object[]
            {
                this.pawn,
                this.Takee
            };
        }
    }

    public class Building_MutagenChamber : Building_Casket
    {
        public static JobDef EnterMutagenChamber;
        public float daysToIncubate = 1f;
        public CompFacility compLinked;
        public Building_MutagenModulator modulator;
        public float daysIn = 0f;
        public List<string> pawnkinds;
        public string pawnkind;
        public string forceGender = "Original";
        public float forceGenderChance = 50;
        public PawnKindDef pawnTFKind = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.race.baseBodySize <= 2.9f && x.race.race.intelligence == Intelligence.Animal && x.race.race.FleshType == FleshTypeDefOf.Normal).RandomElement();
        private bool triggered = false;
        public bool doNotEject = false;
        private string hediffDef = "TransformedHuman";
        private CompRefuelable fuelComp = null;
        private CompPowerTrader powerComp = null;
        public Building_MutagenChamber linkTo;

        public Building_MutagenChamber()
        {
            EnterMutagenChamber = DefDatabase<JobDef>.GetNamed("EnterMutagenChamber");
        }

        public override string GetInspectString()
        {
            base.GetInspectString();
            if (this.modulator != null)
            {
                if (this.modulator.merging && this != this.modulator.Chambers.First())
                {
                    return "MutagenChamberProgress".Translate() + ": Merging with linked pod";
                }
                else
                {
                    if (this.modulator.random)
                    {
                        return "MutagenChamberProgress".Translate() + ": " + daysIn.ToStringPercent() + " ???";
                    }
                    return "MutagenChamberProgress".Translate() + ": " + daysIn.ToStringPercent() + " " + pawnTFKind.LabelCap;
                }
            }
            else
            {
                return "MutagenChamberProgress".Translate() + ": " + daysIn.ToStringPercent() + " ???";
            }
            
        }

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                if (allowSpecialEffects)
                {
                    SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(base.Position, base.Map));
                }
                return true;
            }
            return false;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            fuelComp = this.GetComp<CompRefuelable>();
            powerComp = this.GetComp<CompPowerTrader>();
        }

        public override void Tick()
        {
            int tickMult = 60;

            if (this.IsHashIntervalTick(60)) { 
                float num = 1f / (daysToIncubate * 60000f);
                foreach (Thing item in (IEnumerable<Thing>)innerContainer)
                {
                    Pawn pawn = item as Pawn;
                    if (!fuelComp.HasFuel || !powerComp.PowerOn)
                        return;
                    if (this.modulator != null)
                    {
                        if (!this.modulator.powerComp.PowerOn || !this.modulator.flickableComp.SwitchIsOn)
                        {
                            return;
                        }
                    }
                    if (pawn != null)
                    {
                        if (this.modulator == null)
                        {
                            daysIn += num * tickMult;
                        }
                        else if (!this.modulator.merging || (this.modulator.merging && this.linkTo.fuelComp.HasFuel && this.linkTo.innerContainer.Count() > 0))
                        {
                            daysIn += num * tickMult;
                            
                        }
                        
                    }
                    else { daysIn = 0; }
                }

                if (daysIn > 1f && !doNotEject)
                {
                    EjectContents();
                    fuelComp.ConsumeFuel(fuelComp.Fuel);
                    if (this.modulator != null)
                    { this.modulator.triggered = true; }
                    daysIn = 0;

                }
                else if (doNotEject && this.modulator.triggered)
                {
                    fuelComp.ConsumeFuel(fuelComp.Fuel);
                    daysIn = 0;
                    innerContainer.ClearAndDestroyContentsOrPassToWorld();
                    this.modulator.triggered = false;
                }
            }
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref this.daysIn, "daysIn");
            base.ExposeData();
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(myPawn))
            {
                yield return floatMenuOption;
            }
            if (innerContainer.Count == 0)
            {
                if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
                {
                    yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    yield break;
                }
                JobDef jobDef = EnterMutagenChamber;
                string jobStr = "EnterMutagenChamber".Translate();
                Action jobAction = delegate
                {
                    Job job = new Job(jobDef, this);
                    myPawn.jobs.TryTakeOrderedJob(job);
                };
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction), myPawn, this);
            }
        }

        public override void EjectContents()
        {
            if(innerContainer.Count == 0)
            {
                return;
            }
            Pawn pawn = null;
            ThingDef filth_Slime = ThingDefOf.Filth_Slime;
            foreach (Thing item in (IEnumerable<Thing>)innerContainer)
            {
                
                pawn = item as Pawn;
                if (pawn != null && daysIn > 1f)
                {

                    Gender newGender = pawn.gender;

                    if (Rand.RangeInclusive(0, 100) <= forceGenderChance)
                    {
                        switch (forceGender)
                        {
                            case ("Male"):
                                newGender = Gender.Male;
                                break;
                            case ("Female"):
                                newGender = Gender.Female;
                                break;
                            case ("Switch"):
                                switch (pawn.gender)
                                {
                                    case (Gender.Male):
                                        newGender = Gender.Female;
                                        break;
                                    case (Gender.Female):
                                        newGender = Gender.Male;
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    float lifeExpectancyDelta = pawn.def.race.lifeExpectancy / pawnTFKind.race.race.lifeExpectancy;
                    float lifeExpectancy = pawnTFKind.race.race.lifeExpectancy / lifeExpectancyDelta;

                    if (lifeExpectancy > pawn.ageTracker.AgeChronologicalYears)
                    {
                        lifeExpectancy = pawn.ageTracker.AgeChronologicalYears;
                    }
                    Pawn pawnTF = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnTFKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, new float?(lifeExpectancy), new float?(pawn.ageTracker.AgeChronologicalYearsFloat), new Gender?(newGender), null, null));
                    pawnTF.needs.food.CurLevel = pawn.needs.food.CurLevel;
                    pawnTF.needs.rest.CurLevel = pawn.needs.rest.CurLevel;
                    pawnTF.training.SetWantedRecursive(TrainableDefOf.Obedience, true);
                    pawnTF.training.Train(TrainableDefOf.Obedience, null, true);
                    pawnTF.Name = pawn.Name;
                    Pawn pawn3 = (Pawn)GenSpawn.Spawn(pawnTF, pawn.PositionHeld, pawn.MapHeld, 0);
                    for (int i = 0; i < 10; i++)
                    {
                        IntermittentMagicSprayer.ThrowMagicPuffDown(pawn3.Position.ToVector3(), pawn3.MapHeld);
                        IntermittentMagicSprayer.ThrowMagicPuffUp(pawn3.Position.ToVector3(), pawn3.MapHeld);
                    }

                    int faction = Rand.RangeInclusive(0, 2);

                    pawn3.SetFaction(Faction.OfPlayer, null);

                    
                    pawn.apparel.DropAll(pawn.PositionHeld);
                    pawn.equipment.DropAllEquipment(pawn.PositionHeld);
                    if (pawn.RaceProps.intelligence == Intelligence.Humanlike)
                    {
                        if (this.modulator != null)
                        {
                            if (this.modulator.merging)
                            {
                                hediffDef = "2xMergedHuman";
                                Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationMergeLabel".Translate(pawn.LabelShort, this.linkTo.innerContainer.First().LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), "LetterHediffFromTransformationMerge".Translate(pawn.LabelShort, this.linkTo.innerContainer.First().LabelCap, pawnTFKind.LabelCap).CapitalizeFirst(), LetterDefOf.NeutralEvent, pawn, null, null);
                            }
                            else
                            {
                                hediffDef = "TransformedHuman";
                                Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationLabel".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), "LetterHediffFromTransformation".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), LetterDefOf.NeutralEvent, pawn, null, null);
                            }
                        }
                        else
                        {
                            Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationLabel".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), "LetterHediffFromTransformation".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), LetterDefOf.NeutralEvent, pawn, null, null);

                        }
                        Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named(hediffDef), pawn3, null);
                        hediff.Severity = Rand.Range(0.00f, 1.00f);
                        pawn3.health.AddHediff(hediff, null, null, null);

                    }
                    if (this.modulator != null)
                    {
                        if (this.modulator.merging)
                        {
                            PawnMorphInstanceMerged pm = new PawnMorphInstanceMerged((Pawn)this.linkTo.innerContainer.First(), (Pawn)this.innerContainer.First(), pawn3);
                            Find.World.GetComponent<PawnmorphGameComp>().addPawnMerged(pm);
                        }
                        else
                        {
                            PawnMorphInstance pmm = new PawnMorphInstance((Pawn)this.innerContainer.First(), pawn3);
                            Find.World.GetComponent<PawnmorphGameComp>().addPawn(pmm);
                        }

                    }
                    else
                    {
                        PawnMorphInstance pmm = new PawnMorphInstance((Pawn)this.innerContainer.First(), pawn3);
                        Find.World.GetComponent<PawnmorphGameComp>().addPawn(pmm);
                        pawnTFKind = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.race.baseBodySize <= 2.9f && x.race.race.intelligence == Intelligence.Animal && x.race.race.FleshType == FleshTypeDefOf.Normal).RandomElement();
                    }
                    
                    Find.TickManager.slower.SignalForceNormalSpeedShort();
                    PawnComponentsUtility.AddComponentsForSpawn(pawn3);
                    if(this.modulator != null)
                    {
                        this.modulator.triggered = true;
                    }
                    

                }
            
            }
            if (!base.Destroyed)
            {
                SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
                fuelComp.ConsumeFuel(fuelComp.Fuel);

            }
            base.EjectContents();
            
            if (daysIn < 1f)
            {
                pawn.health.AddHediff(HediffDef.Named("FullRandomTFAnyOutcome"));
                if (this.modulator != null && this.linkTo != null && this.modulator.merging)
                {
                    this.linkTo.EjectContents();
                }
            }
            if (daysIn > 1f && pawn.Spawned) { 
                pawn.DeSpawn();
            }
            daysIn = 0;
        }

        

        public static Building_MutagenChamber FindCryptosleepCasketFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
        {
            IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
                                               where typeof(Building_MutagenChamber).IsAssignableFrom(def.thingClass)
                                               select def;
            foreach (ThingDef item in enumerable)
            {
                Building_MutagenChamber building_MutagenChamber = (Building_MutagenChamber)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(item), PathEndMode.InteractionCell, TraverseParms.For(traveler), 9999f, delegate (Thing x)
                {
                    int result;
                    if (!((Building_MutagenChamber)x).HasAnyContents)
                    {
                        Pawn p2 = traveler;
                        LocalTargetInfo target = x;
                        bool ignoreOtherReservations2 = ignoreOtherReservations;
                        result = (p2.CanReserve(target, 1, -1, null, ignoreOtherReservations2) ? 1 : 0);
                    }
                    else
                    {
                        result = 0;
                    }
                    return (byte)result != 0;
                });
                if (building_MutagenChamber != null)
                {
                    return building_MutagenChamber;
                }
            }
            return null;
        }
    }
    [DefOf]
    public static class Mutagen_JobDefOf
    {
        public static JobDef CarryToMutagenChamber;
        public static JobDef EnterMutagenChamber;
    }

    [StaticConstructorOnStartup]
    internal static class PawnmorphPatches
    {
        static PawnmorphPatches()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("com.BioReactor.rimworld.mod");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    [HarmonyPatch(typeof(FloatMenuMakerMap)), HarmonyPatch("AddHumanlikeOrders")]
    internal class FloatMenuMakerMapPatches
    {
        [HarmonyPrefix]
        static bool Prefix_AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                foreach (LocalTargetInfo localTargetInfo3 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), true))
                {
                    LocalTargetInfo localTargetInfo4 = localTargetInfo3;
                    Pawn victim = (Pawn)localTargetInfo4.Thing;
                    if (victim.Downed && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true) && Building_MutagenChamber.FindCryptosleepCasketFor(victim, pawn, true) != null)
                    {
                        string text4 = "CarryToChamber".Translate(localTargetInfo4.Thing.LabelCap, localTargetInfo4.Thing);
                        JobDef jDef = Mutagen_JobDefOf.CarryToMutagenChamber;
                        Action action3 = delegate ()
                        {
                            Building_MutagenChamber building_chamber = Building_MutagenChamber.FindCryptosleepCasketFor(victim, pawn, false);
                            if (building_chamber == null)
                            {
                                building_chamber = Building_MutagenChamber.FindCryptosleepCasketFor(victim, pawn, true);
                            }
                            if (building_chamber == null)
                            {
                                Messages.Message("CannotCarryToChamber".Translate() + ": " + "NoChamber".Translate(), victim, MessageTypeDefOf.RejectInput, false);
                                return;
                            }
                            Job job = new Job(jDef, victim, building_chamber);
                            job.count = 1;
                            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        };
                        string label = text4;
                        Action action2 = action3;
                        Pawn revalidateClickTarget = victim;
                        opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2, MenuOptionPriority.Default, null, revalidateClickTarget, 0f, null, null), pawn, victim, "ReservedBy"));
                    }
                }
            }
            return true;
        }
    }

}