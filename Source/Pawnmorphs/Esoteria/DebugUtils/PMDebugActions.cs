// DebugActions.cs created by Iron Wolf for Pawnmorph on 03/18/2020 1:42 PM
// last updated 03/18/2020  1:42 PM

using System;
using System.Linq;
using Pawnmorph.Jobs;
using Pawnmorph.Social;
using Pawnmorph.TfSys;
using Pawnmorph.ThingComps;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.DebugUtils
{
    static class PMDebugActions
    {
        private const string FORMER_HUMAN_CATEGORY = "Former Humans";

        [DebugAction(category = FORMER_HUMAN_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void RecruitFormerHuman(Pawn pawn)
        {
            var sapienceState = pawn?.GetSapienceState();
            if (sapienceState?.StateDef == SapienceStateDefOf.FormerHuman)
            {
                Worker_FormerHumanRecruitAttempt.DoRecruit(pawn.Map.mapPawns.FreeColonists.FirstOrDefault(), pawn, 1f);
                DebugActionsUtility.DustPuffFrom(pawn);
            }
        }

        [DebugAction(category = FORMER_HUMAN_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void ReduceSapience(Pawn pawn)
        {
            var sTracker = pawn?.GetComp<SapienceTracker>();
            if (sTracker == null) return; 

            sTracker.SetSapience(Mathf.Max(0, sTracker.Sapience -0.2f ));
        }

        [DebugAction(category = FORMER_HUMAN_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void MakeAnimalSapientFormerHuman(Pawn pawn)
        {
            if (pawn == null) return;
            if (pawn.GetSapienceState() != null) return;
            if (!pawn.RaceProps.Animal) return;

            FormerHumanUtilities.MakeAnimalSapient(pawn); 

        }
        [DebugAction(category = FORMER_HUMAN_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
        static void MakeAnimalFormerHuman(Pawn pawn)
        {
            if (pawn == null) return;
            if (pawn.GetSapienceState() != null) return;
            if (!pawn.RaceProps.Animal) return;

            FormerHumanUtilities.MakeAnimalSapient(pawn, Rand.Range(0.1f, 1f));

        }

        [DebugAction(category = FORMER_HUMAN_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
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
    }
}