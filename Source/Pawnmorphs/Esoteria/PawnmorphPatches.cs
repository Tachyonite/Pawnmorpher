// PawnmorphPatches.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on //2019 
// last updated 08/11/2019  9:54 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AlienRace;
using Harmony;
using Pawnmorph.DefExtensions;
using Pawnmorph.FormerHumans;
using Pawnmorph.Hybrids;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
    [StaticConstructorOnStartup]
    internal static class PawnmorphPatches
    {
        private static readonly Type patchType = typeof(PawnmorphPatches);

        static PawnmorphPatches()
        {
            HarmonyInstance
                harmonyInstance = HarmonyInstance.Create("com.BioReactor.rimworld.mod"); //shouldn't this be different? 
            harmonyInstance.Patch(
                                  AccessTools.Method(typeof(FoodUtility),
                                                     nameof(FoodUtility.ThoughtsFromIngesting)),
                                  null,
                                  new HarmonyMethod(patchType, nameof(ThoughtsFromIngestingPostfix))
                                 );

            // Job patches.
            harmonyInstance.Patch(AccessTools.Method(typeof(JobDriver_Ingest),
                                                     "PrepareToIngestToils"),
                                  new HarmonyMethod(patchType, nameof(PrepareToIngestToilsPrefix))
                                 );

            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void ThoughtsFromIngestingPostfix(Pawn ingester, Thing foodSource, ref List<ThoughtDef> __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(ingester.def, out MorphDef morphDef))
            {
                AteThought cannibalThought = morphDef.raceSettings?.thoughtSettings?.ateAnimalThought;
                if (cannibalThought == null) return;
                bool cannibal = ingester.story.traits.HasTrait(TraitDefOf.Cannibal);

                if (foodSource.def == morphDef.race.race.meatDef && !cannibal)
                {
                    __result.Add(ThoughtDef.Named(cannibalThought.thought));
                    return;
                }

                ThingDef comp = foodSource.TryGetComp<CompIngredients>()
                                         ?.ingredients?.FirstOrDefault(def => def == morphDef.race.race.meatDef);
                if (comp != null && !cannibal) __result.Add(ThoughtDef.Named(cannibalThought.ingredientThought));
            }
            else
            {
                FormerHumanStatus? fHStatus = ingester.GetFormerHumanStatus();
                if (fHStatus == null || fHStatus == FormerHumanStatus.PermanentlyFeral) return;

                ThoughtDef thought = GetCannibalThought(ingester, foodSource);
                if (thought != null) __result.Add(thought);
            }
        }

        private static ThoughtDef GetCannibalThought(Pawn ingester, Thing foodSource, ThingDef raceDef = null)
        {
            raceDef = raceDef ?? ingester.def;
            CannibalThoughtStatus cannibalStatus = PMFoodUtilities.GetStatusOfFoodForPawn(raceDef, foodSource);
            var ext = ingester.def.GetModExtension<FormerHumanSettings>();
            ThoughtDef thoughtDefBad;
            ThoughtDef cannibalThoughtDef;
            FoodThoughtSettings foodSettings = ext?.foodThoughtSettings;
            switch (cannibalStatus)
                //assign the correct cannibal thoughts, one for pawns without the cannibal trait the other for pawns with it 
            {
                case CannibalThoughtStatus.None:
                    return null;
                case CannibalThoughtStatus.Direct:
                    thoughtDefBad = foodSettings?.cannibalThought
                                 ?? PMThoughtDefOf.FHDefaultCannibalThought_Direct;
                    cannibalThoughtDef = foodSettings?.cannibalThoughtGood
                                      ?? PMThoughtDefOf.FHDefaultCannibalGoodThought_Direct;
                    break;
                case CannibalThoughtStatus.Ingredient:
                    thoughtDefBad = foodSettings?.cannibalThoughtIngredient
                                 ?? PMThoughtDefOf.FHDefaultCannibalThought_Ingredient;
                    cannibalThoughtDef = foodSettings?.cannibalThoughtIngredientGood
                                      ?? PMThoughtDefOf.FHDefaultCannibalGoodThought_Ingredient;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            bool isCannibal = ingester.story?.traits?.HasTrait(TraitDefOf.Cannibal) ?? false;
            ThoughtDef thought = isCannibal ? cannibalThoughtDef : thoughtDefBad;
            return thought;
        }

        private static bool PrepareToIngestToilsPrefix(JobDriver __instance, Toil chewToil, ref IEnumerable<Toil> __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(__instance.pawn.def, out MorphDef def))
            {
                Thing thing = __instance.job.targetA.Thing;
                if (thing.def.ingestible == null) return true;

                FoodTypeFlags flg = thing.def.ingestible.foodType & (FoodTypeFlags.Tree | FoodTypeFlags.Plant);
                if (flg != 0)
                {
                    __result = new[]
                    {
                        ReserveFoodIfWillIngestWholeStack(__instance),
                        Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
                    };

                    return false;
                }
            }

            return true;
        }

        private static Toil ReserveFoodIfWillIngestWholeStack(JobDriver driver)
        {
            return new Toil
            {
                initAction = delegate
                {
                    if (driver.pawn.Faction == null) return;

                    Thing thing = driver.job.GetTarget(TargetIndex.A).Thing;
                    if (driver.pawn.carryTracker.CarriedThing == thing) return;

                    int num = FoodUtility.WillIngestStackCountOf(driver.pawn, thing.def,
                                                                 thing.GetStatValue(StatDefOf.Nutrition));
                    if (num >= thing.stackCount)
                    {
                        if (!thing.Spawned)
                        {
                            driver.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                            return;
                        }

                        driver.pawn.Reserve(thing, driver.job);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant,
                atomicWithPrevious = true
            };
        }
    }
}