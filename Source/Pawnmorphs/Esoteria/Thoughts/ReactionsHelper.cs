// ReactionsHelper.cs modified by Iron Wolf for Pawnmorph on 07/30/2019 2:48 PM
// last updated 07/30/2019  2:48 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
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

			PermanentlyFeral,

			Merged
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
				case EventType.Merged:
					return isFemale ? modExtension.mergedThoughtFemale : modExtension.mergedThought;
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
			//if (PawnUtility.ShouldGetThoughtAbout(reactorPawn, originalPawn)) return null;
			int opinion = reactorPawn.relations.OpinionOf(originalPawn);

			if (opinion <= -20)
				switch (type)
				{
					case EventType.Transformation:
						return ThoughtDefOfs.RivalTransformedThought;
					case EventType.Reverted:
						return ThoughtDefOfs.RivalRevertedThought;
					case EventType.PermanentlyFeral:
						return ThoughtDefOfs.RivalPermFeralThought;
					case EventType.Merged:
						return ThoughtDefOfs.RivalMergedThought;
					default:
						throw new ArgumentOutOfRangeException(nameof(type), type, null);
				}

			if (opinion >= 20)
				switch (type)
				{
					case EventType.Transformation:
						return ThoughtDefOfs.FriendTransformedThought;
					case EventType.Reverted:
						return ThoughtDefOfs.FriendRevertedThought;
					case EventType.PermanentlyFeral:
						return ThoughtDefOfs.FriendPermFeralThought;
					case EventType.Merged:
						return ThoughtDefOfs.FriendMergedThought;
					default:
						throw new ArgumentOutOfRangeException(nameof(type), type, null);
				}

			return null;
		}

		/// <summary>
		/// call when the original pawn transforms into the transformedPawn
		/// </summary>
		/// note: this does nothing if ideology is active, instead reaction thoughts are given by precepts instead 
		/// <param name="original">The original.</param>
		/// <param name="transformedPawn">The transformed pawn.</param>
		/// <param name="reactionStatus">The reaction status.</param>
		public static void OnPawnTransforms(Pawn original, Pawn transformedPawn, FormerHumanReactionStatus reactionStatus = FormerHumanReactionStatus.Wild)
		{
			if (ModsConfig.IdeologyActive) return;
			HandleColonistReactions(original, transformedPawn, reactionStatus, EventType.Transformation);

			HandleRelatedPawnsReaction(original, transformedPawn, EventType.Transformation);
		}



		/// <summary>
		/// call when a pawn is reverted from an animal to handle giving the correct thoughts to colonists
		/// </summary>
		/// note: this does nothing if ideology is active, instead reaction thoughts are given by precepts instead 
		/// <param name="originalPawn">The original pawn.</param>
		/// <param name="animalPawn">The animal pawn.</param>
		/// <param name="reactionStatus">The reaction status of the original pawn</param>
		public static void OnPawnReverted(Pawn originalPawn, Pawn animalPawn, FormerHumanReactionStatus reactionStatus = FormerHumanReactionStatus.Wild)
		{
			if (ModsConfig.IdeologyActive) return;
			HandleColonistReactions(originalPawn, animalPawn, reactionStatus, EventType.Reverted);
			HandleRelatedPawnsReaction(originalPawn, animalPawn, EventType.Reverted);
		}

		/// <summary>
		/// call when an animal goes permanently feral to handle giving the correct thoughts to colonists
		/// </summary>
		/// note: this does nothing if ideology is active, instead reaction thoughts are given by precepts instead 
		/// <param name="originalPawn">The original pawn.</param>
		/// <param name="animalPawn">The animal pawn.</param>
		/// <param name="reactionStatus">The reaction status.</param>
		/// <exception cref="ArgumentNullException">originalPawn
		/// or
		/// animalPawn</exception>
		public static void OnPawnPermFeral([NotNull] Pawn originalPawn, [NotNull] Pawn animalPawn, FormerHumanReactionStatus reactionStatus = FormerHumanReactionStatus.Wild)
		{
			if (originalPawn == null) throw new ArgumentNullException(nameof(originalPawn));
			if (animalPawn == null) throw new ArgumentNullException(nameof(animalPawn));
			if (originalPawn.Faction == Faction.OfPlayer || animalPawn.Faction == Faction.OfPlayer) //only give thoughts to colonists if the original pawn or animal is part of the player faction 
				HandleColonistReactions(originalPawn, animalPawn, reactionStatus, EventType.PermanentlyFeral);
			HandleRelatedPawnsReaction(originalPawn, animalPawn, EventType.PermanentlyFeral);
		}

		/// <summary>
		///     call when 2 pawns are merged into one meld/merge to handle giving the correct thoughts to colonists
		/// </summary>
		/// <param name="merge0">the first pawn of the merge</param>
		/// <param name="wasPrisoner0">if the first pawn was a prisoner</param>
		/// <param name="merge1">the second pawn of the merge</param>
		/// <param name="wasPrisoner1">if the second pawn was a prisoner</param>
		/// <param name="animalPawn">the resulting animal pawn</param>
		public static void OnPawnsMerged(Pawn merge0, bool wasPrisoner0, Pawn merge1, bool wasPrisoner1, Pawn animalPawn) //TODO take reaction status into account 
		{
			_scratchList.Clear();
			_scratchList.AddRange(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists
											 .Where(p => p != merge0 && p != merge1));
			//don't give the merged pawns thoughts about themselves 

			HandleColonistReactions(merge0, animalPawn, wasPrisoner0 ? FormerHumanReactionStatus.Prisoner : FormerHumanReactionStatus.Colonist, EventType.Merged, _scratchList);
			HandleColonistReactions(merge1, animalPawn, wasPrisoner1 ? FormerHumanReactionStatus.Prisoner : FormerHumanReactionStatus.Colonist, EventType.Merged, _scratchList);


			//need to handle relationships manually 

			_scratchList.Clear();

			bool IsValidRelation(Pawn p)
			{
				return p != merge0 && p != merge1 && p.needs?.mood != null && p.relations != null;
			}


			IEnumerable<Pawn> linq = merge0.relations.PotentiallyRelatedPawns.Where(IsValidRelation);


			foreach (Pawn rPawn in linq)
			{
				PawnRelationDef relation = rPawn.GetMostImportantRelation(merge0);
				var modExt = relation?.GetModExtension<RelationshipDefExtension>();
				if (modExt == null) continue;

				ThoughtDef thought = modExt.GetThoughtDef(EventType.Merged, merge0.gender);
				if (thought == null)
				{
					Log.Warning($"relationship {relation.defName} has (pawnmorpher) extension but no thought for morphing, is this intentional?");
					continue;
				}

				if (ThoughtUtility.CanGetThought(rPawn, thought))
					rPawn.TryGainMemory(thought);
			}

			foreach (Pawn rPawn in merge1.relations.PotentiallyRelatedPawns.Where(IsValidRelation))
			{
				PawnRelationDef relation = rPawn.GetMostImportantRelation(merge1);
				var modExt = relation?.GetModExtension<RelationshipDefExtension>();
				if (modExt == null) continue;

				ThoughtDef thought = modExt.GetThoughtDef(EventType.Merged, merge0.gender);
				if (thought == null)
				{
					Log.Warning($"relationship {relation.defName} has (pawnmorpher) extension but no thought for morphing, is this intentional?");
					continue;
				}

				if (ThoughtUtility.CanGetThought(rPawn, thought))
					rPawn.TryGainMemory(thought);
			}
		}

		private static readonly List<Pawn> _scratchList = new List<Pawn>();


		private static void HandleColonistReactions(Pawn original, Pawn transformedPawn, FormerHumanReactionStatus reactionStatus, EventType type,
													IEnumerable<Pawn> pawns = null)
		{


			pawns = pawns
				 ?? PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists
							   .Where(p => p != original); //use all colonists except the original pawn as the default 



			ThoughtDef defaultThought;
			switch (type)
			{
				case EventType.Transformation:
					defaultThought = ThoughtDefOfs.DefaultTransformationReaction;
					break;
				case EventType.Reverted:
					defaultThought = ThoughtDefOfs.DefaultRevertedPawnReaction;
					break;
				case EventType.PermanentlyFeral:
					defaultThought = ThoughtDefOfs.DefaultPermanentlyFeralReaction;
					break;
				case EventType.Merged:
					defaultThought = ThoughtDefOfs.DefaultMergedThought;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			int defaultStage = (int)reactionStatus;
			defaultStage = Mathf.Min(defaultStage, (defaultThought?.stages?.Count - 1) ?? 0);


			foreach (Pawn reactor in pawns)
			{
				if (type == EventType.PermanentlyFeral && ModsConfig.IdeologyActive && reactor.Ideo?.HasPositionOn(PMIssueDefOf.PM_SapienceLoss) == true) continue; //don't give default thoughts to pawns whos ideo handles reversion thoughts 
				if (reactor == transformedPawn) continue; //make sure pawns don't react to themselves as if they were a different pawn 
				ThoughtDef opinionThought = GetOpinionThought(original, reactor, type);
				ThoughtDef def;
				if (opinionThought != null)
				{
					def = opinionThought;
					defaultStage = 0;
				}
				else
				{
					def = defaultThought;
				}

				if (def == null) continue;
				if (ThoughtUtility.CanGetThought(reactor, def))
				{
					var memory = ThoughtMaker.MakeThought(def, defaultStage);
					reactor.TryGainMemory(memory);
				}
			}
		}

		private static void
			HandleRelatedPawnsReaction(Pawn original, Pawn animalPawn,
									   EventType type) //use PotentiallyRelatedPawns to get all relationships not DirectRelations for some reason
		{
			foreach (Pawn pReactor in original.relations.PotentiallyRelatedPawns)
			{
				if (pReactor == animalPawn) continue; //make sure pawns don't react to themselves as if it were a different pawn
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

					if (ThoughtUtility.CanGetThought(pReactor, thought))
						pReactor.TryGainMemory(thought);
				}
			}

			//TODO handle bond animal reverted? 
		}
	}

	/// <summary>
	/// enum for tracking the original status of a transformed pawn for colonist reactions 
	/// </summary>
	public enum FormerHumanReactionStatus
	{
		/// <summary>
		/// The original status of the former human is unknown 
		/// </summary>
		Wild,
		/// <summary>
		/// The original pawn was a colonist 
		/// </summary>
		Colonist,
		/// <summary>
		/// the original pawn was a prisoner 
		/// </summary>
		Prisoner,
		/// <summary>
		/// The original pawn was a guest 
		/// </summary>
		Guest
	}

}