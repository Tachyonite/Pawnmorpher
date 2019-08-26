using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.TfSys;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using Pawnmorph.Thoughts;

namespace Pawnmorph
{
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

        public override void EjectContents() //should refactor the mutagenic chamber, make it a state machine 
        {
            if (innerContainer.Count == 0) return;

            if (innerContainer.Count > 1)
            {
                Log.Error("there is more then 1 pawn in mutagenic chamber? ");
                return;
            }

            var pawn = (Pawn) innerContainer[0];
            if (pawn == null) return;

            if (daysIn > 1)
            {
                TransformPawn(pawn);
            }
            else
            {
                base.EjectContents();
                if (!Destroyed)
                {
                    SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
                    fuelComp.ConsumeFuel(fuelComp.Fuel);
                }

                pawn.health.AddHediff(Hediffs.MorphTransformationDefOf.FullRandomTFAnyOutcome); 
                if(IsMerging)
                    linkTo.EjectContents();


            }

            daysIn = 0; 
        }

        private void TransformPawn(Pawn pawn)
        {
            TransformationRequest request;
            Mutagen mutagen;

            if (IsMerging)
            {
                request = new TransformationRequest(pawnTFKind, pawn, (Pawn) linkTo.innerContainer[0])
                {
                    forcedGenderChance = 50
                };
                mutagen = MutagenDefOf.MergeMutagen.MutagenCached;
            }
            else
            {
                request = new TransformationRequest(pawnTFKind, pawn)
                {
                    forcedGenderChance = 50
                };
                mutagen = MutagenDefOf.defaultMutagen.MutagenCached;
            }

            TransformedPawn pmInst = mutagen.Transform(request);
            if (pmInst == null) return;
            SendLetter(pawn);
            base.EjectContents();
            foreach (Pawn pmInstOriginalPawn in pmInst.OriginalPawns)
            {
                if (pmInstOriginalPawn == null) continue;
                TransformerUtility.CleanUpHumanPawnPostTf(pmInstOriginalPawn, null);
            }


            Find.TickManager.slower.SignalForceNormalSpeedShort();
            PawnComponentsUtility.AddComponentsForSpawn(pmInst.TransformedPawns.First());
            pawn.ownership.UnclaimAll();
            if (modulator != null)
            {
                modulator.triggered = true;
                if (modulator.merging)
                {
                    modulator.merging = false;
                    modulator.random = true;
                }
            }

            if (!Destroyed)
            {
                SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
                fuelComp.ConsumeFuel(fuelComp.Fuel);
            }
        }

        bool IsMerging
        {
            get { return modulator?.merging ?? false;  }
        }

        

        private void SendLetter(Pawn pawn)
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
        }

        private Pawn CreateAnimalPawn(Pawn pawn)
        {
            Gender newGender = GetNewGender(pawn);
            PawnGenerationRequest request = GetPawnRequest(pawn, newGender);
            Pawn pawn3 = SpawnPawn(pawn, request);
            for (int i = 0; i < 10; i++)
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn3.Position.ToVector3(), pawn3.MapHeld);
                IntermittentMagicSprayer.ThrowMagicPuffUp(pawn3.Position.ToVector3(), pawn3.MapHeld);
            }

            int faction = Rand.RangeInclusive(0, 2);

            pawn3.SetFaction(Faction.OfPlayer, null);
            return pawn3;
        }

        private static Pawn SpawnPawn(Pawn pawn, PawnGenerationRequest request)
        {
            Pawn pawnTF = PawnGenerator.GeneratePawn(request);


            pawnTF.needs.food.CurLevel = pawn.needs.food.CurLevel;
            pawnTF.needs.rest.CurLevel = pawn.needs.rest.CurLevel;
            pawnTF.training.SetWantedRecursive(TrainableDefOf.Obedience, true);
            pawnTF.training.Train(TrainableDefOf.Obedience, null, true);
            pawnTF.Name = pawn.Name;
            Pawn pawn3 = (Pawn)GenSpawn.Spawn(pawnTF, pawn.PositionHeld, pawn.MapHeld, 0);
            return pawn3;
        }

        private PawnGenerationRequest GetPawnRequest(Pawn pawn, Gender newGender)
        {
            float humanLE = pawn.def.race.lifeExpectancy;
            float animalLE = pawnTFKind.race.race.lifeExpectancy;
            float humanAge = pawn.ageTracker.AgeBiologicalYears;

            float animalDelta = humanAge / humanLE;
            float animalAge = animalLE * animalDelta;
            var request = new PawnGenerationRequest(pawnTFKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1,
                                                    false, false, false, false, true, false, 1f, false, true, true, false,
                                                    false, false, false, null, null, null, new float?(animalAge),
                                                    new float?(pawn.ageTracker.AgeChronologicalYearsFloat),
                                                    new Gender?(newGender), null, null);
            return request;
        }

        private Gender GetNewGender(Pawn pawn)
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

            return newGender;
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
}