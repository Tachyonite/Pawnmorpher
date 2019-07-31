// ReactionsHelper.cs modified by Iron Wolf for Pawnmorph on 07/30/2019 2:48 PM
// last updated 07/30/2019  2:48 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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


        /// <summary>
        ///     get a thought related the reactor pawn's opinion of the original pawn
        /// </summary>
        /// <param name="originalPawn"></param>
        /// <param name="reactorPawn"></param>
        /// <param name="type"></param>
        /// <returns>
        ///     the thought def, null if there is no specific thoughtDef or the reactor has no special opinion of the original
        ///     pawn
        /// </returns>
        [CanBeNull]
        private static ThoughtDef GetOpinionThought(Pawn originalPawn, Pawn reactorPawn, EventType type)
        {
            if (PawnUtility.ShouldGetThoughtAbout(reactorPawn, originalPawn)) return null;
            int opinion = reactorPawn.relations.OpinionOf(originalPawn);

            if (opinion <= -20)
                switch (type)
                {
                    case EventType.Transformation:
                        return ReactionDefs.RivalTransformedThought;
                    case EventType.Reverted:
                        return ReactionDefs.RivalRevertedThought;
                    case EventType.PermanentlyFeral:
                        return ReactionDefs.RivalPermFeralThought;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

            if (opinion >= 20)
                switch (type)
                {
                    case EventType.Transformation:
                        return ReactionDefs.FriendTransformedThought;
                    case EventType.Reverted:
                        return ReactionDefs.FriendRevertedThought;
                    case EventType.PermanentlyFeral:
                        return ReactionDefs.FriendPermFeralThought;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

            return null;
        }

        /// <summary>
        ///     call when the original pawn transforms into the transformedPawn
        /// </summary>
        /// <param name="original"></param>
        /// <param name="transformedPawn"></param>
        /// <param name="wasPrisoner">where they a prisoner of the colony?</param>
        public static void OnPawnTransforms(Pawn original, Pawn transformedPawn, bool wasPrisoner = false)
        {
            HandleColonistReactions(original, transformedPawn, wasPrisoner, EventType.Transformation);

            HandleRelatedPawnsReaction(original, transformedPawn, EventType.Transformation);
        }

        /// <summary>
        ///     call when a pawn is reverted from an animal to handle giving the correct thoughts to colonists
        /// </summary>
        /// <param name="originalPawn"></param>
        /// <param name="animalPawn"></param>
        public static void OnPawnReverted(Pawn originalPawn, Pawn animalPawn)
        {
            HandleColonistReactions(originalPawn, animalPawn, false, EventType.Reverted);
            HandleRelatedPawnsReaction(originalPawn, animalPawn, EventType.Reverted);
        }

        /// <summary>
        ///     call when an animal goes permanently feral to handle giving the correct thoughts to colonists
        /// </summary>
        /// <param name="originalPawn"></param>
        /// <param name="animalPawn"></param>
        public static void OnPawnPermFeral(Pawn originalPawn, Pawn animalPawn)
        {
            HandleColonistReactions(originalPawn, animalPawn, false, EventType.PermanentlyFeral);
            HandleRelatedPawnsReaction(originalPawn, animalPawn, EventType.PermanentlyFeral);
        }

        private static void HandleColonistReactions(Pawn original, Pawn transformedPawn, bool wasPrisoner, EventType type)
        {
            ThoughtDef defaultThought;
            if (original.IsColonist || wasPrisoner)
                switch (type)
                {
                    case EventType.Transformation:
                        defaultThought = wasPrisoner
                            ? ReactionDefs.PrisonerTransformedThought
                            : ReactionDefs.ColonistTransformedThought;
                        break;
                    case EventType.Reverted:
                        defaultThought = wasPrisoner ? null : ReactionDefs.ColonistRevertedThought;
                        break;
                    case EventType.PermanentlyFeral:
                        defaultThought = wasPrisoner ? null : ReactionDefs.ColonistPermFeralThought;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            else
                defaultThought = null;


            IEnumerable<Pawn> pawns =
                PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists
                           .Where(p => p != original); //don't give the original a thought about themselves 

            foreach (Pawn reactor in pawns)
            {
                ThoughtDef opinionThought = GetOpinionThought(original, reactor, type);
                ThoughtDef def = opinionThought ?? defaultThought;
                if (def != null) reactor.TryGainMemory(def);
            }
        }

        private static void
            HandleRelatedPawnsReaction(Pawn original, Pawn animalPawn,
                                       EventType type) //use PotentiallyRelatedPawns to get all relationships not DirectRelations for some reason
        {
            foreach (Pawn pReactor in original.relations.PotentiallyRelatedPawns)
            {
                if (pReactor == original) continue;
                if (pReactor.needs?.mood == null) continue;
                PawnRelationDef importantRelation = pReactor.GetMostImportantRelation(original);
                var modExt = importantRelation?.GetModExtension<RelationshipDefExtension>();

                if (modExt != null)
                {
                    ThoughtDef thought = modExt.GetThoughtDef(type, original.gender);
                    if (thought == null)
                    {
                        Log.Warning($"relation {importantRelation.defName} has (Pawnmorpher) extension but no thought for {type}, is this intentional?");
                        continue;
                    }

                    pReactor.TryGainMemory(thought);
                }
            }

            //TODO handle bond animal reverted? 
        }
    }
}