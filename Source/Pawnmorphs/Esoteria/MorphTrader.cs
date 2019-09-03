using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Multiplayer.API;

namespace Pawnmorph
{
    public class StockGenerator_Morphs : StockGenerator
    {
        [NoTranslate]
        private List<string> tradeTagsSell = new List<string>();

        private IntRange kindCountRange = new IntRange(1, 1);

        private float minWildness = 0f;

        private float maxWildness = 1f;

        private bool checkTemperature = true;

        private static readonly SimpleCurve SelectionChanceFromWildnessCurve = new SimpleCurve
    {
        new CurvePoint(0f, 100f),
        new CurvePoint(0.25f, 60f),
        new CurvePoint(0.5f, 30f),
        new CurvePoint(0.75f, 12f),
        new CurvePoint(1f, 2f)
    };

        public override IEnumerable<Thing> GenerateThings(int forTile)
        {
            int numKinds = kindCountRange.RandomInRange;
            int count = countRange.RandomInRange;
            List<PawnKindDef> kinds = new List<PawnKindDef>();
            for (int j = 0; j < numKinds; j++)
            {
                if (!(from k in DefDatabase<PawnKindDef>.AllDefs
                      where !kinds.Contains(k) && PawnKindAllowed(k, forTile)
                      select k).TryRandomElementByWeight((PawnKindDef k) => SelectionChance(k), out PawnKindDef result))
                {
                    break;
                }
                kinds.Add(result);
            }
            for (int i = 0; i < count; i++)
            {
                if (!kinds.TryRandomElement(out PawnKindDef kind))
                {
                    break;
                }
                PawnKindDef kind2 = kind;
                int tile = forTile;

                Pawn pawnOriginal = Find.WorldPawns.AllPawnsAlive.Where(p => !p.IsPlayerControlledCaravanMember() && (PawnUtility.ForSaleBySettlement(p) || p.kindDef == PawnKindDefOf.Slave || (PawnUtility.IsKidnappedPawn(p) && p.RaceProps.Humanlike) && !PawnUtility.IsFactionLeader(p))).RandomElement();
                
                PawnGenerationRequest request = new PawnGenerationRequest(kind2, null, PawnGenerationContext.NonPlayer, tile); 
                Pawn pawn = PawnGenerator.GeneratePawn(request); //Generate the animal!

                
                

                Hediff h = HediffMaker.MakeHediff(HediffDef.Named("TransformedHuman"), pawn);
                h.Severity = Rand.Range(0.02f, 0.9f);
                pawn.health.AddHediff(h);

                if (pawnOriginal == null)
                {
                    Gender newGender = pawn.gender;

                    if (Rand.RangeInclusive(0, 100) <= 25)
                    {
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

                    }

                    float animalAge = pawn.ageTracker.AgeBiologicalYearsFloat;
                    float animalLifeExpectancy = pawn.def.race.lifeExpectancy;
                    float humanLifeExpectancy = 80f;

                    float converted = animalLifeExpectancy / animalAge;

                    float lifeExpectancy = humanLifeExpectancy / converted;

                    List<PawnKindDef> pkds = new List<PawnKindDef>();
                    pkds.Add(PawnKindDefOf.Slave);
                    pkds.Add(PawnKindDefOf.WildMan);
                    pkds.Add(PawnKindDefOf.Colonist);
                    pkds.Add(PawnKindDefOf.SpaceRefugee);
                    pkds.Add(PawnKindDefOf.Villager);
                    pkds.Add(PawnKindDefOf.Drifter);
                    pkds.Add(PawnKindDefOf.AncientSoldier);

                    pawnOriginal = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pkds.RandomElement(), Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, new float?(lifeExpectancy), new float?(Rand.Range(lifeExpectancy, lifeExpectancy + 200)), new Gender?(newGender), null, null));
                    pawn.Name = pawnOriginal.Name;
                }
                else {
                    pawn.Name = pawnOriginal.Name;
                    Find.WorldPawns.RemovePawn(pawnOriginal);
                }

                var pm = TfSys.TransformedPawn.Create(pawnOriginal, pawn); //pawnOriginal is human, pawn is animal
                Find.World.GetComponent<PawnmorphGameComp>().AddTransformedPawn(pm);

                yield return pawn;
            }
        }

        private float SelectionChance(PawnKindDef k)
        {
            return SelectionChanceFromWildnessCurve.Evaluate(k.RaceProps.wildness);
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef.category == ThingCategory.Pawn && thingDef.race.Animal && thingDef.tradeability != 0 && (tradeTagsSell.Any((string tag) => thingDef.tradeTags != null && thingDef.tradeTags.Contains(tag)));
        }

        private bool PawnKindAllowed(PawnKindDef kind, int forTile)
        {
            if (!kind.RaceProps.Animal || kind.RaceProps.wildness < minWildness || kind.RaceProps.wildness > maxWildness || kind.RaceProps.wildness > 1f)
            {
                return false;
            }
            if (checkTemperature)
            {
                int num = forTile;
                if (num == -1 && Find.AnyPlayerHomeMap != null)
                {
                    num = Find.AnyPlayerHomeMap.Tile;
                }
                if (num != -1 && !Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(num, kind.race))
                {
                    return false;
                }
            }
            if (kind.race.tradeTags == null)
            {
                return false;
            }
            if (!tradeTagsSell.Any((string x) => kind.race.tradeTags.Contains(x)))
            {
                return false;
            }
            if (!kind.race.tradeability.TraderCanSell())
            {
                return false;
            }
            return true;
        }

        public void LogAnimalChances()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PawnKindDef allDef in DefDatabase<PawnKindDef>.AllDefs)
            {
                stringBuilder.AppendLine(allDef.defName + ": " + SelectionChance(allDef).ToString("F2"));
            }
            Log.Message(stringBuilder.ToString());
        }
    }
}
