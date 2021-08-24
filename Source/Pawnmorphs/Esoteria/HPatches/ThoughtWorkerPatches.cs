// ThoughtWorkerPatches.cs created by Iron Wolf for Pawnmorph on 07/24/2021 10:56 AM
// last updated 07/24/2021  10:56 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    /// <summary>
    ///     patches thought workers
    /// </summary>
    public static class ThoughtWorkerPatches
    {
        [NotNull]
        private static IEnumerable<Type> AllPreceptNudityThoughts
        {
            get
            {
                IssueDef maleNudity = DefDatabase<IssueDef>.GetNamed("Nudity_Male");
                IssueDef femaleNudity = DefDatabase<IssueDef>.GetNamed("Nudity_Female");

                IEnumerable<PreceptDef> precepts =
                    DefDatabase<PreceptDef>.AllDefs.Where(p => p.issue == maleNudity || p.issue == femaleNudity);
                IEnumerable<PreceptComp_SituationalThought> comps =
                    precepts.SelectMany(p => p.comps.MakeSafe().OfType<PreceptComp_SituationalThought>());
                IEnumerable<Type> types = comps.Select(t => t.thought.workerClass);
                return types.Distinct();
            }
        }

        [NotNull]
        private static IEnumerable<MethodInfo> AllNormalMethods
        {
            get
            {
                return AllPreceptNudityThoughts.Where(tp => !tp.Name.Contains("Social"))
                                               .Select(t => t.GetMethod("ShouldHaveThought",
                                                                        BindingFlags.NonPublic | BindingFlags.Instance));
            }
        }

        [NotNull]
        private static IEnumerable<MethodInfo> AllSocialMethods
        {
            get
            {
                return AllPreceptNudityThoughts.Where(tp => tp.Name.Contains("Social"))
                                               .Select(t => t.GetMethod("ShouldHaveThought",
                                                                        BindingFlags.NonPublic | BindingFlags.Instance));
            }
        }

        /// <summary>
        ///     patches thought worker.
        /// </summary>
        /// <param name="harInstance">The har instance.</param>
        public static void DoPatches([NotNull] Harmony harInstance)
        {
            try
            {
                if (!ModsConfig.IdeologyActive) return;

                BindingFlags stBindingFlags = BindingFlags.NonPublic | BindingFlags.Static;
                MethodInfo normalPatchMethod =
                    typeof(ThoughtWorkerPatches).GetMethod("DisableFormerHumanThoughtNormal", stBindingFlags);
                MethodInfo socialPatchMethod =
                    typeof(ThoughtWorkerPatches).GetMethod("DisableFormerHumanThoughtSocial", stBindingFlags);


                foreach (MethodInfo method in AllNormalMethods)
                    harInstance.Patch(method, postfix: new HarmonyMethod(normalPatchMethod));

                foreach (MethodInfo allSocialMethod in AllSocialMethods)
                    harInstance.Patch(allSocialMethod, postfix: new HarmonyMethod(socialPatchMethod));
            }
            catch (Exception e)
            {
                Log.Error($"unable to perform thought worker patching!\n{e}");
            }
        }


        private static void DisableFormerHumanThoughtNormal(Pawn p, ref ThoughtState __result)
        {
            if (__result.Active && p.IsFormerHuman()) __result = false;
        }

        private static void DisableFormerHumanThoughtSocial(Pawn p, Pawn otherPawn, ref ThoughtState __result)
        {
            if (__result.Active && otherPawn?.IsFormerHuman() == true) __result = false;
        }

        [HarmonyPatch(typeof(ThoughtWorker_Hot), "CurrentStateInternal")]
        private static class FixThoughtWorkerHot
        {
            private static bool Prefix(Pawn p, ref ThoughtState __result)
            {
                if (p.IsAnimal() || ModsConfig.IdeologyActive && p.Ideo == null)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(ThoughtWorker_Cold), "CurrentStateInternal")]
        private static class FixThoughtWorkerCold
        {
            private static bool Prefix(Pawn p, ref ThoughtState __result)
            {
                if (p.IsAnimal() || ModsConfig.IdeologyActive && p.Ideo == null)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(ThoughtWorker_Precept_AnyBodyPartButGroinCovered),
            nameof(ThoughtWorker_Precept_AnyBodyPartButGroinCovered.HasCoveredBodyPartsButGroin))]
        private static class GroinCheckPatch
        {
            private static bool Prefix(Pawn p, ref bool __result)
            {
                if (p.RaceProps.body == BodyDefOf.Human) return true; //if they're human let rimworld take care of it

                BodyUtilities.NudityValues nReport = BodyUtilities.GetNudityValues(p);
                __result = nReport != 0 && (nReport & BodyUtilities.NudityValues.Groin) == 0;
                return false;
            }
        }

        [HarmonyPatch(typeof(ThoughtWorker_Precept_GroinChestHairOrFaceUncovered),
            nameof(ThoughtWorker_Precept_GroinChestHairOrFaceUncovered.HasUncoveredGroinChestHairOrFace))]
        private static class GroinChestHairOrFacePatch
        {
            private static bool Prefix(Pawn p, ref bool __result)
            {
                if (p.RaceProps.body == BodyDefOf.Human) return true; //if they're human let rimworld take care of it

                BodyUtilities.NudityValues nReport = BodyUtilities.GetNudityValues(p);
                __result = nReport != BodyUtilities.NudityValues.GroinHeadOrFace;
                return false;
            }
        }

        [HarmonyPatch(typeof(ThoughtWorker_Precept_GroinOrChestUncovered),
            nameof(ThoughtWorker_Precept_GroinOrChestUncovered.HasUncoveredGroinOrChest))]
        private static class HasUncoveredGroinOrChestPatch
        {
            private static bool Prefix(Pawn p, ref bool __result)
            {
                if (p.RaceProps.body == BodyDefOf.Human) return true; //if they're human let rimworld take care of it

                BodyUtilities.NudityValues nReport = BodyUtilities.GetNudityValues(p);
                __result = (nReport & BodyUtilities.NudityValues.GroinOrChest) != BodyUtilities.NudityValues.GroinOrChest;
                return false;
            }
        }

        //not patching hair or face checks further but this is fine for naga, update this if we need special cases for those 
    }
}