using System.Linq;
using Multiplayer.API;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class HediffComp_Production : HediffComp
    {
        private int HatchingTicker = 0;
        private float brokenChance = 0f;
        private float bondChance = 0f;

        public HediffCompProperties_Production Props => (HediffCompProperties_Production) props;

        public override void CompPostTick(ref float severityAdjustment)
        {

            if (Props.stages != null)
            {
                HediffComp_Staged stage = Props.stages.ElementAt(parent.CurStageIndex);
               
                TryProduce(stage.daysToProduce, stage.amount, stage.chance, ThingDef.Named(stage.resource), stage.RareResource,
                        stage.thought);
            }
            else
            {
                TryProduce(Props.daysToProduce, Props.amount, Props.chance, ThingDef.Named(Props.resource), Props.RareResource);
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref HatchingTicker, "hatchingTicker");
            Scribe_Values.Look(ref brokenChance, "brokenChance");
            Scribe_Values.Look(ref bondChance, "bondChance");
            base.CompExposeData();
        }
        
        void TryProduce(float daysToProduce, int amount, float chance, ThingDef resource, ThingDef rareResource,
                            ThoughtDef stageThought = null)
        {

            if (HatchingTicker < daysToProduce * 60000)
                HatchingTicker++;
            else if (Pawn.Map != null) Produce(amount, chance, resource, rareResource, stageThought);

            
        }

        /// <summary>
        /// spawns in the products at the parent's current location 
        /// </summary>
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
            bool hasEtherBond = Pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBond"));
            bool hasEtherBroken = Pawn.health.hediffSet.HasHediff(HediffDef.Named("EtherBroken"));

            HatchingTicker = 0;

            int thingCount = 0;
            int rareThingCount = 0;

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


            if (!hasEtherBond && !hasEtherBroken)
            {
                if (Rand.RangeInclusive(0, 100) <= bondChance)
                {
                    Pawn.health.AddHediff(HediffDef.Named("EtherBond"));
                    hasEtherBond = true;
                    Find.LetterStack.ReceiveLetter(
                                                   "LetterHediffFromEtherBondLabel".Translate(Pawn).CapitalizeFirst(),
                                                   "LetterHediffFromEtherBond".Translate(Pawn).CapitalizeFirst(),
                                                   LetterDefOf.NeutralEvent, Pawn);
                }
                else if (Rand.RangeInclusive(0, 100) <= brokenChance)
                {
                    Pawn.health.AddHediff(HediffDef.Named("EtherBroken"));
                    hasEtherBroken = true;
                    Find.LetterStack.ReceiveLetter(
                                                   "LetterHediffFromEtherBrokenLabel".Translate(Pawn).CapitalizeFirst(),
                                                   "LetterHediffFromEtherBroken".Translate(Pawn).CapitalizeFirst(),
                                                   LetterDefOf.NeutralEvent, Pawn);
                }
            }

            if (stageThought != null) thoughts.TryGainMemory(stageThought);

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
                    thoughts.TryGainMemory(Props.wrongGenderThought);
                else if (Props.thought != null) thoughts.TryGainMemory(Props.thought);
                brokenChance += 0.5f;
                bondChance += 0.2f;
            }

            RandUtilities.PopState();

        }
    }
}