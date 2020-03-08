// PawnmorphPatches.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on //2019 
// last updated 08/11/2019  9:54 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AlienRace;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.FormerHumans;
using Pawnmorph.Hybrids;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
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
            Harmony.DEBUG = true; 

            var
                harmonyInstance = new Harmony("com.pawnmorpher.mod"); //shouldn't this be different? 
            harmonyInstance.Patch(
                                  AccessTools.Method(typeof(FoodUtility),
                                                     nameof(FoodUtility.ThoughtsFromIngesting)),
                                  null,
                                  new HarmonyMethod(patchType, nameof(ThoughtsFromIngestingPostfix))
                                 );

#if true 
            // Job patches.
           harmonyInstance.Patch(AccessTools.Method(typeof(JobDriver_Ingest),
                                                     "PrepareToIngestToils"),
                                  new HarmonyMethod(patchType, nameof(PrepareToIngestToilsPrefix))
                                 ); 
#endif



            try
            {

                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                Log.Error($"Pawnmorpher cannot preform harmony patches! caught {e.GetType().Name}\n{e}"); 
            }
        }

        [NotNull]
        private static  readonly List<IFoodThoughtModifier> _modifiersCache = new List<IFoodThoughtModifier>();

        class FoodModifierComparer : IComparer<IFoodThoughtModifier>
        {
            /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
            /// <returns>Value Condition Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.</returns>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            public int Compare(IFoodThoughtModifier x, IFoodThoughtModifier y)
            {
                if (ReferenceEquals(x, y)) return 0; 
                if (x == null) return -1;
                if (y == null) return 1;
                return x.Priority.CompareTo(y.Priority); 
            }
        }
        
        [NotNull] private static FoodModifierComparer ModComparer { get; } = new FoodModifierComparer();

        public static void ThoughtsFromIngestingPostfix(Pawn ingester, Thing foodSource, ref List<ThoughtDef> __result)
        {
            ApplyMorphFoodThoughts(ingester, foodSource, __result);

            //now apply any modifiers if present 

            //first clear the cache 
            _modifiersCache.Clear();
            //add any hediffs 
            var hModifiers = ingester.health?.hediffSet?.hediffs?.OfType<IFoodThoughtModifier>(); 
            //any hediff stages 
            var hStageModifiers = ingester.health?.hediffSet?.hediffs?.Select(h => h.CurStage)
                                          .Where(s => s != null)
                                          .OfType<IFoodThoughtModifier>();
            //now all aspects 
            var aModifiers = ingester.GetAspectTracker()?.Aspects.OfType<IFoodThoughtModifier>();

            //now add them all to the cache 
            _modifiersCache.AddRange(hModifiers.MakeSafe());
            _modifiersCache.AddRange(hStageModifiers.MakeSafe());
            _modifiersCache.AddRange(aModifiers.MakeSafe()); 
            //now sort by priority 
            _modifiersCache.Sort(ModComparer); 

            //now apply them all in order 
            foreach (IFoodThoughtModifier foodThoughtModifier in _modifiersCache)
            {
                foodThoughtModifier.ModifyThoughtsFromFood(foodSource, __result); 
            }

        }

        private static void ApplyMorphFoodThoughts(Pawn ingester, Thing foodSource, List<ThoughtDef> foodThoughts)
        {
            if (RaceGenerator.TryGetMorphOfRace(ingester.def, out MorphDef morphDef))
            {
                AteThought cannibalThought = morphDef.raceSettings?.thoughtSettings?.ateAnimalThought;
                if (cannibalThought == null) return;
                bool cannibal = ingester.story.traits.HasTrait(TraitDefOf.Cannibal);

                if (foodSource.def == morphDef.race.race.meatDef && !cannibal)
                {
                    foodThoughts.Add(ThoughtDef.Named(cannibalThought.thought));
                    return;
                }

                ThingDef comp = foodSource.TryGetComp<CompIngredients>()
                                         ?.ingredients?.FirstOrDefault(def => def == morphDef.race.race.meatDef);
                if (comp != null && !cannibal) foodThoughts.Add(ThoughtDef.Named(cannibalThought.ingredientThought));
            }
            else
            {
                 var fHStatus = ingester.GetQuantizedSapienceLevel();
                if (fHStatus == null || fHStatus == SapienceLevel.PermanentlyFeral) return;

                ThoughtDef thought = GetCannibalThought(ingester, foodSource);
                if (thought != null) foodThoughts.Add(thought);
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

        private static bool PrepareToIngestToilsPrefix(JobDriver __instance, Toil chewToil, ref IEnumerable<Toil> __result) //TODO figure out how to turn this into a transpiler patch 
        {
            Thing thing = __instance.job.targetA.Thing;
            if (RaceGenerator.TryGetMorphOfRace(__instance.pawn.def, out MorphDef def))
            {
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
            }else if (__instance.pawn.IsSapientOrFeralFormerHuman())
            {
                if (thing.def.ingestible == null) return true;

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