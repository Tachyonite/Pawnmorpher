using System.Linq;
using RimWorld;
using Verse;

namespace Pawnmorph
{
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
            if (Props.stages != null)
            {
                HediffComp_Staged stage = Props.stages.ElementAt(parent.CurStageIndex);
                Produce(stage.daysToProduce, stage.amount, stage.chance, ThingDef.Named(stage.resource), ThingDef.Named(stage.rareResource), stage.thought);
            }
            else
            {
                Produce(Props.daysToProduce, Props.amount, Props.chance, ThingDef.Named(Props.resource), ThingDef.Named(Props.rareResource));
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref HatchingTicker, "hatchingTicker");
            Scribe_Values.Look(ref brokenChance, "brokenChance");
            Scribe_Values.Look(ref bondChance, "bondChance");
            base.CompExposeData();
        }

        public void Produce(float daysToProduce, int amount, float chance, ThingDef resource, ThingDef rareResource, ThoughtDef stageThought = null)
        {
            MemoryThoughtHandler thoughts = Pawn.needs.mood.thoughts.memories;
            bool hasEtherBond = Pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBond"));
            bool hasEtherBroken = Pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBroken"));

            if (HatchingTicker < (daysToProduce * 60000))
            {
                HatchingTicker += 1;
            }
            else if (Pawn.Map != null)
            {
                HatchingTicker = 0;
                for (int i = 0; i < amount; i++)
                {
                    if (Rand.RangeInclusive(0, 100) <= (chance) && rareResource != null)
                    {
                        GenSpawn.Spawn(rareResource, Pawn.Position, Pawn.Map);
                    }
                    else
                    {
                        GenSpawn.Spawn(resource, Pawn.Position, Pawn.Map); 
                    }
                }

                if (!hasEtherBond && !hasEtherBroken)
                {
                    if (Rand.RangeInclusive(0, 100) <= bondChance)
                    {
                        Pawn.health.AddHediff(HediffDef.Named("EtherBond"));
                        Find.LetterStack.ReceiveLetter(
                            "LetterHediffFromEtherBondLabel".Translate(Pawn).CapitalizeFirst(),
                            "LetterHediffFromEtherBond".Translate(Pawn).CapitalizeFirst(),
                            LetterDefOf.NeutralEvent, Pawn, null, null);
                    }
                    else if (Rand.RangeInclusive(0, 100) <= brokenChance)
                    {
                        Pawn.health.AddHediff(HediffDef.Named("EtherBroken"));
                        Find.LetterStack.ReceiveLetter(
                            "LetterHediffFromEtherBrokenLabel".Translate(Pawn).CapitalizeFirst(),
                            "LetterHediffFromEtherBroken".Translate(Pawn).CapitalizeFirst(),
                            LetterDefOf.NeutralEvent, Pawn, null, null);
                    }
                }

                if (stageThought != null)
                {
                    thoughts.TryGainMemory(stageThought);
                }

                if (hasEtherBond && Props.etherBondThought != null)
                {
                    thoughts.TryGainMemory(Props.etherBondThought);
                }
                else if (hasEtherBroken && Props.etherBrokenThought != null)
                {
                    thoughts.TryGainMemory(Props.etherBrokenThought);
                }
                else
                {
                    if (Props.genderAversion == Pawn.gender && Props.wrongGenderThought != null)
                    {
                        thoughts.TryGainMemory(Props.wrongGenderThought);
                    }
                    else if (Props.thought != null)
                    {
                        thoughts.TryGainMemory(Props.thought);
                    }
                    brokenChance += 0.5f;
                    bondChance += 0.2f;
                }
            }
        }
    }
}
