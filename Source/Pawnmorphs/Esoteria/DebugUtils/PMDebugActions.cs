// DebugActions.cs created by Iron Wolf for Pawnmorph on 03/18/2020 1:42 PM
// last updated 03/18/2020  1:42 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.Jobs;
using Pawnmorph.Social;
using Pawnmorph.TfSys;
using Pawnmorph.ThingComps;
using Pawnmorph.User_Interface;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.DebugUtils
{
    static class PMDebugActions
    {
        private const string PM_CATEGORY = "Pawnmorpher";


        [DebugAction(category = PM_CATEGORY, actionType = DebugActionType.Action)]
        static void TagAllAnimals()
        {
            var gComp = Find.World.GetComponent<PawnmorphGameComp>();


            foreach (var kindDef in DefDatabase<PawnKindDef>.AllDefs)
            {
                var thingDef = kindDef.race; 
                if(thingDef.race?.Animal != true) continue;

                if(!thingDef.IsValidAnimal()) continue;
                if(gComp.taggedAnimals.Contains(kindDef)) continue;
                gComp.TagPawn(kindDef); 

            }

        }

        [DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void TryExitSapienceState(Pawn pawn)
        {
            var sapienceT = pawn?.GetSapienceTracker();
            if (sapienceT?.CurrentState == null) return;
            var stateName = sapienceT.CurrentState.StateDef.defName; 
            try
            {
                sapienceT.ExitState();
            }
            catch (Exception e)
            {
                Log.Error($"caught {e.GetType().Name} while trying to exit sapience state {stateName}!\n{e.ToString().Indented("|\t")}");
            }
        }

        [DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void AddMutation(Pawn pawn)
        {
            if (pawn == null) return;
            Find.WindowStack.Add(new DebugMenu_AddMutations(pawn)); 
        }


        static IEnumerable<DebugMenuOption> GetAddMutationOptions([NotNull] Pawn pawn)
        {

            bool CanAddMutationToPawn(MutationDef mDef)
            {
                if (mDef.parts == null)
                {
                    return pawn.health.hediffSet.GetFirstHediffOfDef(mDef) == null; 
                }

                foreach (BodyPartDef bodyPartDef in mDef.parts)
                {
                    foreach (BodyPartRecord record in pawn.GetAllNonMissingParts().Where(p => p.def == bodyPartDef))
                    {
                        if (!pawn.health.hediffSet.HasHediff(mDef, record)) return true; 
                    }
                }

                return false; 
            }

            void CreateMutationDialog(MutationDef mDef)
            {
                var nMenu = new DebugMenu_AddMutation(mDef, pawn);
                Find.WindowStack.Add(nMenu); 
            }


            foreach (MutationDef mutation in MutationDef.AllMutations.Where(CanAddMutationToPawn))
            {
                var tMu = mutation;
                yield return new DebugMenuOption(mutation.label, DebugMenuOptionMode.Action, () => CreateMutationDialog(tMu)); 
            }

        }

        [DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void RecruitFormerHuman(Pawn pawn)
        {
            var sapienceState = pawn?.GetSapienceState();
            if (sapienceState?.IsFormerHuman == true)
            {
                Worker_FormerHumanRecruitAttempt.DoRecruit(pawn.Map.mapPawns.FreeColonists.FirstOrDefault(), pawn, 1f);
                DebugActionsUtility.DustPuffFrom(pawn);
            }
        }

        [DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void ReduceSapience(Pawn pawn)
        {
            var sTracker = pawn?.GetComp<SapienceTracker>();
            if (sTracker == null) return; 

            sTracker.SetSapience(Mathf.Max(0, sTracker.Sapience -0.2f ));
        }

        [DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void IncreaseSapience(Pawn pawn)
        {
            var sTracker = pawn?.GetComp<SapienceTracker>();
            if (sTracker == null) return;

            sTracker.SetSapience(sTracker.Sapience + 0.2f);

        }

        [DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void MakeAnimalSapientFormerHuman(Pawn pawn)
        {
            if (pawn == null) return;
            if (pawn.GetSapienceState() != null) return;
            if (!pawn.RaceProps.Animal) return;

            FormerHumanUtilities.MakeAnimalSapient(pawn); 

        }
        [DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void MakeAnimalFormerHuman(Pawn pawn)
        {
            if (pawn == null) return;
            if (pawn.GetSapienceState() != null) return;
            if (!pawn.RaceProps.Animal) return;

            FormerHumanUtilities.MakeAnimalSapient(pawn, Rand.Range(0.1f, 1f));

        }

        [DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void TryRevertTransformedPawn(Pawn pawn)
        {
            if (pawn == null) return;
            var gComp = Find.World.GetComponent<PawnmorphGameComp>();
            (TransformedPawn pawn, TransformedStatus status)? tfPawn = gComp?.GetTransformedPawnContaining(pawn);
            TransformedPawn transformedPawn = tfPawn?.pawn;
            if (transformedPawn == null || tfPawn?.status != TransformedStatus.Transformed) return;
            MutagenDef mut = transformedPawn.mutagenDef ?? MutagenDefOf.defaultMutagen;
            mut.MutagenCached.TryRevert(transformedPawn); 
        }


        [DebugAction("General","Explosion (mutagenic small)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void SmallExplosionMutagenic()
        {
            GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 10, PMDamageDefOf.MutagenCloud, null); 
        }

        [DebugAction("General", "Explosion (mutagenic large)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void ExplosionMutagenic()
        {
            GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 10, PMDamageDefOf.MutagenCloud_Large, null);
        }

        [DebugAction("Pawnmorpher", "Open action menu", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void OpenActionMenu()
        {
            Find.WindowStack.Add(new Pawnmorpher_DebugDialogue());
        }

        [DebugAction(category=PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void OpenPartPickerMenu(Pawn pawn)
        {
            if (pawn == null) return;
            Find.WindowStack.Add(new Dialog_PartPicker(pawn));
        }
    }
}