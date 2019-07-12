using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<Thing> LinkedFacilities => CompLinked.LinkedFacilitiesListForReading;

        public List<Building_MutagenChamber> Chambers => LinkedFacilities.Cast<Building_MutagenChamber>().ToList();

        public Building_MutagenModulator()
        {
            
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = this.GetComp<CompPowerTrader>();
            flickableComp = this.GetComp<CompFlickable>();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (Chambers.Count() > 0)
            {
                foreach (Building_MutagenChamber c in Chambers)
                {

                    c.modulator = null;

                }
            }
            base.Destroy(mode);

        }
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (Chambers.Count() > 0)
            {
                foreach (Building_MutagenChamber c in Chambers)
                {

                    c.modulator = null;

                }
            }
            base.DeSpawn(mode);
        }

        public override string GetInspectString()
        {
            base.GetInspectString();
            if (Chambers.Count > 0 && !this.random)
            {
                return "Setting: " + Chambers.First().pawnTFKind.LabelCap;
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
            if (Chambers.Count() > 0)
            {
                foreach (Building_MutagenChamber c in Chambers)
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
                    { if (this.Chambers.First().daysIn > 0f || this.Chambers.Last().daysIn > 0f)
                        {
                            if (this.Chambers.First() != this.Chambers.Last())
                            {
                                if (Chambers.All(x => x.ContainedThing != null) && Chambers.Count > 1)
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

            if (Chambers.First().def == ThingDef.Named("BigMutagenicChamber"))
            {
                maxBodySize = 5.0f;
            }
            else
            {
                maxBodySize = 2.5f;
            }

            IEnumerable<PawnKindDef> pks = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.race.race.baseBodySize <= maxBodySize && x.race.race.intelligence == Intelligence.Animal && x.race.race.FleshType == FleshTypeDefOf.Normal && (x.label != "chaomeld" && x.label != "chaofusion"));

            if (Chambers.All(x => x.ContainedThing != null) && Chambers.Count > 1)
            {
                void Action()
                {
                    Chambers.First().pawnTFKind = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.label == "chaomeld").RandomElement();
                    Chambers.Last().pawnTFKind = null;
                    Chambers.Last().doNotEject = true;
                    Chambers.First().linkTo = Chambers.Last();
                    Chambers.Last().linkTo = Chambers.First();
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
                        foreach (Building_MutagenChamber chamber in Chambers)
                        {
                            chamber.pawnTFKind = pk;
                            Chambers.First().linkTo = null;
                            Chambers.Last().linkTo = null;
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
