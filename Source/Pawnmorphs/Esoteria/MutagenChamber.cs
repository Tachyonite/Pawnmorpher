using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using AlienRace;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
using Harmony;
using Pawnmorph.Hybrids;
using Pawnmorph.Thoughts;
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
        public bool doNotEject = false;
        private string hediffDef = "TransformedHuman";
        private CompRefuelable fuelComp = null;
        private CompPowerTrader powerComp = null;
        private CompFlickable flickComp = null;
        public Building_MutagenChamber linkTo;

        public Building_MutagenChamber()
        {
            EnterMutagenChamber = DefDatabase<JobDef>.GetNamed("EnterMutagenChamber");
        }

        public override string GetInspectString()
        {
            base.GetInspectString();
            StringBuilder stringBuilder = new StringBuilder();
            string inspectString = base.GetInspectString();

            if (this.modulator != null)
            {
                if (this.modulator.merging && this != this.modulator.Chambers.First())
                {
                    stringBuilder.AppendLine("MutagenChamberProgress".Translate() + ": Merging with linked pod");
                }
                else
                {
                    if (this.modulator.random)
                    {
                        stringBuilder.AppendLine("MutagenChamberProgress".Translate() + ": " + daysIn.ToStringPercent() + " ???");
                    }
                    else
                    {
                        stringBuilder.AppendLine("MutagenChamberProgress".Translate() + ": " + daysIn.ToStringPercent() + " " + pawnTFKind.LabelCap);
                    }
                }
            }
            else
            {
                stringBuilder.AppendLine("MutagenChamberProgress".Translate() + ": " + daysIn.ToStringPercent() + " ???");
            }
            stringBuilder.AppendLine("Slurry contained: " + fuelComp.Fuel + "/" + fuelComp.TargetFuelLevel);
            stringBuilder.AppendLine("(requires " + fuelComp.TargetFuelLevel + " to begin)");

            return stringBuilder.ToString().TrimEndNewlines();
            
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
            flickComp = this.GetComp<CompFlickable>();
        }

        public override void Tick()
        {
            int tickMult = 60;

            if (this.IsHashIntervalTick(60)) { 
                float num = 1f / (daysToIncubate * 60000f);
                foreach (Thing item in (IEnumerable<Thing>)innerContainer)
                {
                    Pawn pawn = item as Pawn;
                    if ((!fuelComp.HasFuel || !powerComp.PowerOn) || fuelComp.FuelPercentOfMax != 1f)
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
                    {
                        this.modulator.triggered = true;
                        if (this.modulator.random)
                        {
                            PickRandom();
                        }
                        else
                        {
                            PickRandom();
                        }
                    }
                    daysIn = 0;
                }
                else if (this.modulator != null)
                {
                    if (doNotEject && this.modulator.triggered)
                    {
                        fuelComp.ConsumeFuel(fuelComp.Fuel);
                        daysIn = 0;
                        innerContainer.ClearAndDestroyContentsOrPassToWorld();
                        this.modulator.triggered = false;
                    }
                }
                
            }
        }

        public void PickRandom()
        {
            IEnumerable<PawnKindDef> pks = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.race.baseBodySize <= 2.9f && x.race.race.intelligence == Intelligence.Animal && x.race.race.FleshType == FleshTypeDefOf.Normal && (x.label.StartsWith("chao") && x.label != "chaomeld" && x.label != "chaofusion"));
            IEnumerable<PawnKindDef> pks2 = Find.World.GetComponent<PawnmorphGameComp>().taggedAnimals.ToArray();
            pawnTFKind = pks.Concat(pks2).RandomElement();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref this.daysIn, "daysIn");
            Scribe_Values.Look(ref this.doNotEject, "doNotEject");
            Scribe_Defs.Look(ref pawnTFKind, "pawnTFKind");
            Scribe_References.Look(ref this.modulator, "modulator");
            Scribe_References.Look(ref this.linkTo, "linkTo");
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

                    float humanLE = pawn.def.race.lifeExpectancy;
                    float animalLE = pawnTFKind.race.race.lifeExpectancy;
                    float humanAge = pawn.ageTracker.AgeBiologicalYears;

                    float animalDelta = humanAge / humanLE;
                    float animalAge = animalLE * animalDelta;

                    Pawn pawnTF = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnTFKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, new float?(animalAge), new float?(pawn.ageTracker.AgeChronologicalYearsFloat), new Gender?(newGender), null, null));
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
                            var first = (Pawn)this.linkTo.innerContainer.First();
                            var second = (Pawn)this.innerContainer.First();

                            ReactionsHelper.OnPawnsMerged(first, first.IsPrisoner, second, second.IsPrisoner, pawn3); 

                            PawnMorphInstanceMerged pm = new PawnMorphInstanceMerged(first, second, pawn3);
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
                        IEnumerable<PawnKindDef> pks = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.race.baseBodySize <= 2.9f && x.race.race.intelligence == Intelligence.Animal && x.race.race.FleshType == FleshTypeDefOf.Normal && (x.label.ToLower().StartsWith("chao") && x.label.ToLower() != "chaomeld" && x.label.ToLower() != "chaofusion"));
                        IEnumerable<PawnKindDef> pks2 = Find.World.GetComponent<PawnmorphGameComp>().taggedAnimals.ToArray();
                        pks = pks.Concat(pks2);
                    }
                    
                    Find.TickManager.slower.SignalForceNormalSpeedShort();
                    PawnComponentsUtility.AddComponentsForSpawn(pawn3);
                    pawn.ownership.UnclaimAll();
                    if (this.modulator != null)
                    {
                        this.modulator.triggered = true;
                        if (this.modulator.merging)
                        {
                            this.modulator.merging = false;
                            this.modulator.random = true;
                        }
                        
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
                pawn.ownership.UnclaimAll();
                pawn.Destroy();
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
                    if (!((Building_MutagenChamber)x).HasAnyContents && ((Building_MutagenChamber)x).flickComp.SwitchIsOn)
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
        private static readonly Type patchType = typeof(PawnmorphPatches);
        static PawnmorphPatches()
        {
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("com.BioReactor.rimworld.mod"); //shouldn't this be different? 
            harmonyInstance.Patch(original: AccessTools.Method(type: typeof(FoodUtility), name: nameof(FoodUtility.ThoughtsFromIngesting)),
                 prefix: null,
                 postfix: new HarmonyMethod(type: patchType, name: nameof(ThoughtsFromIngestingPostfix)));


            //untested changes below 
            //trying to change the preferability of the morphs, so they'll eat raw food before starving themselves (only really effects the warg morph) 
#if false
            harmonyInstance.Patch( AccessTools.Method(typeof(CaravanPawnsNeedsUtility), nameof(CaravanPawnsNeedsUtility.GetFoodScore),
                                                      new Type[]
                                                      {
                                                          typeof(ThingDef), typeof(Pawn), typeof(float)
                                                      })
                                  , null, new HarmonyMethod(patchType, nameof(GetFoodScoreDefPostfix)));
            harmonyInstance
               .Patch(AccessTools.Method(typeof(CaravanPawnsNeedsUtility), nameof(CaravanPawnsNeedsUtility.CanEatForNutritionEver)),
                      null, new HarmonyMethod(patchType, nameof(CanEatForNutritionEverPostfix)));
            harmonyInstance.Patch(AccessTools.Method(typeof(CaravanPawnsNeedsUtility),
                                                     nameof(CaravanPawnsNeedsUtility.CanEatForNutritionNow),
                                                     new Type[]
                                                     {
                                                         typeof(ThingDef), typeof(Pawn)
                                                     }),
                                  null, new HarmonyMethod(patchType, nameof(CanEatForNutritionNowDefPostfix)));

            harmonyInstance.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.WillEat), new[]
            {
                typeof(Pawn), typeof(Thing), typeof(Pawn)
            }), null, new HarmonyMethod(patchType, nameof(WillEatThingPostfix)));

            harmonyInstance.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.WillEat), new[]
            {
                typeof(Pawn), typeof(ThingDef), typeof(Pawn)
            }), null, new HarmonyMethod(patchType, nameof(WillEatDefPostfix)));

            harmonyInstance.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality)),
                                  null, new HarmonyMethod(patchType, nameof(FoodOptimalityPostfix)));

            harmonyInstance.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap)),
                                  new HarmonyMethod(patchType, nameof(BestFoodSourceOnMapPrefix))); 

#endif
          

            //end untested changes 

            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }



        public static void ThoughtsFromIngestingPostfix(Pawn ingester, Thing foodSource, ref List<ThoughtDef> __result)
        {
            if (Hybrids.RaceGenerator.TryGetMorphOfRace(ingester.def, out MorphDef morphDef))
            {
                AteThought cannibalThought = morphDef.raceSettings?.thoughtSettings?.ateAnimalThought;  
                if (cannibalThought == null) return;
                bool cannibal = ingester.story.traits.HasTrait(TraitDefOf.Cannibal);

                
                
                if (foodSource.def == morphDef.race.race.meatDef && !cannibal)
                {
                    __result.Add(ThoughtDef.Named(cannibalThought.thought));
                    return; 
                }

                var comp = foodSource.TryGetComp<CompIngredients>()
                                    ?.ingredients?.FirstOrDefault(def => def == morphDef.race.race.meatDef);
                if (comp != null && !cannibal)
                {
                    __result.Add(ThoughtDef.Named(cannibalThought.ingredientThought)); 
                }



            }



        }

        public static void GetFoodScoreDefPostfix(ThingDef food, Pawn pawn, float singleFoodNutrition, ref float __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(pawn.def, out MorphDef morph))
            {
                var prefOverride = morph.GetOverride(food);
                if (prefOverride != null)
                {
                    __result = (float) prefOverride.Value; 
                }
            }


            
        }

        public static void CanEatForNutritionEverPostfix(ThingDef food, Pawn pawn, ref bool __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(pawn.def, out MorphDef def))
            {

                var pref = pawn.GetOverride(food) ?? food.ingestible.preferability;
                __result =  food.IsNutritionGivingIngestible
                         && pawn.WillEat(food)
                         && pref > FoodPreferability.NeverForNutrition
                         && (!food.IsDrug || pawn.IsTeetotaler());
                
            }



        }

        public static void CanEatForNutritionNowDefPostfix(ThingDef food, Pawn pawn, ref bool __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(pawn.def, out MorphDef def))
            {
                __result = CaravanPawnsNeedsUtility.CanEatForNutritionEver(food, pawn); 
            }
        }


        public static void WillEatThingPostfix(Pawn p, Thing food, Pawn getter, ref bool __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(p.def, out MorphDef def))
            {

                if (!def.race.race.CanEverEat(food))
                {
                    __result = false;
                    return; 
                }
                if (p.foodRestriction != null)
                {
                    FoodRestriction currentRespectedRestriction = p.foodRestriction.GetCurrentRespectedRestriction(getter);
                    if (currentRespectedRestriction != null && !currentRespectedRestriction.Allows(food) && (food.def.IsWithinCategory(ThingCategoryDefOf.Foods)))
                    {
                        __result =  false;
                        return; 
                    }
                }

                __result = true; 

            }
        }


        public static void WillEatDefPostfix(Pawn p, ThingDef food, Pawn getter, ref bool __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(p.def, out MorphDef def))
            {
                if (!def.race.race.CanEverEat(food))
                {
                    __result = false;
                    return; 
                }

                if (p.foodRestriction != null)
                {
                    FoodRestriction currentRespectedRestriction = p.foodRestriction.GetCurrentRespectedRestriction(getter);
                    if (currentRespectedRestriction != null && !currentRespectedRestriction.Allows(food) && (food.IsWithinCategory(ThingCategoryDefOf.Foods)))
                    {
                        __result = false;
                        return; 
                    }
                }

                __result = true; 

            }


        }

        public static void FoodOptimalityPostfix(Pawn eater, Thing foodSource, ThingDef foodDef, float dist,
                                                 bool takingToInventory, ref float __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(eater.def, out MorphDef def))
            {
                var fOverride = def.GetOverride(foodDef);
                if (fOverride != null)
                    __result = FoodOptimality(eater, foodSource, foodDef, dist, takingToInventory, def, fOverride.Value); 
            }


        }

        static float FoodOptimality(Pawn eater, Thing foodSource, ThingDef foodDef, float dist, bool takingToInventory,
                                   MorphDef morph, FoodPreferability fOverride)
        {
            float num = 300f;
            num -= dist;
            FoodPreferability preferability = fOverride;

            //var fOverride = morph.GetOverride(foodDef);
          

            //preferability = fOverride ?? preferability; 

            if (preferability != FoodPreferability.NeverForNutrition)
            {
                if (preferability != FoodPreferability.DesperateOnly)
                {
                    if (preferability == FoodPreferability.DesperateOnlyForHumanlikes)
                    {
                        if (eater.RaceProps.Humanlike)
                        {
                            num -= 150f;
                        }
                    }
                }
                else
                {
                    num -= 150f;
                }
                CompRottable compRottable = foodSource.TryGetComp<CompRottable>();
                if (compRottable != null)
                {
                    if (compRottable.Stage == RotStage.Dessicated)
                    {
                        return -9999999f;
                    }
                    if (!takingToInventory && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
                    {
                        num += 12f;
                    }
                }
                if (eater.needs != null && eater.needs.mood != null)
                {
                    List<ThoughtDef> list = FoodUtility.ThoughtsFromIngesting(eater, foodSource, foodDef);
                    for (int i = 0; i < list.Count; i++)  
                    {
                        num += FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect); 
                    }
                }
                if (foodDef.ingestible != null)
                {
                    
                    
                        num += foodDef.ingestible.optimalityOffsetFeedingAnimals;
                    
                }

                var diff = 5.5f * ((int) fOverride - (int) foodDef.ingestible.preferability); //constant is a guess at how large the optimality should be 
                num += FoodOptimalityEffectFromMoodCurve.Evaluate(diff); 
                Log.Message($"{eater.Name} has preferability for {foodDef.defName} of {num}");
            

               

                

                return num;
            }
            return -9999999f;



            
        }

        static bool BestFoodSourceOnMapPrefix(
            Pawn getter,
            Pawn eater,
            bool desperate,
            ref ThingDef foodDef,
            FoodPreferability maxPref,
            bool allowPlant,
            bool allowDrug,
            bool allowCorpse,
            bool allowDispenserFull,
            bool allowDispenserEmpty,
            bool allowForbidden,
            bool allowSociallyImproper,
            bool allowHarvest,
            bool forceScanWholeMap,
            ref Thing __result)
        {

            if (!RaceGenerator.TryGetMorphOfRace(eater.def, out MorphDef def))
            {
                return true; 
            }

            foodDef = (ThingDef)null;
            bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            if (!getterCanManipulate && getter != eater)
            {
                return true; 
            }
            FoodPreferability minPref = !eater.NonHumanlikeOrWildMan() ? (!desperate ? (eater.needs.food.CurCategory < HungerCategory.UrgentlyHungry ? FoodPreferability.MealAwful : FoodPreferability.RawBad) : FoodPreferability.DesperateOnly) : FoodPreferability.NeverForNutrition;
            Predicate<Thing> foodValidator = (Predicate<Thing>)(t =>
            {
                Building_NutrientPasteDispenser nutrientPasteDispenser = t as Building_NutrientPasteDispenser;
                if (nutrientPasteDispenser != null)
                {
                    if (!allowDispenserFull || !getterCanManipulate || (ThingDefOf.MealNutrientPaste.ingestible.preferability < minPref || ThingDefOf.MealNutrientPaste.ingestible.preferability > maxPref) || (!eater.WillEat(ThingDefOf.MealNutrientPaste, getter) || t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter) || !nutrientPasteDispenser.powerComp.PowerOn || (!allowDispenserEmpty && !nutrientPasteDispenser.HasEnoughFeedstockInHoppers() || (!t.InteractionCell.Standable(t.Map) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)))) || (getter.IsWildMan() || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map, false), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false))))
                        return false;
                }
                else
                {
                    FoodPreferability ingestiblePreferability =  def.GetOverride(t.def) ?? t.def.ingestible.preferability;
                    


                    if (ingestiblePreferability < minPref || ingestiblePreferability > maxPref || (!eater.WillEat(t, getter) || !t.def.IsNutritionGivingIngestible) || (!t.IngestibleNow || !allowCorpse && t is Corpse) || (!allowDrug && t.def.IsDrug || !allowForbidden && t.IsForbidden(getter) || (!desperate && t.IsNotFresh() || (t.IsDessicated() || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)))) || (!getter.AnimalAwareOf(t) && !forceScanWholeMap || !getter.CanReserve((LocalTargetInfo)t, 1, -1, (ReservationLayerDef)null, false)))
                        return false;
                }

                return true;
            });
            ThingRequest req = (eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) == FoodTypeFlags.None || !allowPlant ? ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree) : ThingRequest.ForGroup(ThingRequestGroup.FoodSource);
            Thing bestThing;

            bestThing = SpawnedFoodSearchInnerScan(eater, getter.Position, getter.Map.listerThings.ThingsMatching(req), PathEndMode.ClosestTouch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, foodValidator);
            if (allowHarvest && getterCanManipulate)
            {
                int searchRegionsMax = !forceScanWholeMap || bestThing != null ? 30 : -1;
                Thing foodSource = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForGroup(ThingRequestGroup.HarvestablePlant), PathEndMode.Touch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, (Predicate<Thing>)(x =>
                {
                    Plant t = (Plant)x;
                    if (!t.HarvestableNow)
                    {  return false;}
                    ThingDef harvestedThingDef = t.def.plant.harvestedThingDef;
                    return harvestedThingDef.IsNutritionGivingIngestible && eater.WillEat(harvestedThingDef, getter) && getter.CanReserve((LocalTargetInfo)((Thing)t), 1, -1, (ReservationLayerDef)null) && ((allowForbidden || !t.IsForbidden(getter)) && (bestThing == null || FoodUtility.GetFinalIngestibleDef(bestThing, false).ingestible.preferability < harvestedThingDef.ingestible.preferability));
                }), (IEnumerable<Thing>)null, 0, searchRegionsMax, false, RegionType.Set_Passable, false);
                if (foodSource != null)
                {
                    bestThing = foodSource;
                    foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, true);
                }
            }
            if (foodDef == null && bestThing != null)
                foodDef = FoodUtility.GetFinalIngestibleDef(bestThing, false);

            __result = bestThing;
            return false; 
        }
        private static bool IsFoodSourceOnMapSociallyProper(
            Thing t,
            Pawn getter,
            Pawn eater,
            bool allowSociallyImproper)
        {
            if (!allowSociallyImproper)
            {
                bool animalsCare = !getter.RaceProps.Animal;
                if (!t.IsSociallyProper(getter) && !t.IsSociallyProper(eater, eater.IsPrisonerOfColony, animalsCare))
                    return false;
            }
            return true;
        }
        private static Thing SpawnedFoodSearchInnerScan(
            Pawn eater,
            IntVec3 root,
            List<Thing> searchSet,
            PathEndMode peMode,
            TraverseParms traverseParams,
            float maxDistance = 9999f,
            Predicate<Thing> validator = null)
        {
            if (searchSet == null)
                return (Thing)null;
            Pawn pawn = traverseParams.pawn ?? eater;
            int num1 = 0;
            int num2 = 0;
            Thing thing = (Thing)null;
            float num3 = float.MinValue;
            for (int index = 0; index < searchSet.Count; ++index)
            {
                Thing search = searchSet[index];
                ++num2;
                float lengthManhattan = (float)(root - search.Position).LengthManhattan;
                if ((double)lengthManhattan <= (double)maxDistance)
                {
                    float num4 = FoodUtility.FoodOptimality(eater, search, FoodUtility.GetFinalIngestibleDef(search, false), lengthManhattan, false);
                    if ((double)num4 >= (double)num3 && pawn.Map.reachability.CanReach(root, (LocalTargetInfo)search, peMode, traverseParams) && search.Spawned && (validator == null || validator(search)))
                    {
                        thing = search;
                        num3 = num4;
                        ++num1;
                    }
                }
            }
            return thing;
        }

        private static readonly SimpleCurve FoodOptimalityEffectFromMoodCurve = new SimpleCurve
        {
            {
                new CurvePoint(-100f, -600f),
                true
            },
            {
                new CurvePoint(-10f, -100f),
                true
            },
            {
                new CurvePoint(-5f, -70f),
                true
            },
            {
                new CurvePoint(-1f, -50f),
                true
            },
            {
                new CurvePoint(0f, 0f),
                true
            },
            {
                new CurvePoint(100f, 800f),
                true
            }
        };


        static FoodPreferability? GetOverride(this Pawn pawn, ThingDef food)
        {
            if (RaceGenerator.TryGetMorphOfRace(pawn.def, out MorphDef morph))
            {
                var prefOverride = morph.GetOverride(food);
                return prefOverride; 
            }

            return null; 
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
                    if (victim.RaceProps.intelligence == Intelligence.Humanlike && pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, true) && Building_MutagenChamber.FindCryptosleepCasketFor(victim, pawn, true) != null)
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