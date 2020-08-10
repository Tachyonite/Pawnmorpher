using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.Chambers;
using Pawnmorph.DebugUtils;
using Pawnmorph.TfSys;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
#pragma warning disable 1591 //this is going to be re worked, disabling for now 
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
        public PawnKindDef pawnTFKind;
        public bool doNotEject = false;
        private CompRefuelable fuelComp = null;
        private CompPowerTrader powerComp = null;
        private CompFlickable flickComp = null;
        public Building_MutagenChamber linkTo;

        private ChamberState _state;
        public override void Draw()
        {
            
            //TODO draw pawn
            Comps_PostDraw();
        }

        public Building_MutagenChamber()
        {
            pawnTFKind = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.race.baseBodySize <= 2.9f && x.race.race.intelligence == Intelligence.Animal && x.race.race.FleshType == FleshTypeDefOf.Normal).RandomElement();

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

                _state = ChamberState.Transforming;
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

            if (this.IsHashIntervalTick(60))
            {
                CheckState();
                DoTick(tickMult);
            }
        }




        private void DoTick(int tickMult)
        {
            float num = 1f / (daysToIncubate * 60000f);

            if (!fuelComp.HasFuel || !powerComp.PowerOn || fuelComp.FuelPercentOfMax < 1f)
                return;
            if (modulator != null)
                if (!modulator.powerComp.PowerOn || !modulator.flickableComp.SwitchIsOn)
                    return;

            Pawn pawn = innerContainer.OfType<Pawn>().FirstOrDefault();
            if (pawn == null)
                return;

            switch (_state)
            {
                case ChamberState.Transforming:
                    daysIn += num * tickMult;
                    break;
                case ChamberState.MergeInto:
                    if (linkTo.fuelComp.IsFull && linkTo.innerContainer.Count > 0)
                        daysIn += num * tickMult;
                    break;
                case ChamberState.MergeOutOf:
                case ChamberState.Idle:
                    daysIn = 0;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (IsFinished)
            {

                if (modulator != null)
                {
                    modulator.NotifyChamberFinished(this); 
                }
                else
                {
                    EjectContents();
                    fuelComp.ConsumeFuel(fuelComp.Fuel);

                }


                //EjectContents();
                //fuelComp.ConsumeFuel(fuelComp.Fuel);
                //if (modulator != null)
                //{
                //    modulator.triggered = true;
                //    if (modulator.random)
                //        PickRandom();
                    
                        
                //}

                daysIn = 0;
            }
           
        }

        public void ClearContents()
        {
            fuelComp.ConsumeFuel(fuelComp.Fuel);
            daysIn = 0;
            innerContainer.ClearAndDestroyContentsOrPassToWorld();
            modulator.triggered = false;
        }

        public void PickRandom()
        {
            var comp = def.GetCompProperties<ThingCompProperties_ModulatorOptions>();
            var defaultAnimals = comp.defaultAnimals;

            var taggedAnimals = Find.World.GetComponent<ChamberDatabase>().TaggedAnimals;
            if (taggedAnimals == null || taggedAnimals.Count == 0)
            {
                pawnTFKind = defaultAnimals.RandElement();
                return;
            }

            var tmpLst = new List<PawnKindDef>(defaultAnimals);
            tmpLst.AddRange(taggedAnimals);

            pawnTFKind = tmpLst.RandElement();
        }

        void CheckState()
        {
            if (innerContainer.Count == 0)
            {
                _state = ChamberState.Idle;
                return;
            }

            if (innerContainer.Count == 1)
            {

                if (modulator?.merging ?? false)
                {
                    _state = modulator.GetLinkedChamber() == this ? ChamberState.MergeInto : ChamberState.MergeOutOf;
                }
            }


        }

        /// <summary>
        /// Notifies this instance that it is merging 
        /// </summary>
        /// <param name="isMasterChamber">if set to <c>true</c> this instance is the master chamber.</param>
        public void NotifyMerging(bool isMasterChamber)
        {
            if(_state != ChamberState.Transforming)
                Log.Warning($"Mutagenic chamber is merging when it isn't transforming already!");
            _state = isMasterChamber ? ChamberState.MergeInto : ChamberState.MergeOutOf; 
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.daysIn, "daysIn");
            Scribe_Values.Look(ref this.doNotEject, "doNotEject");
            Scribe_Defs.Look(ref pawnTFKind, "pawnTFKind");
            Scribe_References.Look(ref this.modulator, "modulator");
            Scribe_References.Look(ref this.linkTo, "linkTo");

            

            //base.ExposeData();
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {


            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(myPawn))
            {
                yield return floatMenuOption;
            }
            if(!MutagenDefOf.MergeMutagen.CanTransform(myPawn))
                yield break;
            
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

        public bool IsFinished => daysIn >= 1; 


        public override void EjectContents() //should refactor the mutagenic chamber, make it a state machine 
        {
            DebugLogUtils.Assert(innerContainer.Count == 1, "innerContainer.Count == 1");

            var pawn = (Pawn) innerContainer[0];
            if (pawn == null)
            {
                Log.Warning($"mutagenic chamber ejecting nothing");

                return;
            }

        

            if (IsFinished)
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


                if (_state != ChamberState.MergeOutOf || (linkTo?.daysIn ?? 0) < 1)
                {
                    pawn.health.AddHediff(Hediffs.MorphTransformationDefOf.FullRandomTFAnyOutcome);

                }

                pawn.health.AddHediff(Hediffs.MorphTransformationDefOf.FullRandomTFAnyOutcome); 
                if(_state == ChamberState.MergeInto)
                    linkTo?.EjectContents();


            }

            _state = ChamberState.Idle;
            daysIn = 0; 
        }

        private void TransformPawn(Pawn pawn)
        {
            TransformationRequest request;
            Mutagen mutagen;

            switch (_state)
            {
              
                case ChamberState.Transforming:
                    request = new TransformationRequest(pawnTFKind, pawn)
                    {
                        forcedGender = TFGender.Switch,
                        forcedGenderChance = 50,
                        manhunterSettingsOverride = ManhunterTfSettings.Never,
                        factionResponsible = Faction,
                        forcedFaction = Faction
                    };

                    mutagen = MutagenDefOf.defaultMutagen.MutagenCached;

                    break;
                case ChamberState.MergeInto:
                    request = new TransformationRequest(pawnTFKind,   pawn, (Pawn) linkTo.innerContainer[0])
                    {
                        forcedGender = TFGender.Switch,
                        forcedGenderChance = 50,
                        manhunterSettingsOverride = ManhunterTfSettings.Never,
                        factionResponsible = Faction,
                        forcedFaction = Faction
                    };
                    mutagen = MutagenDefOf.MergeMutagen.MutagenCached;
                    break;
                case ChamberState.MergeOutOf:
                    return; 
                case ChamberState.Idle:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TransformedPawn pmInst = mutagen.Transform(request);
           
            if (pmInst == null)
            {
                Log.Error($"mutagenic chamber could not transform pawns {string.Join(",",request.originals.Select(p => p.Name.ToStringFull).ToArray())} using mutagen {mutagen.def.defName}");

                return;
            }

            SendLetter(pawn);
            base.EjectContents();
            if(_state == ChamberState.MergeInto)
                linkTo.EjectContents();
            foreach (Pawn pmInstOriginalPawn in pmInst.OriginalPawns)
            {
                if (pmInstOriginalPawn == null) continue;
                TransformerUtility.CleanUpHumanPawnPostTf(pmInstOriginalPawn, null);
            }

            foreach (Pawn pmInstOriginalPawn in pmInst.OriginalPawns)
            {
                pmInstOriginalPawn.DeSpawn(); 
            }

            var comp = Find.World.GetComponent<PawnmorphGameComp>();
            comp.AddTransformedPawn(pmInst); 

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
                    Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationMergeLabel".Translate(pawn.LabelShort, this.linkTo.innerContainer.First().LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), "LetterHediffFromTransformationMerge".Translate(pawn.LabelShort, this.linkTo.innerContainer.First().LabelCap, pawnTFKind.LabelCap).CapitalizeFirst(), LetterDefOf.NeutralEvent, pawn, null, null);
                }
                else
                {
                    Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationLabel".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), "LetterHediffFromTransformation".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), LetterDefOf.NeutralEvent, pawn, null, null);
                }
            }
            else
            {
                Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationLabel".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), "LetterHediffFromTransformation".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), LetterDefOf.NeutralEvent, pawn, null, null);

            }
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