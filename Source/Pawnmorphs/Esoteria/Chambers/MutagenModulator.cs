using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using Multiplayer.API;


namespace Pawnmorph
{

    

    public class Building_MutagenModulator : Building
    {

        public CompAffectedByFacilities compLinked;
        public bool triggered;
        public bool merging = false;
        public bool random = true;
        public float maxBodySize = 2.9f;
        public CompPowerTrader powerComp = null;
        public CompFlickable flickableComp = null;

        public CompAffectedByFacilities CompLinked => compLinked ?? (compLinked = this.GetComp<CompAffectedByFacilities>());
        public IEnumerable<Thing> LinkedFacilities => CompLinked.LinkedFacilitiesListForReading ?? Enumerable.Empty<Thing>();

        /// <summary>
        /// Gets the chambers.
        /// </summary>
        /// This property makes a new list every time it's called! be careful calling it 
        /// <value>
        /// The chambers.
        /// </value>
        public List<Building_MutagenChamber> Chambers => LinkedFacilities.Cast<Building_MutagenChamber>().ToList();


        /// <summary>
        /// Gets the nth linked chamber.
        /// </summary>
        /// <param name="index">The index of the chamber to get</param>
        /// <returns>the nth linked chamber, if one exists. Null otherwise </returns>
        public Building_MutagenChamber GetLinkedChamber(int index=0)
        {
            int counter = 0;
            foreach (Thing thing in CompLinked.LinkedFacilitiesListForReading)
            {
                if (thing is Building_MutagenChamber chamber)
                {
                    if (counter == index) return chamber;
                    counter++; 
                }
            }

            return null; 
        }

        /// <summary>
        /// Gets the last chamber if any are linked 
        /// </summary>
        /// <returns></returns>
        [CanBeNull]
        public Building_MutagenChamber GetLastChamber()
        {
            for (int i = CompLinked.LinkedFacilitiesListForReading.Count - 1; i >= 0; i--)
            {
                if (CompLinked.LinkedFacilitiesListForReading[i] is Building_MutagenChamber chamber) return chamber; 
            }

            return null; 
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = this.GetComp<CompPowerTrader>();
            flickableComp = this.GetComp<CompFlickable>();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            List<Building_MutagenChamber> buildingMutagenChambers = Chambers;
            if (buildingMutagenChambers.Count > 0)
            {
                foreach (Building_MutagenChamber c in buildingMutagenChambers)
                {

                    c.modulator = null;

                }
            }
            base.Destroy(mode);

        }
        
        

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.merging, "merging");
            Scribe_Values.Look(ref this.random, "random");
            
        }
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            List<Building_MutagenChamber> buildingMutagenChambers = Chambers;
            if (buildingMutagenChambers.Count > 0)
            {
                foreach (Building_MutagenChamber c in buildingMutagenChambers)
                {
                    c.EjectContents();
                    c.modulator = null;
                }
            }
            base.DeSpawn(mode);
        }

        public override string GetInspectString()
        {
            base.GetInspectString();
            List<Building_MutagenChamber> buildingMutagenChambers = Chambers;
            if (buildingMutagenChambers.Count > 0 && !this.random)
            {
                return "Setting: " + buildingMutagenChambers.First().pawnTFKind.LabelCap;
            }
            else if (this.random)
            {
                return "Setting: Random";
            }
            else
            {
                return "No chambers allocated. Up to two are allowed.";
            }
        }

        public override void Tick()
        {
            List<Building_MutagenChamber> buildingMutagenChambers = Chambers;
            if (buildingMutagenChambers.Count > 0)
            {
                foreach (Building_MutagenChamber c in buildingMutagenChambers)
                {

                    c.modulator = this;

                }
            }
            base.Tick();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            Command_Action commandAction = new Command_Action
            {
                defaultLabel = "Set Animal",
                defaultDesc = "Set Animal",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Merge", true),
                action = () =>
                {
                    {
                        List<Building_MutagenChamber> chambers = this.Chambers;
                        Building_MutagenChamber firstChamber = chambers.First();
                        Building_MutagenChamber lastChamber = chambers.Last();
                        if (firstChamber.daysIn > 0f || lastChamber.daysIn > 0f)
                        {
                            if (firstChamber != lastChamber)
                            {
                                if (chambers.All(x => x.ContainedThing != null) && chambers.Count > 1)
                                {
                                    Find.WindowStack.Add(new FloatMenu(GenMenuOptions(true)));
                                }
                                else
                                {
                                    Messages.Message("Can't change a morph while there is one in progress.", MessageTypeDefOf.CautionInput);
                                }
                                    
                            }
                            else
                            {
                                Messages.Message("Can't change a morph while there is one in progress.", MessageTypeDefOf.CautionInput);
                            }
                        }
                        else
                        {
                            Find.WindowStack.Add(new FloatMenu(GenMenuOptions()));
                        }
                    }
                }
            };
            
            yield return commandAction;
            
        }

        List<FloatMenuOption> GenMenuOptions(bool merge = false)
        {

            List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();

            List<Building_MutagenChamber> linkedChambers = Chambers;
            Building_MutagenChamber firstChamber = linkedChambers.First();
            if (firstChamber.def == BuildingDefOf.BigMutagenicChamber)
            {
                maxBodySize = 5.0f;
            }
            else
            {
                maxBodySize = 2.9f;
            }
            IEnumerable<PawnKindDef> pks = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.race.baseBodySize <= maxBodySize && x.race.race.intelligence == Intelligence.Animal && x.race.race.FleshType == FleshTypeDefOf.Normal && (x.label.ToLower().StartsWith("chao") && x.label.ToLower() != "chaomeld" && x.label.ToLower() != "chaofusion"));
            IEnumerable<PawnKindDef> pks2 = Find.World.GetComponent<PawnmorphGameComp>().taggedAnimals;
            if (pks2 != null)
            {
                pks2 = pks2.ToArray();
                pks = pks.Concat(pks2);
            }
            

            if (linkedChambers.All(x => x.ContainedThing != null) && linkedChambers.Count > 1)
            {
                void Action()
                {
                    if (maxBodySize == 5.0f)
                    {
                        firstChamber.pawnTFKind = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.label == "chaofusion").RandomElement();
                    }
                    else
                    {
                        firstChamber.pawnTFKind = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.label == "chaomeld").RandomElement();
                    }

                    Building_MutagenChamber lastChamber = linkedChambers.Last();
                    lastChamber.pawnTFKind = null;
                    lastChamber.doNotEject = true;
                    firstChamber.linkTo = lastChamber;
                    lastChamber.linkTo = firstChamber;
                    firstChamber.NotifyMerging(true);
                    lastChamber.NotifyMerging(false); 

                    this.merging = true;
                    this.random = false;
                }

                menuOptions.Add(new FloatMenuOption("Merge Chambers", Action, priority: MenuOptionPriority.High));
            }
            if (!merge)
            {
                foreach (PawnKindDef pk in pks)
                {

                    void Action()
                    {
                        foreach (Building_MutagenChamber chamber in linkedChambers)
                        {
                            chamber.pawnTFKind = pk;
                            chamber.linkTo = null;
                            this.merging = false;
                            this.random = false;
                        }
                    }

                    menuOptions.Add(new FloatMenuOption(pk.LabelCap, Action));
                    
                }
                menuOptions = menuOptions.OrderBy(o=>o.Label).ToList();
            }

            return menuOptions;

        }

    }
}
