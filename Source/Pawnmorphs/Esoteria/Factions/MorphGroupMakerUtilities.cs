// MorphGroupMakerUtilities.cs created by Iron Wolf for Pawnmorph on 10/30/2019 11:18 AM
// last updated 10/30/2019  11:18 AM

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph.Factions
{
    /// <summary>
    ///     static container for applying mutations and morphs to pawns during generation
    /// </summary>
    public class MorphGroupMakerUtilities
    {
        /// <summary>Applies the mutations after the pawn has been loaded by the game.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="canApplyRestricted">if set to <c>true</c> allow restricted mutations to be applied.</param>
        public static void ApplyMutationsPostLoad(Pawn pawn, bool canApplyRestricted)
        {
            var kindExtension = pawn.kindDef.GetModExtension<MorphPawnKindExtension>();
            if (kindExtension == null) return;

            List<HediffGiver_Mutation> givers;
            if (canApplyRestricted)
                givers = kindExtension.AllMutationGivers.ToList();
            else
                givers = kindExtension.AllMutationGivers.Where(g => !(g.hediff.GetModExtension<MutationHediffExtension>()
                                                                      ?.IsRestricted
                                                                   ?? false)) //only keep the unrestricted mutations 
                                      .ToList();


            List<HediffGiver_Mutation> toGive;

            int toGiveCount = kindExtension.hediffRange.RandomInRange;
            if (givers.Count <= toGiveCount)
            {
                toGive = givers;
            }
            else
            {
                toGive = new List<HediffGiver_Mutation>();
                for (var i = 0; i < toGiveCount; i++)
                {
                    int rIndex = Rand.Range(0, givers.Count);
                    HediffGiver_Mutation rGiver = givers[rIndex];
                    givers.RemoveAt(rIndex); //remove each giver so it can't be picked again

                    toGive.Add(rGiver);
                }
            }

            foreach (HediffGiver_Mutation giver in toGive) giver.TryApply(pawn, MutagenDefOf.defaultMutagen);

            pawn.CheckRace(false); //don't apply missing mutations to avoid giving restricted mutations and to respect the limit
        }
    }
}