// PMFoodUtilities.cs modified by Iron Wolf for Pawnmorph on 01/07/2020 7:51 PM
// last updated 01/07/2020  7:51 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
    /// <summary>
    ///     static class for food related utilities
    /// </summary>
    [StaticConstructorOnStartup]
    public static class PMFoodUtilities
    {
        [NotNull] private static readonly List<Pawn> tmpPredatorCandidates = new List<Pawn>();


        [NotNull] private static readonly HashSet<Thing> filtered = new HashSet<Thing>();


        static PMFoodUtilities()
        {
            NutrientPastePreferability = ThingDefOf.MealNutrientPaste.ingestible.preferability;
        }

        private static FoodPreferability NutrientPastePreferability { get; }

        static FoodPreferability GetAdjustedPreferability(Pawn pawn, [NotNull] Thing food)
        {
            var rawPref = food.def.ingestible?.preferability;
            if (rawPref == null) return FoodPreferability.Undefined;
            var rPrefVal = rawPref.Value; 
            FoodTypeFlags plantOrTree = FoodTypeFlags.Plant | FoodTypeFlags.Tree;
            if((food.def.ingestible.foodType & plantOrTree) != 0 && (pawn.RaceProps.foodType & plantOrTree) != 0 )
            {
                var nx = ((int) rPrefVal) + 1; //make plants appear better then they actually are 
                nx = Mathf.Clamp(nx, 0, 9);
                rPrefVal = (FoodPreferability) nx; 
            }else if (pawn.RaceProps.foodType == FoodTypeFlags.CarnivoreAnimalStrict
                   && (food.def.ingestible.foodType & FoodTypeFlags.CarnivoreAnimalStrict) != 0)
            {
                var nx = ((int)rPrefVal) + 2; //make meat appear better then they actually are for strict carnivorous 
                nx = Mathf.Clamp(nx, 0, 9);
                rPrefVal = (FoodPreferability)nx;
            }

            return rPrefVal; 
        }


        /// <summary>
        ///     Gets the best food source on the map for the given getter and eater pawns
        /// </summary>
        /// this function gets the best food source on the map for the given pawns, making sure to optimize for the case where
        /// a humanoid pawn can eat plants
        /// <param name="getter">The getter.</param>
        /// <param name="eater">The eater.</param>
        /// <param name="desperate">if set to <c>true</c> [desperate].</param>
        /// <param name="foodDef">The food definition.</param>
        /// <param name="maxPref">The maximum preference.</param>
        /// <param name="allowPlant">if set to <c>true</c> [allow plant].</param>
        /// <param name="allowDrug">if set to <c>true</c> [allow drug].</param>
        /// <param name="allowCorpse">if set to <c>true</c> [allow corpse].</param>
        /// <param name="allowDispenserFull">if set to <c>true</c> [allow dispenser full].</param>
        /// <param name="allowDispenserEmpty">if set to <c>true</c> [allow dispenser empty].</param>
        /// <param name="allowForbidden">if set to <c>true</c> [allow forbidden].</param>
        /// <param name="allowSociallyImproper">if set to <c>true</c> [allow socially improper].</param>
        /// <param name="allowHarvest">if set to <c>true</c> [allow harvest].</param>
        /// <param name="forceScanWholeMap">if set to <c>true</c> [force scan whole map].</param>
        /// <param name="ignoreReservations">if set to <c>true</c> [ignore reservations].</param>
        /// <param name="minPrefOverride">The minimum preference override.</param>
        /// <returns></returns>
        public static Thing BestFoodSourceOnMapOptimized(
            [NotNull] Pawn getter,
            [NotNull] Pawn eater,
            bool desperate,
            out ThingDef foodDef,
            FoodPreferability maxPref = FoodPreferability.MealLavish,
            bool allowPlant = true,
            bool allowDrug = true,
            bool allowCorpse = true,
            bool allowDispenserFull = true,
            bool allowDispenserEmpty = true,
            bool allowForbidden = false,
            bool allowSociallyImproper = false,
            bool allowHarvest = false,
            bool forceScanWholeMap = false,
            bool ignoreReservations = false,
            FoodPreferability minPrefOverride = FoodPreferability.Undefined)
        {
            foodDef = null;
            HungerCategory foodCurCategory = eater.needs.food.CurCategory;
            bool getterCanManipulate = getter.IsToolUser() && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            if (!getterCanManipulate && getter != eater)
            {
                Log.Error(getter
                        + " tried to find food to bring to "
                        + eater
                        + " but "
                        + getter
                        + " is incapable of Manipulation.");
                return null;
            }

            FoodPreferability minPref;
            if (minPrefOverride != FoodPreferability.Undefined)
                minPref = minPrefOverride;
            else if (!eater.NonHumanlikeOrWildMan()) //with the new patch, to 'recruit' sapient former humans pawns will need 
                if (!desperate)
                {
                    if (foodCurCategory >= HungerCategory.UrgentlyHungry)
                        minPref = FoodPreferability.RawBad;
                    else
                        minPref = FoodPreferability.MealAwful;
                }
                else
                {
                    minPref = FoodPreferability.DesperateOnly;
                }
            else
                minPref = FoodPreferability.NeverForNutrition;

            bool FoodValidator(Thing t)
            {
                if (allowDispenserFull && getterCanManipulate && t is Building_NutrientPasteDispenser nutrientPDispenser)
                {
                    if (ThingDefOf.MealNutrientPaste.ingestible.preferability < minPref
                     || ThingDefOf.MealNutrientPaste.ingestible.preferability > maxPref
                     || !eater.WillEat(ThingDefOf.MealNutrientPaste, getter)
                     || t.Faction != getter.Faction && t.Faction != getter.HostFaction
                     || !allowForbidden && t.IsForbidden(getter)
                     || !nutrientPDispenser.powerComp.PowerOn
                     || !allowDispenserEmpty && !nutrientPDispenser.HasEnoughFeedstockInHoppers()
                     || !t.InteractionCell.Standable(t.Map)
                     || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)
                     || !getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.InteractionCell, t.Map),
                                                                  PathEndMode.OnCell, TraverseParms.For(getter, Danger.Some)))
                        return false;
                }
                else
                {
                    FoodPreferability pref = GetAdjustedPreferability(eater, t);
                    if (pref < minPref
                     || pref > maxPref)
                        return false;

                    if (!eater.WillEat(t, getter)) return false;

                    if (!t.def.IsNutritionGivingIngestible || !t.IngestibleNow) return false;

                    if (!allowCorpse && t is Corpse
                     || !allowDrug && t.def.IsDrug
                     || !allowForbidden && t.IsForbidden(getter)
                     || !desperate && t.IsNotFresh()
                     || t.IsDessicated()
                     || !IsFoodSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper)
                     || !getter.AnimalAwareOf(t) && !forceScanWholeMap
                     || !ignoreReservations && !getter.CanReserve((LocalTargetInfo) t, 10, 1)) return false;
                }

                return true;
            }

            ThingRequest thingRequest;
            if (!CanEatPlants(eater, allowPlant, foodCurCategory))
                thingRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
            else
                thingRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodSource);
            Thing bestThing;
            if (getter.IsHumanlike())
            {
                //TODO split up search for hungry humanlike into 2 phases 
                //whole map search for good food 
                //small search for good plants 
                bestThing = SpawnedFoodSearchInnerScan(eater, getter.Position,
                                                       getter.Map.listerThings.ThingsMatching(thingRequest),
                                                       PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f, FoodValidator);

               

                if (allowHarvest & getterCanManipulate)
                {
                    int searchRegionsMax = !forceScanWholeMap || bestThing != null ? 30 : -1;

                    bool HarvestValidator(Thing x)
                    {
                        var t = (Plant) x;
                        if (!t.HarvestableNow) return false;
                        ThingDef harvestedThingDef = t.def.plant.harvestedThingDef;
                        return harvestedThingDef.IsNutritionGivingIngestible
                            && eater.WillEat(harvestedThingDef, getter)
                            && getter.CanReserve((LocalTargetInfo) t)
                            && (allowForbidden || !t.IsForbidden(getter))
                            && (bestThing == null
                             || FoodUtility.GetFinalIngestibleDef(bestThing)
                                           .ingestible.preferability
                              < harvestedThingDef.ingestible.preferability);
                    }

                    Thing foodSource = GenClosest.ClosestThingReachable(getter.Position, getter.Map,
                                                                        ThingRequest.ForGroup(ThingRequestGroup.HarvestablePlant),
                                                                        PathEndMode.Touch, TraverseParms.For(getter), 9999f,
                                                                        HarvestValidator, null, 0, searchRegionsMax);
                    if (foodSource != null)
                    {
                        bestThing = foodSource;
                        foodDef = FoodUtility.GetFinalIngestibleDef(foodSource, true);
                    }
                }

                if (foodDef == null && bestThing != null)
                    foodDef = FoodUtility.GetFinalIngestibleDef(bestThing);
            }
            else
            {
                int maxRegionsToScan =
                    GetMaxRegionsToScan(getter, forceScanWholeMap, foodCurCategory); //this is where the lag comes from 
                //humanlikes alwayse scan the whole map 
                filtered.Clear();
                foreach (Thing thing in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, true))
                {
                    var pawn = thing as Pawn;
                    if (pawn != null
                     && pawn != getter
                     && pawn.RaceProps.Animal
                     && pawn.CurJob != null
                     && pawn.CurJob.def == JobDefOf.Ingest
                     && pawn.CurJob.GetTarget(TargetIndex.A).HasThing)
                        filtered.Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing);
                }

                bool ignoreEntirelyForbiddenRegions = !allowForbidden
                                                   && ForbidUtility.CaresAboutForbidden(getter, true)
                                                   && getter.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap != null;
                var validator = (Predicate<Thing>) (t => FoodValidator(t)
                                                      && !filtered.Contains(t)
                                                      && (t is Building_NutrientPasteDispenser
                                                       || t.def.ingestible.preferability > FoodPreferability.DesperateOnly)
                                                      && !t.IsNotFresh());
                bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest, PathEndMode.ClosestTouch,
                                                             TraverseParms.For(getter), 9999f, validator, null, 0,
                                                             maxRegionsToScan, false, RegionType.Set_Passable,
                                                             ignoreEntirelyForbiddenRegions);
                filtered.Clear();
                if (bestThing == null)
                {
                    desperate = true;
                    bestThing = GenClosest.ClosestThingReachable(getter.Position, getter.Map, thingRequest,
                                                                 PathEndMode.ClosestTouch, TraverseParms.For(getter), 9999f,
                                                                 FoodValidator, null, 0, maxRegionsToScan, false,
                                                                 RegionType.Set_Passable, ignoreEntirelyForbiddenRegions);
                }

                if (bestThing != null)
                    foodDef = FoodUtility.GetFinalIngestibleDef(bestThing);
            }

            return bestThing;
        }

        /// <summary>
        ///     Gets the cannibal status of food for pawn.
        /// </summary>
        /// <param name="raceDef">The race definition.</param>
        /// <param name="foodSource">The food source.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     raceDef
        ///     or
        ///     foodSource
        /// </exception>
        public static CannibalThoughtStatus GetStatusOfFoodForPawn([NotNull] ThingDef raceDef, [NotNull] Thing foodSource)
        {
            if (raceDef == null) throw new ArgumentNullException(nameof(raceDef));
            if (foodSource == null) throw new ArgumentNullException(nameof(foodSource));
            if (foodSource.def == raceDef.race?.meatDef) return CannibalThoughtStatus.Direct;
            if (foodSource.TryGetComp<CompIngredients>()?.ingredients?.Any(d => d == raceDef.race?.meatDef) ?? false)
                return CannibalThoughtStatus.Ingredient;
            return CannibalThoughtStatus.None;
        }

        /// <summary>
        ///     Tries the find best food source for the given getter and eater.
        /// </summary>
        /// Tries to find the best food source for the given getter and eater, taking into account humanoids that can eat plants
        /// <param name="getter">The getter.</param>
        /// <param name="eater">The eater.</param>
        /// <param name="desperate">if set to <c>true</c> [desperate].</param>
        /// <param name="foodSource">The food source.</param>
        /// <param name="foodDef">The food definition.</param>
        /// <param name="canRefillDispenser">if set to <c>true</c> [can refill dispenser].</param>
        /// <param name="canUseInventory">if set to <c>true</c> [can use inventory].</param>
        /// <param name="allowForbidden">if set to <c>true</c> [allow forbidden].</param>
        /// <param name="allowCorpse">if set to <c>true</c> [allow corpse].</param>
        /// <param name="allowSociallyImproper">if set to <c>true</c> [allow socially improper].</param>
        /// <param name="allowHarvest">if set to <c>true</c> [allow harvest].</param>
        /// <param name="forceScanWholeMap">if set to <c>true</c> [force scan whole map].</param>
        /// <param name="ignoreReservations">if set to <c>true</c> [ignore reservations].</param>
        /// <param name="minPrefOverride">The minimum preference override.</param>
        /// <returns></returns>
        public static bool TryFindBestFoodSourceForOptimized(
            Pawn getter,
            Pawn eater,
            bool desperate,
            out Thing foodSource,
            out ThingDef foodDef,
            bool canRefillDispenser = true,
            bool canUseInventory = true,
            bool allowForbidden = false,
            bool allowCorpse = true,
            bool allowSociallyImproper = false,
            bool allowHarvest = false,
            bool forceScanWholeMap = false,
            bool ignoreReservations = false,
            FoodPreferability minPrefOverride = FoodPreferability.Undefined)
        {
            bool canDoManipulation = getter.IsToolUser() && getter.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            bool allowDrug = !eater.IsTeetotaler();
            Thing thing1 = null;
            if (canUseInventory)
            {
                if (canDoManipulation)
                    thing1 = FoodUtility.BestFoodInInventory(getter, eater,
                                                             minPrefOverride == FoodPreferability.Undefined
                                                                 ? FoodPreferability.MealAwful
                                                                 : minPrefOverride);
                if (thing1 != null)
                {
                    if (getter.Faction != Faction.OfPlayer)
                    {
                        foodSource = thing1;
                        foodDef = FoodUtility.GetFinalIngestibleDef(foodSource);
                        return true;
                    }

                    var comp = thing1.TryGetComp<CompRottable>();
                    if (comp != null && comp.Stage == RotStage.Fresh && comp.TicksUntilRotAtCurrentTemp < 30000)
                    {
                        foodSource = thing1;
                        foodDef = FoodUtility.GetFinalIngestibleDef(foodSource);
                        return true;
                    }
                }
            }

            Pawn getter1 = getter;
            Pawn eater1 = eater;
            int num1 = desperate ? 1 : 0;
            ThingDef foodDef1 = null;
            ref ThingDef local = ref foodDef1;
            int num2 = getter == eater ? 1 : 0;
            int num3 = allowDrug ? 1 : 0;
            bool flag2 = allowForbidden;
            int num4 = allowCorpse ? 1 : 0;
            int num5 = canRefillDispenser ? 1 : 0;
            int num6 = flag2 ? 1 : 0;
            int num7 = allowSociallyImproper ? 1 : 0;
            int num8 = allowHarvest ? 1 : 0;
            int num9 = forceScanWholeMap ? 1 : 0;
            int num10 = ignoreReservations ? 1 : 0;
            var num11 = (int) minPrefOverride;
            Thing foodSource1 = BestFoodSourceOnMapOptimized(getter1, eater1, num1 != 0, out local,
                                                             FoodPreferability.MealLavish, num2 != 0, num3 != 0, num4 != 0,
                                                             true, num5 != 0, num6 != 0, num7 != 0, num8 != 0, num9 != 0,
                                                             num10 != 0, (FoodPreferability) num11);
            if (thing1 != null || foodSource1 != null)
            {
                if (thing1 == null && foodSource1 != null)
                {
                    foodSource = foodSource1;
                    foodDef = foodDef1;
                    return true;
                }

                ThingDef finalIngestibleDef = FoodUtility.GetFinalIngestibleDef(thing1);
                if (foodSource1 == null)
                {
                    foodSource = thing1;
                    foodDef = finalIngestibleDef;
                    return true;
                }

                if (FoodUtility.FoodOptimality(eater, foodSource1, foodDef1,
                                               (getter.Position - foodSource1.Position).LengthManhattan)
                  > (double) (FoodUtility.FoodOptimality(eater, thing1, finalIngestibleDef, 0.0f) - 32f))
                {
                    foodSource = foodSource1;
                    foodDef = foodDef1;
                    return true;
                }

                foodSource = thing1;
                foodDef = FoodUtility.GetFinalIngestibleDef(foodSource);
                return true;
            }

            if (canUseInventory & canDoManipulation)
            {
                Thing thing2 = FoodUtility.BestFoodInInventory(getter, eater, FoodPreferability.DesperateOnly,
                                                               FoodPreferability.MealLavish, 0.0f, allowDrug);
                if (thing2 != null)
                {
                    foodSource = thing2;
                    foodDef = FoodUtility.GetFinalIngestibleDef(foodSource);
                    return true;
                }
            }

            if (foodSource1 == null && getter == eater && (getter.RaceProps.predator || getter.IsWildMan() && !getter.IsPrisoner))
            {
                Pawn huntForPredator = BestPawnToHuntForPredator(getter, forceScanWholeMap);
                if (huntForPredator != null)
                {
                    foodSource = huntForPredator;
                    foodDef = FoodUtility.GetFinalIngestibleDef(foodSource);
                    return true;
                }
            }

            foodSource = null;
            foodDef = null;
            return false;
        }

        private static Pawn BestPawnToHuntForPredator(Pawn predator, bool forceScanWholeMap)
        {
            if (predator.meleeVerbs.TryGetMeleeVerb(null) == null)
                return null;
            bool flag = predator.health.summaryHealth.SummaryHealthPercent < 0.25;
            tmpPredatorCandidates.Clear();
            if (GetMaxRegionsToScan(predator, forceScanWholeMap, predator.needs.food.CurCategory) < 0)
            {
                tmpPredatorCandidates.AddRange(predator.Map.mapPawns.AllPawnsSpawned);
            }
            else
            {
                TraverseParms traverseParms = TraverseParms.For(predator);
                RegionTraverser.BreadthFirstTraverse(predator.Position, predator.Map,
                                                     (from, to) => to.Allows(traverseParms, true), x =>
                                                     {
                                                         List<Thing> thingList =
                                                             x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                                                         foreach (Thing t in thingList)
                                                             tmpPredatorCandidates.Add((Pawn) t);

                                                         return false;
                                                     });
            }

            var pawn = (Pawn) null;
            var num = 0.0f;
            bool tutorialMode = TutorSystem.TutorialMode;
            for (var index = 0; index < tmpPredatorCandidates.Count; ++index)
            {
                Pawn predatorCandidate = tmpPredatorCandidates[index];
                if (predator.GetRoom() == predatorCandidate.GetRoom()
                 && predator != predatorCandidate
                 && (!flag || predatorCandidate.Downed)
                 && FoodUtility.IsAcceptablePreyFor(predator, predatorCandidate)
                 && predator.CanReach((LocalTargetInfo) predatorCandidate, PathEndMode.ClosestTouch, Danger.Deadly)
                 && !predatorCandidate.IsForbidden(predator)
                 && (!tutorialMode || predatorCandidate.Faction != Faction.OfPlayer))
                {
                    float preyScoreFor = FoodUtility.GetPreyScoreFor(predator, predatorCandidate);
                    if (preyScoreFor > (double) num || pawn == null)
                    {
                        num = preyScoreFor;
                        pawn = predatorCandidate;
                    }
                }
            }

            tmpPredatorCandidates.Clear();
            return pawn;
        }

        private static bool CanEatPlants(Pawn eater, bool allowPlant, HungerCategory category)
        {
            bool retval;
            //humanlikes will never eat plants unless they are urgently hungry or starving
            if (eater.IsHumanlike() && category < HungerCategory.UrgentlyHungry)
                retval = false;
            else
                retval = ((uint) (eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) > 0U) & allowPlant;

            return retval;
        }

        private static int GetMaxRegionsToScan(Pawn getter, bool forceScanWholeMap, HungerCategory curCategory)
        {
            if (forceScanWholeMap)
                return -1;
            if (getter.IsHumanlike()) //make sure human likes don't always scan the whole map 
            {
                int retVal;
                switch (curCategory)
                {
                    case HungerCategory.Fed:
                    case HungerCategory.Hungry:
                        retVal = -1;
                        break;
                    case HungerCategory.UrgentlyHungry:
                        retVal = 200;
                        break;
                    case HungerCategory.Starving:
                        retVal = 100;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(curCategory), curCategory, null);
                }

                return retVal;
            }

            return getter.Faction == Faction.OfPlayer ? 100 : 30;
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
                return null;
            Pawn pawn = traverseParams.pawn ?? eater;
            var num1 = 0;
            var num2 = 0;
            var thing = (Thing) null;
            float num3 = float.MinValue;
            for (var index = 0; index < searchSet.Count; ++index)
            {
                Thing search = searchSet[index];
                ++num2;
                var lengthManhattan = (float) (root - search.Position).LengthManhattan;
                if (lengthManhattan <= (double) maxDistance)
                {
                    float num4 = FoodUtility.FoodOptimality(eater, search, FoodUtility.GetFinalIngestibleDef(search),
                                                            lengthManhattan);
                    if (num4 >= (double) num3
                     && pawn.Map.reachability.CanReach(root, (LocalTargetInfo) search, peMode, traverseParams)
                     && search.Spawned
                     && (validator == null || validator(search)))
                    {
                        thing = search;
                        num3 = num4;
                        ++num1;
                    }
                }
            }

            return thing;
        }
    }

    /// <summary>
    ///     the status of the cannibal thought to receive
    /// </summary>
    public enum CannibalThoughtStatus
    {
        /// <summary>
        ///     the pawn did not eat anything they would consider cannibalism
        /// </summary>
        None,

        /// <summary>
        ///     The pawn directly ate something they would consider cannibalism
        /// </summary>
        Direct,

        /// <summary>
        ///     the pawn ate something they would consider cannibalism as an ingredient
        /// </summary>
        Ingredient
    }
}