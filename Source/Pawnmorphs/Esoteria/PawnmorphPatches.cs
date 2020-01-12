// PawnmorphPatches.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on //2019 
// last updated 08/11/2019  9:54 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AlienRace;
using Harmony;
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
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("com.BioReactor.rimworld.mod"); //shouldn't this be different? 
            harmonyInstance.Patch(
                original: AccessTools.Method(type: typeof(FoodUtility),
                name: nameof(FoodUtility.ThoughtsFromIngesting)),
                prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(ThoughtsFromIngestingPostfix))
                );

            // Job patches.
            harmonyInstance.Patch(AccessTools.Method(typeof(JobDriver_Ingest),
                "PrepareToIngestToils"),
                new HarmonyMethod(patchType, nameof(PrepareToIngestToilsPrefix))
                );

            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        static bool PrepareToIngestToilsPrefix(JobDriver __instance, Toil chewToil, ref IEnumerable<Toil> __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(__instance.pawn.def, out MorphDef def))
            {
                var thing = __instance.job.targetA.Thing;
                if (thing.def.ingestible == null) return true;

                var flg = thing.def.ingestible.foodType & (FoodTypeFlags.Tree | FoodTypeFlags.Plant);
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

        static Toil ReserveFoodIfWillIngestWholeStack(JobDriver driver)
        {
            return new Toil()
            {
                initAction = delegate
                {
                    if (driver.pawn.Faction == null)
                    {
                        return;
                    }

                    Thing thing = driver.job.GetTarget(TargetIndex.A).Thing;
                    if (driver.pawn.carryTracker.CarriedThing == thing)
                    {
                        return;
                    }

                    int num = FoodUtility.WillIngestStackCountOf(driver.pawn, thing.def,
                        thing.GetStatValue(StatDefOf.Nutrition, true));
                    if (num >= thing.stackCount)
                    {
                        if (!thing.Spawned)
                        {
                            driver.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                            return;
                        }

                        driver.pawn.Reserve(thing, driver.job, 1, -1, null, true);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant,
                atomicWithPrevious = true
            };
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
    }
}
