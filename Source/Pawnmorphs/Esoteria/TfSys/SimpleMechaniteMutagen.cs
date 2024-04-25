// SimpleMechaniteMutagen.cs modified by Iron Wolf Pawnmorph on 08/14/2019 4:07 PM
// last updated 08/15/2019  1:40 PM

using System;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Hediffs;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Pawnmorph.TfSys
{
	/// <summary>
	///     simple implementation of Mutagen that just transforms a single pawn into a single animal
	/// </summary>
	/// <seealso cref="TransformedPawnSingle" />
	public class SimpleMechaniteMutagen : Mutagen<TransformedPawnSingle>
	{
		/// <summary>
		///     Determines whether this instance can transform the specified pawn.
		/// </summary>
		/// <param name="pawn">The pawns.</param>
		public override bool CanTransform(Pawn pawn)
		{
			return CanInfect(pawn);
		}


		/// <summary>
		///     Tries to revert the given pawn.
		/// </summary>
		/// <param name="transformedPawn">The transformed pawn.</param>
		/// <returns></returns>
		public override bool TryRevert(Pawn transformedPawn)
		{
			var pawnStatus = GameComp.GetTransformedPawnContaining(transformedPawn);
			if (pawnStatus != null)
			{
				if (pawnStatus.Value.status != TransformedStatus.Transformed) return false;
				if (pawnStatus.Value.pawn is TransformedPawnSingle inst)
					if (TryRevertImpl(inst))
					{
						GameComp.RemoveInstance(inst);
						return true;
					}
					else
					{
						Log.Warning($"could not revert original pawn instance {inst}");
					}
				else
				{
					Log.Warning($"{nameof(SimpleMechaniteMutagen)} received \"{pawnStatus.Value.pawn?.GetType()?.Name ?? "NULL"}\" but was expecting \"{nameof(TransformedPawnSingle)}\"");
				}
				return false;
			}
			else
			{
				Log.Warning($"unable to find status for transformed pawn {transformedPawn.ThingID}");
			}



			PawnGenerationRequest request = TransformerUtility.GenerateRandomPawnFromAnimal(transformedPawn);



			Pawn pawnTf = PawnGenerator.GeneratePawn(request);


			if (pawnTf.needs.food != null)
				pawnTf.needs.food.CurLevel = transformedPawn.needs.food?.CurLevel ?? pawnTf.needs.food.MaxLevel;

			if (pawnTf.needs.rest != null)
				pawnTf.needs.rest.CurLevel = transformedPawn.needs.rest?.CurLevel ?? pawnTf.needs.rest.MaxLevel;

			Log.Message($"going to spawn {pawnTf.Name} {pawnTf.KindLabel}");
			var spawned = (Pawn)GenSpawn.Spawn(pawnTf, transformedPawn.GetCorrectPosition(), transformedPawn.GetCorrectMap());
			spawned.equipment.DestroyAllEquipment();
			spawned.apparel.DestroyAll();

			if (transformedPawn.Name is NameTriple nT)
			{
				spawned.Name = nT; //give the new random pawn the same name as the former human 
			}


			for (var i = 0; i < 10; i++)
			{
				IntermittentMagicSprayer.ThrowMagicPuffDown(spawned.GetCorrectPosition().ToVector3(), spawned.GetCorrectMap());
				IntermittentMagicSprayer.ThrowMagicPuffUp(spawned.GetCorrectPosition().ToVector3(), spawned.GetCorrectMap());
			}


			AddReversionThought(spawned);



			transformedPawn.Destroy();
			return true;
		}

		/// <summary>
		///     Determines whether this instance can revert pawn the specified transformed pawn.
		/// </summary>
		/// <param name="transformedPawn">The transformed pawn.</param>
		/// <returns>
		///     <c>true</c> if this instance can revert pawn  the specified transformed pawn; otherwise, <c>false</c>.
		/// </returns>
		protected override bool CanRevertPawnImp(TransformedPawnSingle transformedPawn)
		{
			if (!transformedPawn.IsValid) return false;
			var tracker = transformedPawn.animal?.GetSapienceTracker();
			if (tracker == null) return false;
			return transformedPawn.animal.GetSapienceState()?.StateDef == def.transformedSapienceState && !tracker.IsPermanentlyFeral;
		}

		/// <summary>Returns true if the specified request is valid.</summary>
		/// <param name="request">The request.</param>
		/// <returns>
		/// <c>true</c> if the specified request is valid; otherwise, <c>false</c>.
		/// </returns>
		protected override bool IsValid(TransformationRequest request)
		{
			return base.IsValid(request) && request.originals.Length == 1;
		}

		/// <summary>
		///     preform the requested transform
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		protected override TransformedPawnSingle TransformImpl(TransformationRequest request)
		{
			Pawn original = request.originals[0];

			// Drop the pawn if being carried to allow correctly despawning.
			if (original.CarriedBy != null)
				original.CarriedBy.carryTracker.TryDropCarriedThing(original.CarriedBy.Position, ThingPlaceMode.Direct, out _);

			if (request.addMutationToOriginal)
			{
				TryAddMutationsToPawn(original, request.cause, request.outputDef);
				original.GetMutationTracker().RecalculateMutationInfluences();
				original.CheckRace(false, false, false);
			}

			var reactionStatus = original.GetFormerHumanReactionStatus();
			Faction faction;
			faction = GetFaction(request, original);

			Gender newGender =
				TransformerUtility.GetTransformedGender(original, request.forcedGender, request.forcedGenderChance);


			var pRequest = FormerHumanUtilities.CreateSapientAnimalRequest(request.outputDef, original, faction, fixedGender: newGender);



			Pawn animalToSpawn = PawnGenerator.GeneratePawn(pRequest); //make the temp pawn 


			// Copies the original pawn's food need to the animal's.
			if (animalToSpawn.needs.food != null)
				animalToSpawn.needs.food.CurLevel = original.needs.food?.CurLevel ?? animalToSpawn.needs.food.MaxLevel;

			// Copies the original pawn's rest need to the animal's.
			if (animalToSpawn.needs.rest != null)
				animalToSpawn.needs.rest.CurLevel = original.needs.rest?.CurLevel ?? animalToSpawn.needs.rest.MaxLevel;

			animalToSpawn.Name = original.Name; // Copies the original pawn's name to the animal's.

			float sapienceLevel;
			if (original.health?.hediffSet?.HasHediff(TfHediffDefOf.SapienceLimiterHediff) == true)
				sapienceLevel = original.GetSapienceLevel() ?? 1;
			else
				sapienceLevel = request.forcedSapienceLevel ?? GetSapienceLevel(original, animalToSpawn);

			GiveTransformedPawnSapienceState(animalToSpawn, sapienceLevel);

			FormerHumanUtilities.InitializeTransformedPawn(original, animalToSpawn, sapienceLevel);

			Pawn spawnedAnimal = SpawnAnimal(original, animalToSpawn); // Spawns the animal into the map.



			ReactionsHelper.OnPawnTransforms(original, animalToSpawn, reactionStatus); //this needs to happen before MakeSapientAnimal because that removes relations 
			Map correctMap = original.GetCorrectMap();
			if (correctMap != null)
				TransformerUtility.HandleTFWitnesses(original, spawnedAnimal, original.GetCorrectPosition(), correctMap);

			var rFaction = request.factionResponsible ?? GetFactionResponsible(original);



			var inst = new TransformedPawnSingle(request.transformedTick)
			{
				original = original,
				animal = spawnedAnimal,
				factionResponsible = rFaction,
				reactionStatus = reactionStatus
			};


			if (original.Spawned)
				for (var i = 0; i < 10; i++) // Create a cloud of magic.
				{
					IntermittentMagicSprayer.ThrowMagicPuffDown(spawnedAnimal.Position.ToVector3(), spawnedAnimal.MapHeld);
					IntermittentMagicSprayer.ThrowMagicPuffUp(spawnedAnimal.Position.ToVector3(), spawnedAnimal.MapHeld);
				}

			if (request.tale != null) // If a tale was provided, push it to the tale recorder.
				TaleRecorder.RecordTale(request.tale, original, animalToSpawn);

			Faction oFaction = original.Faction; //can't figure out what happened to FactionOrExtraMiniOrHomeFaction, needs further investigation 
			Map oMap = original.Map;


			//apply any other post tf effects 
			ApplyPostTfEffects(original, spawnedAnimal, request);


			TransformerUtility.CleanUpHumanPawnPostTf(original, request.cause); //now clean up the original pawn (remove apparel, drop'em, ect) 


			//notify the faction that their member has been transformed 
			oFaction?.Notify_MemberTransformed(original, animalToSpawn, oMap == null, oMap);

			if (!request.noLetter && reactionStatus == FormerHumanReactionStatus.Colonist || reactionStatus == FormerHumanReactionStatus.Prisoner) //only send the letter for colonists and prisoners 
				SendLetter(request, original, spawnedAnimal);

			if (original.Spawned)
				original.DeSpawn(); // Remove the original pawn from the current map.

			DebugLogUtils.Assert(!PrisonBreakUtility.CanParticipateInPrisonBreak(original),
								 $"{original.Name} has been cleaned up and de-spawned but can still participate in prison breaks");


			return inst;
		}

		private static Faction GetFaction(TransformationRequest request, Pawn original)
		{
			Faction faction;
			if (request.forcedFaction != null) //forced faction should be the highest priority if set 
				faction = request.forcedFaction;
			else if (original.IsColonist)
				faction = original.Faction;
			else if (Rand.Chance(PawnmorpherMod.Settings.hostileKeepFactionTfChance))
				faction = original.Faction;
			else
				faction = null;
			return faction;
		}


		private void TryAddMutationsToPawn([NotNull] Pawn original, [CanBeNull] Hediff requestCause,
										   [NotNull] PawnKindDef requestOutputDef)
		{
			MorphDef mDef = null;

			if (requestCause != null) //first check the cause 
				foreach (MorphDef morphDef in MorphDef.AllDefs)
					if (morphDef.fullTransformation == requestCause.def || morphDef.partialTransformation == requestCause.def)
					{
						mDef = morphDef;
						goto applyMutations; //ugly, but it's the easiest solution
					}

			mDef = MorphUtilities.TryGetBestMorphOfAnimal(requestOutputDef.race);

			if (mDef == null)
			{
				DebugLogUtils.LogMsg(LogLevel.Messages, $"could not apply mutations to {original} with cause {requestCause?.def?.defName ?? "NULL"} and target {requestOutputDef.defName}");
				return;
			}


		applyMutations:
			MutationUtilities.AddAllMorphMutations(original, mDef).SetAllToNaturalMax();
		}

		/// <summary>
		/// Applies the post tf effects.
		/// this should be called just before the original pawn is cleaned up
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="transformedPawn">The transformed pawn.</param>
		/// <param name="request">The transformation request</param>
		protected override void ApplyPostTfEffects(Pawn original, Pawn transformedPawn, TransformationRequest request)
		{
			//apply apparel damage 
			ApplyApparelDamage(original, transformedPawn.def);
			FormerHumanUtilities.TryAssignBackstoryToTransformedPawn(transformedPawn, original);
			base.ApplyPostTfEffects(original, transformedPawn, request);


		}

		private static void SendLetter(TransformationRequest request, Pawn original, Pawn spawnedAnimal)
		{
			Find.LetterStack
				.ReceiveLetter("LetterHediffFromTransformationLabel".Translate(original.LabelShort, request.outputDef.LabelCap).CapitalizeFirst(),
							   "LetterHediffFromTransformation"
								  .Translate(original.LabelShort, request.outputDef.LabelCap)
								  .CapitalizeFirst(),
							   LetterDefOf.NeutralEvent,
							   spawnedAnimal); // Creates a letter saying "Oh no! Pawn X has transformed into a Y!"
			Find.TickManager.slower.SignalForceNormalSpeedShort(); // Slow down the speed of the game.
		}


		/// <summary>
		///     Tries to revert the transformed pawn instance, implementation.
		/// </summary>
		/// <param name="transformedPawn">The transformed pawn.</param>
		/// <returns></returns>
		protected override bool TryRevertImpl(TransformedPawnSingle transformedPawn)
		{
			if (transformedPawn == null) throw new ArgumentNullException(nameof(transformedPawn));
			if (!transformedPawn.IsValid)
			{
				Log.Warning(nameof(SimpleMechaniteMutagen) + " received an invalid transformed pawn to revert");
				return false;
			}


			Pawn animal = transformedPawn.animal;
			if (animal == null) return false;
			var rFaction = transformedPawn.FactionResponsible;

			float currentConvertedAge = TransformerUtility.ConvertAge(transformedPawn.animal, transformedPawn.original.RaceProps);
			float originalAge = transformedPawn.original.ageTracker.AgeBiologicalYearsFloat;

			currentConvertedAge = Math.Max(currentConvertedAge, FormerHumanUtilities.MIN_FORMER_HUMAN_AGE);
			long agedTicksDelta = (long)(currentConvertedAge - originalAge) * 3600000L; // 3600000f ticks per year.
			transformedPawn.original.ageTracker.AgeBiologicalTicks += agedTicksDelta;
			var spawned = (Pawn)GenSpawn.Spawn(transformedPawn.original, animal.PositionHeld, animal.MapHeld);

			if (spawned.Faction != animal.Faction && rFaction == null) //if the responsible faction is null (no one knows who did it) have the reverted pawn join that faction   
			{
				spawned.SetFaction(animal.Faction);
			}


			for (var i = 0; i < 10; i++)
			{
				IntermittentMagicSprayer.ThrowMagicPuffDown(spawned.Position.ToVector3(), spawned.MapHeld);
				IntermittentMagicSprayer.ThrowMagicPuffUp(spawned.Position.ToVector3(), spawned.MapHeld);
			}

			//transfer hediffs from the former human back onto the original pawn
			FormerHumanUtilities.TransferHediffs(animal, spawned);

			SetHumanoidSapience(spawned, animal);

			FixBondRelationship(spawned, animal);
			FormerHumanUtilities.TransferEverything(animal, spawned);

			spawned.Faction?.Notify_MemberReverted(spawned, animal, spawned.Map == null, spawned.Map);

			ReactionsHelper.OnPawnReverted(spawned, animal, transformedPawn.reactionStatus);
			DoPostReversionEffects(spawned, animal);
			if (animal.IsWorldPawn())
			{
				Find.World.worldPawns.RemovePawn(animal);

			}
			animal.ideo = null;
			animal.Destroy();
			return true;
		}

		/// <summary>
		/// preforms effects on either the original or transformed pawn after all core reversion effects are completed but before transformed pawn is cleaned up and destroyed 
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="animal">The animal.</param>
		protected virtual void DoPostReversionEffects(Pawn original, Pawn animal)
		{
			original.health.AddHediff(MorphTransformationDefOf.StabiliserHigh); //add stabilizer on reversion 


			TransformerUtility.CleanUpHumanPawnPostTf(animal, null);

			//make sure to send the event before we destroy the animal 
			SendReversionEvent(original, animal, null);
		}

		/// <summary>
		/// Sets the humanoid sapience upon reversion.
		/// </summary>
		/// <param name="humanoid">The humanoid.</param>
		/// <param name="animal">The animal.</param>
		protected virtual void SetHumanoidSapience([NotNull] Pawn humanoid, [NotNull] Pawn animal)
		{
			PawnComponentsUtility.AddAndRemoveDynamicComponents(humanoid);
			try
			{
				humanoid.needs.AddOrRemoveNeedsAsAppropriate();
			}
			catch (Exception e)
			{
				Log.Message($"caught {e.GetType().Name} \n{e}");
			}
			var hSapienceTracker = humanoid.GetSapienceTracker();
			var aSapienceTracker = animal.GetSapienceTracker();
			if (hSapienceTracker == null || aSapienceTracker == null) return;
			GiveRevertedPawnSapienceState(humanoid, aSapienceTracker.Sapience);
		}

		/// <summary>
		/// transfers or removes bond relationships from reverted animal to the original 
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="revertedAnimal">The reverted animal.</param>
		protected void FixBondRelationship(Pawn original, Pawn revertedAnimal)
		{
			var pRelated = revertedAnimal.relations?.DirectRelations;

			foreach (DirectPawnRelation directPawnRelation in pRelated.MakeSafe().ToList()) //need to cache the values so we don't invalidate the iterator 
			{
				if (directPawnRelation.def != PawnRelationDefOf.Bond) continue;

				//remove the bond relationship 
				revertedAnimal.relations.RemoveDirectRelation(directPawnRelation);

				if (directPawnRelation.otherPawn.RaceProps.Animal)
				{   //add it back to the original if the other pawn is an animal 
					original.relations.AddDirectRelation(PawnRelationDefOf.Bond, directPawnRelation.otherPawn);
				}

			}
		}

		/// <summary>
		/// add the correct reversion thought at the correct stage
		/// </summary>
		/// <param name="spawned">The spawned.</param>
		private void AddReversionThought(Pawn spawned)
		{
			TraitSet traits = spawned.story.traits;
			ThoughtDef thoughtDef;
			var hasPrimalWish = spawned.GetAspectTracker()?.Contains(AspectDefOf.PrimalWish) == true;
			if (hasPrimalWish)
				thoughtDef = def.revertedPrimalWish ?? def.revertedThoughtBad; //substitute with the bad thought if null 
			else if (traits.HasTrait(PMTraitDefOf.MutationAffinity))
				thoughtDef = def.revertedThoughtGood;
			else if (traits.HasTrait(TraitDefOf.BodyPurist))
				thoughtDef = def.revertedThoughtBad;
			else
				thoughtDef = Rand.Value > 0.5f ? def.revertedThoughtGood : def.revertedThoughtBad;

			if (thoughtDef != null)
			{
				//TODO fix this with special memory for animalistic pawns 
				Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, 0);
				spawned.TryGainMemory(mem);
			}
		}

		private static Pawn SpawnAnimal(Pawn original, Pawn animalToSpawn)
		{

			if (original.IsCaravanMember())
			{
				original.GetCaravan().AddPawn(animalToSpawn, true);
				Find.WorldPawns.PassToWorld(animalToSpawn);
				return animalToSpawn;
			}

			if (original.IsWorldPawn())
			{
				Find.WorldPawns.PassToWorld(animalToSpawn);
				return animalToSpawn;
			}

			Map correctMap = original.GetCorrectMap();
			IntVec3 loc = original.GetCorrectPosition();
			var p = (Pawn)GenSpawn.Spawn(animalToSpawn, loc, correctMap);
			if (p == null)
			{
				Log.Error($"unable to spawn pawn {animalToSpawn.Name}!");
				return animalToSpawn;
			}
			return p;
		}
	}
}