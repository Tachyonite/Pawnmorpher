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
using RimWorld.Planet;
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
                original: AccessTools.Method(type: typeof(FoodUtility),
                    name: nameof(FoodUtility.ThoughtsFromIngesting)),
                prefix: null,
                postfix: new HarmonyMethod(type: patchType, name: nameof(ThoughtsFromIngestingPostfix)));


            //untested changes below 
            //trying to change the preferability of the morphs, so they'll eat raw food before starving themselves (only really effects the warg morph) 
#if false
            harmonyInstance.Patch( AccessTools.Method(typeof(CaravanPawnsNeedsUtility), nameof(CaravanPawnsNeedsUtility.GetFoodScore),
                                                      new Type[]
                                                      {
                                                          typeof(ThingDef), typeof(Pawn), typeof(float)
                                                      })
                                  , null, new HarmonyMethod(patchType, nameof(GetFoodScoreDefPostfix)));
            harmonyInstance
               .Patch(AccessTools.Method(typeof(CaravanPawnsNeedsUtility), nameof(CaravanPawnsNeedsUtility.CanEatForNutritionEver)),
                      null, new HarmonyMethod(patchType, nameof(CanEatForNutritionEverPostfix)));
            harmonyInstance.Patch(AccessTools.Method(typeof(CaravanPawnsNeedsUtility),
                                                     nameof(CaravanPawnsNeedsUtility.CanEatForNutritionNow),
                                                     new Type[]
                                                     {
                                                         typeof(ThingDef), typeof(Pawn)
                                                     }),
                                  null, new HarmonyMethod(patchType, nameof(CanEatForNutritionNowDefPostfix)));

            harmonyInstance.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.WillEat), new[]
            {
                typeof(Pawn), typeof(Thing), typeof(Pawn)
            }), null, new HarmonyMethod(patchType, nameof(WillEatThingPostfix)));

            harmonyInstance.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.WillEat), new[]
            {
                typeof(Pawn), typeof(ThingDef), typeof(Pawn)
            }), null, new HarmonyMethod(patchType, nameof(WillEatDefPostfix)));

            harmonyInstance.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality)),
                                  null, new HarmonyMethod(patchType, nameof(FoodOptimalityPostfix)));

            harmonyInstance.Patch(AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap)),
                                  new HarmonyMethod(patchType, nameof(BestFoodSourceOnMapPrefix)));

#endif


            //end untested changes 


            //job patches 
            harmonyInstance.Patch(AccessTools.Method(typeof(JobDriver_Ingest),
                "PrepareToIngestToils"),
                new HarmonyMethod(patchType, nameof(PrepareToIngestToilsPrefix)));


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

        public static void GetFoodScoreDefPostfix(ThingDef food, Pawn pawn, float singleFoodNutrition, ref float __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(pawn.def, out MorphDef morph))
            {
                var prefOverride = morph.GetOverride(food);
                if (prefOverride != null)
                {
                    __result = (float) prefOverride.Value; 
                }
            }


            
        }

        public static void CanEatForNutritionEverPostfix(ThingDef food, Pawn pawn, ref bool __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(pawn.def, out MorphDef def))
            {

                var pref = pawn.GetOverride(food) ?? food.ingestible.preferability;
                __result =  food.IsNutritionGivingIngestible
                            && pawn.WillEat(food)
                            && pref > FoodPreferability.NeverForNutrition
                            && (!food.IsDrug || pawn.IsTeetotaler());
                
            }



        }

        public static void CanEatForNutritionNowDefPostfix(ThingDef food, Pawn pawn, ref bool __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(pawn.def, out MorphDef def))
            {
                __result = CaravanPawnsNeedsUtility.CanEatForNutritionEver(food, pawn); 
            }
        }


        public static void WillEatThingPostfix(Pawn p, Thing food, Pawn getter, ref bool __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(p.def, out MorphDef def))
            {

                if (!def.race.race.CanEverEat(food))
                {
                    __result = false;
                    return; 
                }
                if (p.foodRestriction != null)
                {
                    FoodRestriction currentRespectedRestriction = p.foodRestriction.GetCurrentRespectedRestriction(getter);
                    if (currentRespectedRestriction != null && !currentRespectedRestriction.Allows(food) && (food.def.IsWithinCategory(ThingCategoryDefOf.Foods)))
                    {
                        __result =  false;
                        return; 
                    }
                }

                __result = true; 

            }
        }


        public static void WillEatDefPostfix(Pawn p, ThingDef food, Pawn getter, ref bool __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(p.def, out MorphDef def))
            {
                if (!def.race.race.CanEverEat(food))
                {
                    __result = false;
                    return; 
                }

                if (p.foodRestriction != null)
                {
                    FoodRestriction currentRespectedRestriction = p.foodRestriction.GetCurrentRespectedRestriction(getter);
                    if (currentRespectedRestriction != null && !currentRespectedRestriction.Allows(food) && (food.IsWithinCategory(ThingCategoryDefOf.Foods)))
                    {
                        __result = false;
                        return; 
                    }
                }

                __result = true; 

            }


        }

        public static void FoodOptimalityPostfix(Pawn eater, Thing foodSource, ThingDef foodDef, float dist,
            bool takingToInventory, ref float __result)
        {
            if (RaceGenerator.TryGetMorphOfRace(eater.def, out MorphDef def))
            {
                var fOverride = def.GetOverride(foodDef);
                if (fOverride != null)
                    __result = FoodOptimality(eater, foodSource, foodDef, dist, takingToInventory, def, fOverride.Value); 
            }


        }

        static float FoodOptimality(Pawn eater, Thing foodSource, ThingDef foodDef, float dist, bool takingToInventory,
            MorphDef morph, FoodPreferability fOverride)
        {
            float num = 300f;
            num -= dist;
            FoodPreferability preferability = fOverride;

            //var fOverride = morph.GetOverride(foodDef);
          

            //preferability = fOverride ?? preferability; 

            if (preferability != FoodPreferability.NeverForNutrition)
            {
                if (preferability != FoodPreferability.DesperateOnly)
                {
                    if (preferability == FoodPreferability.DesperateOnlyForHumanlikes)
                    {
                        if (eater.RaceProps.Humanlike)
                        {
                            num -= 150f;
                        }
                    }
                }
                else
                {
                    num -= 150f;
                }
                CompRottable compRottable = foodSource.TryGetComp<CompRottable>();
                if (compRottable != null)
                {
                    if (compRottable.Stage == RotStage.Dessicated)
                    {
                        return -9999999f;
                    }
                    if (!takingToInventory && compRottable.Stage == RotStage.Fresh && compRottable.TicksUntilRotAtCurrentTemp < 30000)
                    {
                        num += 12f;
                    }
                }
                if (eater.needs != null && eater.needs.mood != null)
                {
                    List<ThoughtDef> list = FoodUtility.ThoughtsFromIngesting(eater, foodSource, foodDef);
                    for (int i = 0; i < list.Count; i++)  
                    {
                        num += FoodOptimalityEffectFromMoodCurve.Evaluate(list[i].stages[0].baseMoodEffect); 
                    }
                }
                if (foodDef.ingestible != null)
                {
                    
                    
                    num += foodDef.ingestible.optimalityOffsetFeedingAnimals;
                    
                }

                var diff = 5.5f * ((int) fOverride - (int) foodDef.ingestible.preferability); //constant is a guess at how large the optimality should be 
                num += FoodOptimalityEffectFromMoodCurve.Evaluate(diff); 
                Log.Message($"{eater.Name} has preferability for {foodDef.defName} of {num}");
            

               

                

                return num;
            }
            return -9999999f;



            
        }

        static bool BestFoodSourceOnMapPrefix(
            Pawn getter,
            Pawn eater,
            bool desperate,
            ref ThingDef foodDef,
            FoodPreferability maxPref,
            bool allowPlant,
            bool allowDrug,
            bool allowCorpse,
            bool allowDispenserFull,
            bool allowDispenserEmpty,
            bool allowForbidden,
            bool allowSociallyImproper,
            bool allowHarvest,
            bool forceScanWholeMap,
            ref Thing __result)
        {

            if (!RaceGenerator.TryGetMorphOfRace(eater.def, out MorphDef def))
            {
                return true; 
            }

            foodDef = (ThingDef)null;
            bool getterCanManipulate = getter.RaceProps.ToolUser && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            if (!getterCanManipulate && getter != eater)
            {
                return true; 
            }
            FoodPreferability minPref = !eater.NonHumanlikeOrWildMan() ? (!desperate ? (eater.needs.food.CurCategory < HungerCategory.UrgentlyHungry ? FoodPreferability.MealAwful : FoodPreferability.RawBad) : FoodPreferability.DesperateOnly) : FoodPreferability.NeverForNutrition;
            Predicate<Thing> foodValidator = (Predicate<Thing>)(t =>
            {
                Building_NutrientPasteDispenser nutrientPasteDispenser = t as Building_NutrientPasteDispenser;
                if (nutrientPasteDispenser != null)
                {
                    if (!allowDispenserFull || !getterCanManipulate || (ThingDefOf.MealNutrientPaste.ingestible.preferability < minPref || ThingDefOf.MealNutrientPaste.ingestible.preferability > maxPref) || (!eater.WillEat(ThingDefOf.MealNutrientPaste, getter) || t.Faction != getter.Faction && t.Faction != getter.HostFaction) || (!allowForbidden && t.IsForbidden(getter) || !nutrientPasteDispenser.powerComp.PowerOn || (!allowDispenserEmpty && !nutrientPasteDispenser.HasEnoughFeedstockInHoppers() || (!t.InteractionCell.Standable(t.Map) || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)))) || (getter.IsWildMan() || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map, false), PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some, TraverseMode.ByPawn, false))))
                        return false;
                }
                else
                {
                    FoodPreferability ingestiblePreferability =  def.GetOverride(t.def) ?? t.def.ingestible.preferability;
                    


                    if (ingestiblePreferability < minPref || ingestiblePreferability > maxPref || (!eater.WillEat(t, getter) || !t.def.IsNutritionGivingIngestible) || (!t.IngestibleNow || !allowCorpse && t is Corpse) || (!allowDrug && t.def.IsDrug || !allowForbidden && t.IsForbidden(getter) || (!desperate && t.IsNotFresh() || (t.IsDessicated() || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)))) || (!getter.AnimalAwareOf(t) && !forceScanWholeMap || !getter.CanReserve((LocalTargetInfo)t, 1, -1, (ReservationLayerDef)null, false)))
                        return false;
                }

                return true;
            });
            ThingRequest req = (eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) == FoodTypeFlags.None || !allowPlant ? ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree) : ThingRequest.ForGroup(ThingRequestGroup.FoodSource);
            Thing bestThing;

            bestThing = SpawnedFoodSearchInnerScan(eater, getter.Position, getter.Map.listerThings.ThingsMatching(req), PathEndMode.ClosestTouch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, foodValidator);
            if (allowHarvest && getterCanManipulate)
            {
                int searchRegionsMax = !forceScanWholeMap || bestThing != null ? 30 : -1;
                Thing foodSource = GenClosest.ClosestThingReachable(getter.Position, getter.Map, ThingRequest.ForGroup(ThingRequestGroup.HarvestablePlant), PathEndMode.Touch, TraverseParms.For(getter, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, (Predicate<Thing>)(x =>
                {
                    Plant t = (Plant)x;
                    if (!t.HarvestableNow)
                    {  return false;}
                    ThingDef harvestedThingDef = t.def.plant.harvestedThingDef;
                    return harvestedThingDef.IsNutritionGivingIngestible && eater.WillEat(harvestedThingDef, getter) && getter.CanReserve((LocalTargetInfo)((Thing)t), 1, -1, (ReservationLayerDef)null) && ((allowForbidden || !t.IsForbidden(getter)) && (bestThing == null || FoodUtility.GetFinalIngestibleDef(bestThing, false).ingestible.preferability < harvestedThingDef.ingestible.preferability));
                }), (IEnumerable<Thing>)null, 0, searchRegionsMax, false, RegionType.Set_Passable, false);
                if (foodSource != null)
                {
                    bestThing = foodSource;
                    foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, true);
                }
            }
            if (foodDef == null && bestThing != null)
                foodDef = FoodUtility.GetFinalIngestibleDef(bestThing, false);

            __result = bestThing;
            return false; 
        }
        private static bool IsFoodSourceOnMapSociallyProper(
            Thing t,
            Pawn getter,
            Pawn eater,
            bool allowSociallyImproper)
        {
            if (!allowSociallyImproper)
            {
                bool animalsCare = !getter.RaceProps.Animal;
                if (!t.IsSociallyProper(getter) && !t.IsSociallyProper(eater, eater.IsPrisonerOfColony, animalsCare))
                    return false;
            }
            return true;
        }
        private static Thing SpawnedFoodSearchInnerScan(
            Pawn eater,
            IntVec3 root,
            List<Thing> searchSet,
            PathEndMode peMode,
            TraverseParms traverseParams,
            float maxDistance = 9999f,
            Predicate<Thing> validator = null)
        {
            if (searchSet == null)
                return (Thing)null;
            Pawn pawn = traverseParams.pawn ?? eater;
            int num1 = 0;
            int num2 = 0;
            Thing thing = (Thing)null;
            float num3 = float.MinValue;
            for (int index = 0; index < searchSet.Count; ++index)
            {
                Thing search = searchSet[index];
                ++num2;
                float lengthManhattan = (float)(root - search.Position).LengthManhattan;
                if ((double)lengthManhattan <= (double)maxDistance)
                {
                    float num4 = FoodUtility.FoodOptimality(eater, search, FoodUtility.GetFinalIngestibleDef(search, false), lengthManhattan, false);
                    if ((double)num4 >= (double)num3 && pawn.Map.reachability.CanReach(root, (LocalTargetInfo)search, peMode, traverseParams) && search.Spawned && (validator == null || validator(search)))
                    {
                        thing = search;
                        num3 = num4;
                        ++num1;
                    }
                }
            }
            return thing;
        }

        private static readonly SimpleCurve FoodOptimalityEffectFromMoodCurve = new SimpleCurve
        {
            {
                new CurvePoint(-100f, -600f),
                true
            },
            {
                new CurvePoint(-10f, -100f),
                true
            },
            {
                new CurvePoint(-5f, -70f),
                true
            },
            {
                new CurvePoint(-1f, -50f),
                true
            },
            {
                new CurvePoint(0f, 0f),
                true
            },
            {
                new CurvePoint(100f, 800f),
                true
            }
        };


        static FoodPreferability? GetOverride(this Pawn pawn, ThingDef food)
        {
            if (RaceGenerator.TryGetMorphOfRace(pawn.def, out MorphDef morph))
            {
                var prefOverride = morph.GetOverride(food);
                return prefOverride; 
            }

            return null; 
        }



    }
}