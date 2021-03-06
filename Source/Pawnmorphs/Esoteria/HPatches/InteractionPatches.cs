﻿// InteractionPatches.cs modified by Iron Wolf for Pawnmorph on 12/10/2019 6:31 PM
// last updated 12/10/2019  6:31 PM

using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{

    static class InteractionPatches
    {
       
        [HarmonyPatch(typeof(Pawn_InteractionsTracker)), HarmonyPatch(nameof(Pawn_InteractionsTracker.TryInteractWith))]
        static class TryInteractWithPatch
        {
            [HarmonyPrefix]
            static bool SubstituteInteraction(Pawn recipient, ref InteractionDef intDef, Pawn ___pawn)
            {
                var ext = intDef.GetModExtension<InteractionGroupExtension>();
                InteractionDef alt = ext?.TryGetAlternativeFor(___pawn, recipient);
                if (alt != null)
                {
                    if (DebugLogUtils.ShouldLog(LogLevel.Messages))
                    {

                        var msg = $"substituting {alt.defName} for {intDef.defName} on {___pawn.Name} -> {recipient.Name}";
                        Log.Message(msg); 

                    }
                    
                    
                    intDef = alt;
                }

                return true; 
            }

            [HarmonyPostfix]
            static void AddInteractionThoughts([NotNull] Pawn recipient, [NotNull] InteractionDef intDef, bool __result)
            {
                if (!__result) return;
                if ((recipient.IsFormerHuman() || recipient.GetSapienceState()?.StateDef == SapienceStateDefOf.Animalistic) && recipient.needs?.mood != null)
                {
                    var memory = intDef.GetModExtension<InstinctEffector>()?.thought;  //hacky, should come up with a better solution eventually 
                    if (memory == null) return;

                    if (DebugLogUtils.ShouldLog(LogLevel.Messages))
                    {
                        var msg = $"giving {recipient.Name} memory {memory.defName}";
                        Log.Message(msg); 
                    }


                    //social thoughts to? 
                    recipient.TryGainMemory(memory);
                }
            }
        }

        [HarmonyPatch(typeof(InteractionUtility)), HarmonyPatch(nameof(InteractionUtility.CanReceiveRandomInteraction))]
        static class SapientAnimalsRandomInteractionPatch
        {
            [HarmonyPrefix]
            static bool SapientAnimalPatch([NotNull] Pawn p, ref bool __result)
            {
                if (p.IsFormerHuman() && p.needs?.mood != null)
                {
                    __result = InteractionUtility.CanReceiveInteraction(p) && (!p.Downed && !p.InAggroMentalState);
                    return false; 
                }

                return true; 
            }
        }
    }
}