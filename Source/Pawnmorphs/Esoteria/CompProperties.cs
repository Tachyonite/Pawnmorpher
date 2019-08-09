using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using RimWorld;
using Multiplayer.API;


namespace Pawnmorph
{
    public class HediffCompStage
    {
        public float daysToProduce = 1;
        public int amount = 1;
        public string resource = "Chemfuel";
        public float chance = 50;
        public string rareResource = "Chemfuel";
        public ThoughtDef thought = null;

    }

    public class HediffComp_Single : HediffComp
    {
        public HediffCompProperties_Single Props
        {
            get
            {
                return (HediffCompProperties_Single)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
        }
    }

    public class HediffCompProperties_Single : HediffCompProperties
    {
        public HediffCompProperties_Single()
        {
            this.compClass = typeof(HediffComp_Single);
        }
    }


    public class Comp_AlwaysFormerHuman : ThingComp
    {
        public HediffCompProperties_AlwaysFormerHuman Props
        {
            get
            {
                return (HediffCompProperties_AlwaysFormerHuman)this.props;
            }
        }

        public override void CompTick()
        {
            HediffDef hediff = HediffDef.Named("TransformedHuman");
            if (this.Props.hediff != null)
            {
                hediff = this.Props.hediff;
            }

            Pawn pawn = this.parent as Pawn;
            if (!pawn.health.hediffSet.HasHediff(HediffDef.Named("PermanentlyFeral")) && !pawn.health.hediffSet.HasHediff(hediff))
            {
                Hediff xhediff = HediffMaker.MakeHediff(hediff, pawn, null);
                xhediff.Severity = Rand.Range(0.00f, 1.00f);
                pawn.health.AddHediff(xhediff, null, null, null);
            }

            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("PermanentlyFeral")) && pawn.health.hediffSet.HasHediff(hediff))
            {
                Hediff xhediff = pawn.health.hediffSet.hediffs.Find(x => x.def == hediff);
                pawn.health.RemoveHediff(xhediff);                
            }            
        }
    }

    public class HediffCompProperties_AlwaysFormerHuman : CompProperties
    {
        public HediffDef hediff;
        public HediffCompProperties_AlwaysFormerHuman()
        {
            this.compClass = typeof(Comp_AlwaysFormerHuman);
        }
    }

    public class Comp_FormerHumanChance : ThingComp
    {
        public CompProperties_FormerHumanChance Props
        {
            get
            {
                return (CompProperties_FormerHumanChance)this.props;
            }
        }
        private bool triggered = false;

        public override void CompTick()
        {
            base.Initialize(props);
            Pawn pawn = this.parent as Pawn;
            if (Rand.RangeInclusive(0, 100) <= this.Props.chance && !triggered && LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableWildFormers && pawn.Faction == null)
            {
                if (!pawn.health.hediffSet.HasHediff(HediffDef.Named("TransformedHuman")) && !pawn.health.hediffSet.HasHediff(HediffDef.Named("PermanentlyFeral")))
                {
                    Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("TransformedHuman"), pawn, null);
                    hediff.Severity = Rand.Range(0.00f, 1.00f);
                    pawn.health.AddHediff(hediff, null, null, null);

                }

                if (pawn.health.hediffSet.HasHediff(HediffDef.Named("PermanentlyFeral")) && pawn.health.hediffSet.HasHediff(HediffDef.Named("TransformedHuman")))
                {
                    Hediff hediff = pawn.health.hediffSet.hediffs.Find(x => x.def == HediffDef.Named("TransformedHuman"));
                    pawn.health.RemoveHediff(hediff);

                }
            }
            triggered = true;
        }
    }

    public class CompProperties_FormerHumanChance : CompProperties
    {
        public float chance = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().formerChance;
        public CompProperties_FormerHumanChance()
        {
            this.compClass = typeof(Comp_FormerHumanChance);
        }
    }


    public class HediffComp_Remove : HediffComp
    {
        public HediffCompProperties_Remove Props
        {
            get
            {
                return (HediffCompProperties_Remove)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            List<Hediff> hS = new List<Hediff>(this.parent.pawn.health.hediffSet.hediffs);

            foreach (Hediff hD in hS)
            {
                if (this.Props.makeImmuneTo.Contains(hD.def))
                {
                    this.parent.pawn.health.RemoveHediff(hD);
                }
            }
        }
    }

    public class HediffCompProperties_Remove : HediffCompProperties
    {
        public List<HediffDef> makeImmuneTo;

        public HediffCompProperties_Remove()
        {
            this.compClass = typeof(HediffComp_Remove);
        }
    }


    public class TerrainBasedMorphComp : HediffComp
    {
        public TerrainBasedMorph Props
        {
            get
            {
                return (TerrainBasedMorph)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (parent.pawn.Position.GetTerrain(parent.pawn.Map) == this.Props.terrain)
            {
                Hediff hediff = HediffMaker.MakeHediff(this.Props.hediffDef, parent.pawn, null);
                hediff.Severity = 1f;
                parent.pawn.health.AddHediff(hediff, null, null, null);
            }
        }
    }

    public class TerrainBasedMorph : HediffCompProperties
    {
        public TerrainBasedMorph()
        {
            this.compClass = typeof(TerrainBasedMorphComp);
        }

        public HediffDef hediffDef = null;

        public TerrainDef terrain = null;
    }

    public class HediffCompProperties_AddSeverity : HediffCompProperties
    {
        public HediffCompProperties_AddSeverity()
        {
            this.compClass = typeof(HediffComp_AddSeverity);
        }

        public HediffDef hediff = null;

        public float severity = 0;

        public float mtbDays = 0;
    }

    public class HediffComp_AddSeverity : HediffComp
    {
        public HediffCompProperties_AddSeverity Props
        {
            get
            {
                return (HediffCompProperties_AddSeverity)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            AddSeverity();
        }

        public void AddSeverity()
        {

            if (Rand.MTBEventOccurs(this.Props.mtbDays, 60000f, 60f) && !triggered && this.parent.pawn.health.hediffSet.HasHediff(this.Props.hediff))
            {
                HealthUtility.AdjustSeverity(this.parent.pawn, this.Props.hediff, this.Props.severity);
                triggered = true;
            }
        }

        private bool triggered = false;
    }

    public class HediffCompProperties_Production : HediffCompProperties
    {
        public float daysToProduce = 1f;
        public int amount = 1;
        public int chance = 0;
        public ThoughtDef thought = null;
        public ThoughtDef wrongGenderThought = null;
        public ThoughtDef etherBondThought = null;
        public ThoughtDef etherBrokenThought = null;
        public Gender genderAversion = Gender.None;
        public string resource = "Chemfuel";
        public string rareResource = "Chemfuel";
        public List<HediffCompStage> stages = null;

        public HediffCompProperties_Production()
        {
            this.compClass = typeof(HediffComp_Production);
        }
    }


    public class HediffComp_Production : HediffComp
    {
        private int HatchingTicker = 0;
        private float brokenChance = 0f;
        private float bondChance = 0f;

        public HediffCompProperties_Production Props
        {
            get
            {
                return (HediffCompProperties_Production)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if(this.Props.stages != null){
                ProduceStaged();
            }
            else {
                Produce();
            }            
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref HatchingTicker, "hatchingTicker");
            Scribe_Values.Look(ref brokenChance, "brokenChance");
            Scribe_Values.Look(ref bondChance, "bondChance");
            base.CompExposeData();
        }

        public void ProduceStaged()
        {
            var hediff = this.parent;
            var index = hediff.CurStageIndex;
            var selectStage = this.Props.stages.ElementAt(index);
            var useThought = this.Props.thought;
            if (selectStage.thought != null) {
                useThought = selectStage.thought;
            }

            if (HatchingTicker < (selectStage.daysToProduce * 60000))
            {
                HatchingTicker += 1;
            }
            else
            {
                if ((this.parent.pawn.Map != null) && ((this.parent.pawn.Faction == Faction.OfPlayer) || ((this.parent.pawn.IsPrisoner) && (this.parent.pawn.Map.IsPlayerHome))))
                {
                    for (int i = 0; i < selectStage.amount; i++)
                    {
                        if (Rand.RangeInclusive(0, 100) <= (selectStage.chance))
                        {
                            GenSpawn.Spawn(ThingDef.Named(selectStage.rareResource), this.parent.pawn.Position, this.parent.pawn.Map);
                        }
                        else {
                            GenSpawn.Spawn(ThingDef.Named(selectStage.resource), this.parent.pawn.Position, this.parent.pawn.Map);
                        }

                        if (selectStage.thought != null)
                        {
                            this.parent.pawn.needs.mood.thoughts.memories.TryGainMemory(useThought);
                        }

                        else if (selectStage.resource != null && (!this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBroken")) || !this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBond"))))
                        {
                            if (this.Props.genderAversion == this.parent.pawn.gender)
                            {
                                this.parent.pawn.needs.mood.thoughts.memories.TryGainMemory(this.Props.wrongGenderThought);
                            }
                            else
                            {
                                this.parent.pawn.needs.mood.thoughts.memories.TryGainMemory(useThought);
                            }
                            brokenChance += 0.5f;
                            bondChance += 0.2f;
                        }
                        else if (this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBond")))
                        {
                            this.parent.pawn.needs.mood.thoughts.memories.TryGainMemory(this.Props.etherBondThought);
                        }

                        else if (this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBroken")))
                        {
                            this.parent.pawn.needs.mood.thoughts.memories.TryGainMemory(this.Props.etherBrokenThought);
                        }
                        else if (Rand.RangeInclusive(0, 100) <= brokenChance && (!this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBroken")) && !this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBond"))))
                        {
                            this.parent.pawn.health.AddHediff(HediffDef.Named("EtherBroken"));
                            Find.LetterStack.ReceiveLetter("LetterHediffFromEtherBrokenLabel".Translate(this.parent.pawn).CapitalizeFirst(), "LetterHediffFromEtherBroken".Translate(this.parent.pawn).CapitalizeFirst(), LetterDefOf.NeutralEvent, this.parent.pawn, null, null);

                        }
                        else if (Rand.RangeInclusive(0, 100) <= bondChance && (!this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBroken")) && !this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBond"))))
                        {
                            this.parent.pawn.health.AddHediff(HediffDef.Named("EtherBond"));
                            Find.LetterStack.ReceiveLetter("LetterHediffFromEtherBondLabel".Translate(this.parent.pawn).CapitalizeFirst(), "LetterHediffFromEtherBond".Translate(this.parent.pawn).CapitalizeFirst(), LetterDefOf.NeutralEvent, this.parent.pawn, null, null);
                        }
                    }
                }
                HatchingTicker = 0;
            }
        }

        public void Produce()
        {
            if (HatchingTicker < (this.Props.daysToProduce * 60000))
            {
                HatchingTicker += 1;
            }
            else
            {
                if ((this.parent.pawn.Map != null) && ((this.parent.pawn.Faction == Faction.OfPlayer) || ((this.parent.pawn.IsPrisoner) && (this.parent.pawn.Map.IsPlayerHome))))
                {
                    for (int i = 0; i < this.Props.amount; i++)
                    {
                        if (Rand.RangeInclusive(0,100) <= (this.Props.chance))
                        {

                            GenSpawn.Spawn(ThingDef.Named(this.Props.rareResource), this.parent.pawn.Position, this.parent.pawn.Map);
                        }
                        else { GenSpawn.Spawn(ThingDef.Named(this.Props.resource), this.parent.pawn.Position, this.parent.pawn.Map); }

                        if (this.Props.resource != null && (!this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBroken")) || !this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBond"))))
                        {
                            if (this.Props.genderAversion == this.parent.pawn.gender)
                            {
                                this.parent.pawn.needs.mood.thoughts.memories.TryGainMemory(this.Props.wrongGenderThought);
                            }
                            else
                            {
                                this.parent.pawn.needs.mood.thoughts.memories.TryGainMemory(this.Props.thought);
                            }
                            brokenChance += 0.5f;
                            bondChance += 0.2f;
                        }
                        else if (this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBond")))
                        {
                            this.parent.pawn.needs.mood.thoughts.memories.TryGainMemory(this.Props.etherBondThought);
                        }
                        else if (this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBroken")))
                        {
                            this.parent.pawn.needs.mood.thoughts.memories.TryGainMemory(this.Props.etherBrokenThought);
                        }

                        if (Rand.RangeInclusive(0,100) <= brokenChance && (!this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBroken")) && !this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBond"))))
                        {
                            this.parent.pawn.health.AddHediff(HediffDef.Named("EtherBroken"));
                            Find.LetterStack.ReceiveLetter("LetterHediffFromEtherBrokenLabel".Translate(this.parent.pawn).CapitalizeFirst(), "LetterHediffFromEtherBroken".Translate(this.parent.pawn).CapitalizeFirst(), LetterDefOf.NeutralEvent, this.parent.pawn, null, null);
                        }
                        if (Rand.RangeInclusive(0, 100) <= bondChance && (!this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBroken")) && !this.parent.pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBond"))))
                        {
                            this.parent.pawn.health.AddHediff(HediffDef.Named("EtherBond"));
                            Find.LetterStack.ReceiveLetter("LetterHediffFromEtherBondLabel".Translate(this.parent.pawn).CapitalizeFirst(), "LetterHediffFromEtherBond".Translate(this.parent.pawn).CapitalizeFirst(), LetterDefOf.NeutralEvent, this.parent.pawn, null, null);
                        }
                    }
                }
                HatchingTicker = 0;
            }
            //this.parent.Destroy(DestroyMode.Vanish);
        }
    }
}
