// ReactionsHelper.cs modified by Iron Wolf for Pawnmorph on 07/30/2019 2:48 PM
// last updated 07/30/2019  2:48 PM

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    ///     static class containing a bunch of helper functions related to pawn thought reactions to stuff
    /// </summary>
    public static class ReactionsHelper
    {
        private enum EventType
        {
            Transformation,
            Reverted,

            PermanentlyFeral
            //any others? 
        }


        private static ThoughtDef GetThoughtDef(this RelationshipDefExtension modExtension, EventType type, Gender reactorGender)
        {
            bool isFemale = reactorGender == Gender.Female;
            switch (type)
            {
                case EventType.Transformation:
                    return isFemale ? modExtension.transformThoughtFemale : modExtension.transformThought;
                case EventType.Reverted:
                    return isFemale ? modExtension.revertedThoughtFemale : modExtension.revertedThought;
                case EventType.PermanentlyFeral:
                    return isFemale ? modExtension.permanentlyFeralFemale : modExtension.permanentlyFeral;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }


        private const string DEFAULT_COLONIST_TF_THOUGHT = "ColonistTransformed";
        private const string DEFAULT_PRISONER_TF_THOUGHT = "PrisonerTransformed"; //TODO move these into a defOf static class?  
        private const string RIVAL_TF_THOUGHT = "RivalTransformed";
        private const string FRIEND_TRANSFORMED = "FriendTransformed";

        /// <summary>
        ///     call when the original pawn transforms into the transformedPawn
        /// </summary>
        /// <param name="original"></param>
        /// <param name="transformedPawn"></param>
        /// <param name="wasPrisoner">where they a prisoner of the colony?</param>
        public static void OnPawnTransforms(Pawn original, Pawn transformedPawn, bool wasPrisoner = false)
        {
            IEnumerable<Pawn> pawns =
                PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists
                           .Where(p => p != original); //don't give the original a thought about themselves 


            ThoughtDef defaultDef = wasPrisoner ? ThoughtDef.Named(DEFAULT_PRISONER_TF_THOUGHT) :
                original.IsColonist ? ThoughtDef.Named(DEFAULT_COLONIST_TF_THOUGHT) : null;
            foreach (Pawn reactorPawn in pawns)
            {
                if(defaultDef != null)
                    reactorPawn.TryGainMemory(defaultDef);


                if (PawnUtility.ShouldGetThoughtAbout(reactorPawn, original))
                {
                    int opinion = reactorPawn.relations.OpinionOf(original);

                    if (opinion >= 20)
                    {
                        var thought = new IndividualThoughtToAdd(ThoughtDef.Named(FRIEND_TRANSFORMED), reactorPawn, original,
                                                                 original.relations.GetFriendDiedThoughtPowerFactor(opinion)); 
                        thought.Add();
                    }
                    else if (opinion <= -20)
                    {
                        var thought = new IndividualThoughtToAdd(ThoughtDef.Named(RIVAL_TF_THOUGHT), reactorPawn, original,
                                                                 original.relations.GetRivalDiedThoughtPowerFactor(opinion));

                        thought.Add();
                    }
                }
            }

            foreach (Pawn potentiallyRelatedPawn in original.relations.PotentiallyRelatedPawns
            ) //use PotentiallyRelatedPawns to get all relationships not DirectRelations for some reason
            {
                if (potentiallyRelatedPawn == original) continue;
                if (potentiallyRelatedPawn.needs?.mood == null) continue;
                PawnRelationDef importantRelation = potentiallyRelatedPawn.GetMostImportantRelation(original);
                var modExtension = importantRelation?.GetModExtension<RelationshipDefExtension>();


                if (modExtension != null)
                {
                    ThoughtDef thought = modExtension.GetThoughtDef(EventType.Transformation, original.gender);
                    if (thought == null)
                    {
                        Log.Warning($"relation {importantRelation.defName} has (Pawnmorpher) extension but no thought for transformation! is this intentional?");
                        continue;
                    }

                    potentiallyRelatedPawn.TryGainMemory(thought);
                }
            }
        }
    }
}