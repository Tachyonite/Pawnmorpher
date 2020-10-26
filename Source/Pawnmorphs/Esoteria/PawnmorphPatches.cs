// PawnmorphPatches.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on //2019 
// last updated 08/11/2019  9:54 AM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using AlienRace;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.DefExtensions;
using Pawnmorph.FormerHumans;
using Pawnmorph.HPatches;
using Pawnmorph.Hybrids;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
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
        [NotNull]
        private static readonly MethodInfo _animalTabWorkerMethod; 
        static PawnmorphPatches()
        {

            _animalTabWorkerMethod =
                typeof(PawnmorphPatches).GetMethod(nameof(AnimalTabWorkerMethod), BindingFlags.Static | BindingFlags.NonPublic); 



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
                PatchCaravanUI(harmonyInstance);
            }
            catch (Exception e)
            {
                Log.Error($"Pawnmorpher: encountered {e.GetType().Name} while patching caravan UI delegates\n{e}");
            }

            try
            {
                MassPatchFormerHumanChecks(harmonyInstance); 
            }
            catch (Exception e)
            {
                Log.Error($"Pawnmorpher: encountered {e.GetType().Name} while mass patching former human functions\n{e}");

            }

            try
            {
                ITabPatches.DoPrisonerPatch(harmonyInstance); 
            }
            catch (Exception e)
            {
                Log.Error($"Pawnmorpher: encountered {e.GetType().Name} while patching prisoner tab!\n{e}");
            }

            try
            {

                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                Log.Error($"Pawnmorpher cannot preform harmony patches! caught {e.GetType().Name}\n{e}"); 
            }
        }




        /// <summary>
        /// substitutes all instances of RaceProps Humanlike, Animal, and Tooluser with their equivalent in FormerHumanUtilities
        /// </summary>
        /// <param name="instructions">The code instructions.</param>
        /// <exception cref="System.ArgumentNullException">codeInstructions</exception>
        public static IEnumerable<CodeInstruction> SubstituteFormerHumanMethodsPatch([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            if (instructions == null) throw new ArgumentNullException(nameof(instructions));
            List<CodeInstruction> codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count - 1; i++)
            {
                int j = i + 1;
                CodeInstruction opI = codeInstructions[i];
                CodeInstruction opJ = codeInstructions[j];
                if (opI == null || opJ == null) continue;
                //the segment we're interested in always start with pawn.get_RaceProps() (ie pawn.RaceProps) 
                if (opI.opcode == OpCodes.Callvirt && (MethodInfo)opI.operand == PatchUtilities.RimworldGetRaceMethod)
                {
                    //signatures we care about always have a second callVirt 
                    if (opJ.opcode != OpCodes.Callvirt) continue;

                    var jMethod = opJ.operand as MethodInfo;
                    bool patched;
                    //figure out which method, if any, we're going to be replacing 
                    if (jMethod == PatchUtilities.RimworldGetAnimalMethod)
                    {
                        patched = true;
                        opI.operand = _animalTabWorkerMethod;
                    }
                    else
                    {
                        patched = false;
                    }

                    if (patched)
                    {
                        //now clean up if we did any patching 

                        opI.opcode = OpCodes.Call; //always uses call 

                        //replace opJ with nop (no operation) so we don't fuck up the stack 
                        opJ.opcode = OpCodes.Nop;
                        opJ.operand = null;
                    }
                }
            }

            return codeInstructions;
        }


        static bool AnimalTabWorkerMethod(Pawn pawn)
        {
            return pawn.RaceProps.Animal || pawn.GetIntelligence() == Intelligence.Animal; 
        }

        static void DoAnimalPatches([NotNull] Harmony harmonyInstance)
        {
            var staticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;


            var tpMethod = typeof(PawnmorphPatches).GetMethod(nameof(PawnmorphPatches), staticFlags);
            //animal tabs 
            var methods = typeof(MainTabWindow_Animals).GetNestedTypes(staticFlags | instanceFlags)//looking for delegates used by the animal tab 
                                                       .Where(t => t.IsCompilerGenerated())
                                                       .SelectMany(t => t.GetMethods(instanceFlags).Where(m => m.HasSignature(typeof(Pawn))));

            foreach (MethodInfo methodInfo in methods)
            {
                harmonyInstance.Patch(methodInfo, transpiler: new HarmonyMethod(tpMethod)); 
            }



        }

        private static void MassPatchFormerHumanChecks([NotNull] Harmony harmonyInstance)
        {
            var staticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public; 
            var instanceFlags = BindingFlags.Instance| BindingFlags.NonPublic | BindingFlags.Public;




            List<MethodInfo> methodsToPatch = new List<MethodInfo>(); 
            
            //bed stuff 
            var bedUtilType = typeof(RestUtility);
            var canUseBedMethod = bedUtilType.GetMethod(nameof(RestUtility.CanUseBedEver), staticFlags);
            methodsToPatch.Add(canUseBedMethod); 

     

   
            //map pawns 
            var methods = typeof(MapPawns).GetMethods(instanceFlags).Where(m => m.HasSignature(typeof(Faction)));
            methodsToPatch.AddRange(methods); 

         
            //jobs and toils 
            methodsToPatch.Add(typeof(JobDriver_Ingest).GetMethod("PrepareToIngestToils", instanceFlags));
            methodsToPatch.Add(typeof(GatheringWorker_MarriageCeremony).GetMethod("IsGuest", instanceFlags));
            
            //now patch them 
            foreach (MethodInfo methodInfo in methodsToPatch)
            {
                if(methodInfo == null) continue;
                harmonyInstance.ILPatchCommonMethods(methodInfo); 
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Patched:");
            foreach (MethodInfo methodInfo in methodsToPatch)
            {
                if(methodInfo == null) continue;
                builder.AppendLine($"{methodInfo.Name}");
            }
            Log.Message(builder.ToString());
            DebugLogUtils.LogMsg(LogLevel.Messages, builder.ToString());
        }


        private static void PatchCaravanUI([NotNull] Harmony harmInstance)
        {
            //TODO make patch utilities for mass patching delegates 
            var cUIType = typeof(CaravanUIUtility);
            var flg = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;


            bool CorrectSignature(MethodInfo methodInfo)
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length != 1) return false;
                return parameters[0].ParameterType == typeof(TransferableOneWay) || parameters[0].ParameterType == typeof(Pawn);
            }

            IEnumerable<Type> allInnerTypes = cUIType.GetNestedTypes(flg).Where(IsCompilerGenerated);
            //add in the delegates from LordToil_PrepareCaravan_GatherAnimals
#pragma warning disable 612
            allInnerTypes = allInnerTypes.Concat(typeof(LordToil_PrepareCaravan_GatherAnimals)
#pragma warning restore 612
                                                .GetNestedTypes(flg)
                                                .Where(IsCompilerGenerated));

            var allMethods = allInnerTypes.SelectMany(t => t.GetMethods(flg).Where(CorrectSignature));
            var tsMethodInfo = typeof(PawnmorphPatches).GetMethod(nameof(CaravanDelegatePatch), flg); 
            foreach (MethodInfo methodInfo in allMethods)
            {
                var hMethod = new HarmonyMethod(tsMethodInfo);
                harmInstance.Patch(methodInfo, transpiler: hMethod); 
            }

        }

        

        static IEnumerable<CodeInstruction> CaravanDelegatePatch(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> ilList = instructions.ToList();
            MethodInfo methodSig1 = typeof(Pawn).GetProperty(nameof(Pawn.RaceProps)).GetGetMethod();
            MethodInfo methodSig2 = typeof(RaceProperties).GetProperty(nameof(RaceProperties.Animal)).GetGetMethod();
            MethodInfo replaceMethod = typeof(FormerHumanUtilities).GetMethod(nameof(FormerHumanUtilities.IsAnimal));
            for (var i = 0; i < ilList.Count - 1; i++)
            {
                int j = i + 1;
                CodeInstruction opI = ilList[i];
                CodeInstruction opJ = ilList[j];

                // looking for 
                // pawn.RaceProps.Animal 
                // which is 
                // race =  pawn.get_RaceProps()
                // race.get_Animal()
                if (opI.opcode == OpCodes.Callvirt
                 && (MethodInfo) opI.operand == methodSig1
                 && opJ.opcode == OpCodes.Callvirt
                 && (MethodInfo) opJ.operand == methodSig2)
                {
                    //now replace with one call to FormerHumanUtilities.IsAnimal(Pawn)
                    opI.opcode = OpCodes.Call;
                    opI.operand = replaceMethod;

                    //replace race.get_Animal() op with nop (no operation) so we don't fuck up the stack 
                    opJ.opcode = OpCodes.Nop;
                    opJ.operand = null;
                }
            }

            return ilList;
        }

        static bool IsCompilerGenerated(Type type)
        {
            if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null) return true;
            if (type.Name.Contains(">") || type.Name.Contains("<")) return true; 
            return false; 
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
                    foodThoughts.Add(cannibalThought.thought);
                    return;
                }

                ThingDef comp = foodSource.TryGetComp<CompIngredients>()
                                         ?.ingredients?.FirstOrDefault(def => def == morphDef.race.race.meatDef);
                if (comp != null && !cannibal) foodThoughts.Add(cannibalThought.ingredientThought);
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
            }else if (__instance.pawn.IsSapientFormerHuman())
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