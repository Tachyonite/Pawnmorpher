using System;
using System.Linq;
using Pawnmorph.Thoughts;
using RimWorld;
using Verse;

namespace Pawnmorph.TfSys
{
	/// <summary>
	///     implementation of mutagen that merges 2 or more pawns into a single meld
	/// </summary>
	/// <seealso cref="Pawnmorph.TfSys.Mutagen{T}" />
	public class MergeMutagen : Mutagen<MergedPawns>
	{
		private const string FORMER_HUMAN_HEDIFF = "2xMergedHuman"; //can't put this in a hediffDefOf because of the name 





		/// <summary>
		///     Determines whether this instance can revert pawn the specified transformed pawn.
		/// </summary>
		/// <param name="transformedPawn">The transformed pawn.</param>
		/// <returns>
		///     <c>true</c> if this instance can revert pawn  the specified transformed pawn; otherwise, <c>false</c>.
		/// </returns>
		protected override bool CanRevertPawnImp(MergedPawns transformedPawn)
		{
			if (!transformedPawn.IsValid) return false;

			HediffDef def = HediffDef.Named(FORMER_HUMAN_HEDIFF);

			return transformedPawn.meld.health.hediffSet.HasHediff(def);
		}
		/// <summary>Returns true if the given request is valid.</summary>
		/// <param name="request">The request.</param>
		/// <returns>
		/// <c>true</c> if the specified request is valid; otherwise, <c>false</c>.
		/// </returns>
		protected override bool IsValid(TransformationRequest request)
		{
			return base.IsValid(request) && request.originals.Length == 2;
		}
		/// <summary>
		/// Determines whether this instance can transform the specified pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if this instance can transform the specified pawn; otherwise, <c>false</c>.
		/// </returns>
		public override bool CanTransform(Pawn pawn)
		{
			return base.CanInfect(pawn);
		}

		/// <summary>
		/// preform the requested transform 
		/// </summary>
		/// <param name="request">The request.</param>
		/// <returns></returns>
		protected override MergedPawns TransformImpl(TransformationRequest request)
		{
			Pawn firstPawn = request.originals[0];
			Pawn secondPawn = request.originals[1];
			float averageAge = firstPawn.ageTracker.AgeBiologicalYearsFloat
							 + secondPawn.ageTracker.AgeBiologicalYearsFloat;
			averageAge /= 2;


			float newAge = averageAge * request.outputDef.race.race.lifeExpectancy / firstPawn.RaceProps.lifeExpectancy;

			Faction faction = request.forcedFaction ?? Faction.OfPlayer;

			var pRequest = FormerHumanUtilities.CreateMergedAnimalRequest(request.outputDef, request.originals, faction);



			Pawn meldToSpawn = PawnGenerator.GeneratePawn(pRequest);

			HediffDef hediffToAdd = HediffDef.Named(FORMER_HUMAN_HEDIFF); //make sure hediff is added before spawning meld 

			//make them count as former humans 
			var tracker = meldToSpawn.GetSapienceTracker();
			if (tracker == null)
			{
				Log.Error($"{meldToSpawn.def.defName} is a meld but does not have a former human tracker!");
			}
			else
			{
				GiveTransformedPawnSapienceState(meldToSpawn, 1);
			}



			Hediff hediff = HediffMaker.MakeHediff(hediffToAdd, meldToSpawn);
			hediff.Severity = Rand.Range(request.minSeverity, request.maxSeverity);
			meldToSpawn.health.AddHediff(hediff);

			Pawn_NeedsTracker needs = meldToSpawn.needs;

			if (needs.food != null)
				needs.food.CurLevel = firstPawn.needs.food?.CurLevel ?? needs.food.MaxLevel;

			if (needs.rest != null)
				needs.rest.CurLevel = firstPawn.needs.rest?.CurLevel ?? needs.rest.MaxLevel;

			meldToSpawn.training.SetWantedRecursive(TrainableDefOf.Obedience, true);
			meldToSpawn.training.Train(TrainableDefOf.Obedience, null, true);
			meldToSpawn.Name = firstPawn.Name;
			var meld = (Pawn)GenSpawn.Spawn(meldToSpawn, firstPawn.PositionHeld, firstPawn.MapHeld);
			for (var i = 0; i < 10; i++)
			{
				IntermittentMagicSprayer.ThrowMagicPuffDown(meld.Position.ToVector3(), meld.MapHeld);
				IntermittentMagicSprayer.ThrowMagicPuffUp(meld.Position.ToVector3(), meld.MapHeld);
			}

			meld.SetFaction(Faction.OfPlayer);

			ReactionsHelper.OnPawnsMerged(firstPawn, firstPawn.IsPrisoner, secondPawn, secondPawn.IsPrisoner, meld);
			MergedPawnUtilities.TransferToMergedPawn(request.originals, meld);
			//apply any other post tf effects 
			ApplyPostTfEffects(request.originals[0], meld, request);

			TransformerUtility.CleanUpHumanPawnPostTf(firstPawn, null);
			TransformerUtility.CleanUpHumanPawnPostTf(secondPawn, null);

			var inst = new MergedPawns(request.transformedTick)
			{
				originals = request.originals.ToList(), //we want to make a copy here 
				meld = meld,
				mutagenDef = def,
				factionResponsible = Faction.OfPlayer
			};
			return inst;



		}

		void CheckForBrainDamage(Pawn meld, Pawn human0, Pawn human1)
		{
			var brains = meld.health.hediffSet.GetNotMissingParts()
							 .Where(p => p.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource))
							 .ToList();
			if (brains.Count != 2)
			{
				var pawn = Rand.Value < 0.5f ? human0 : human1;
				//var brain = pawn.health.hediffSet.GetBrain();
				DamageInfo dInfo = new DamageInfo(DamageDefOf.Stab, 100, 1);
				pawn.Kill(dInfo);

			}
		}


		/// <summary>
		///     Tries to revert the transformed pawn instance, implementation.
		/// </summary>
		/// <param name="transformedPawn">The transformed pawn.</param>
		/// <returns></returns>
		protected override bool TryRevertImpl(MergedPawns transformedPawn)
		{
			if (!transformedPawn.IsValid) return false;


			if (transformedPawn.originals.Count != 2) return false;
			HediffDef hDef = HediffDef.Named(FORMER_HUMAN_HEDIFF);
			Pawn meld = transformedPawn.meld;

			Hediff formerHumanHediff = meld?.health?.hediffSet?.hediffs?.FirstOrDefault(h => h.def == hDef);
			if (formerHumanHediff == null) return false;


			PawnRelationDef mergMateDef = DefDatabase<PawnRelationDef>.GetNamed("MergeMate");
			PawnRelationDef mergeMateEx = DefDatabase<PawnRelationDef>.GetNamed("ExMerged"); //TODO put these in a DefOf 



			foreach (Pawn originalPawn in transformedPawn.originals)
			{
				float currentConvertedAge = TransformerUtility.ConvertAge(transformedPawn.meld, originalPawn.RaceProps);
				float originalAge = originalPawn.ageTracker.AgeBiologicalYearsFloat;

				long agedTicksDelta = (long)(currentConvertedAge - originalAge) * 3600000L; // 3600000f ticks per year.
				originalPawn.ageTracker.AgeBiologicalTicks += agedTicksDelta;
			}


			var firstO = (Pawn)GenSpawn.Spawn(transformedPawn.originals[0], meld.PositionHeld, meld.MapHeld);
			var secondO = (Pawn)GenSpawn.Spawn(transformedPawn.originals[1], meld.PositionHeld, meld.MapHeld);

			for (var i = 0; i < 10; i++)
			{
				IntermittentMagicSprayer.ThrowMagicPuffDown(meld.Position.ToVector3(), meld.MapHeld);
				IntermittentMagicSprayer.ThrowMagicPuffUp(meld.Position.ToVector3(), meld.MapHeld);
			}

			var traits = firstO.story.traits;
			bool relationIsMergeMate = false;
			if (traits.HasTrait(PMTraitDefOf.MutationAffinity))
			{
				relationIsMergeMate = true;
			}
			else if (traits.HasTrait(TraitDefOf.BodyPurist))
			{
				relationIsMergeMate = false;
			}
			else
				relationIsMergeMate = Rand.Value < 0.5;


			PawnRelationDef addDef = relationIsMergeMate ? mergMateDef : mergeMateEx; //first element is "WasMerged"

			firstO.relations.AddDirectRelation(addDef, secondO);

			firstO.SetFaction(Faction.OfPlayer);
			secondO.SetFaction(Faction.OfPlayer);
			//TransformerUtility.RemoveAllMutations(firstO);
			//TransformerUtility.RemoveAllMutations(secondO); 

			ReactionsHelper.OnPawnReverted(firstO, meld);
			ReactionsHelper.OnPawnReverted(secondO, meld);
			CheckForBrainDamage(meld, firstO, secondO);
			meld.DeSpawn();

			return true;
		}

		private ThoughtDef AddRandomThought(Pawn p, int stageNum)
		{
			var traits = p.story.traits;
			ThoughtDef thoughtDef;
			if (traits.HasTrait(PMTraitDefOf.MutationAffinity))
			{
				thoughtDef = def.revertedThoughtGood;
			}
			else if (traits.HasTrait(TraitDefOf.BodyPurist))
			{
				thoughtDef = def.revertedThoughtBad;
			}
			else
				thoughtDef = Rand.Value < 0.5 ? def.revertedThoughtGood : def.revertedThoughtBad;

			var memories = p.needs.mood.thoughts.memories;
			var thought = ThoughtMaker.MakeThought(thoughtDef, stageNum);
			memories.TryGainMemory(thought);
			return thoughtDef;
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

		/// <summary>
		///     Tries to revert the given pawn.
		/// </summary>
		/// <param name="transformedPawn">The transformed pawn.</param>
		/// <returns></returns>
		public override bool TryRevert(Pawn transformedPawn)
		{
			if (transformedPawn == null)
			{
				throw new ArgumentNullException(nameof(transformedPawn));
			}

			var pawnStatus = GameComp.GetTransformedPawnContaining(transformedPawn);
			var sState = transformedPawn.GetSapienceState();
			if (sState == null) return false;
			if (pawnStatus != null)
			{
				if (pawnStatus.Value.status != TransformedStatus.Transformed) return false;
				if (pawnStatus.Value.pawn is MergedPawns merged)
				{
					if (sState.StateDef != def.transformedSapienceState) return false;

					if (TryRevertImpl(merged))
					{
						GameComp.RemoveInstance(merged);
						return true;
					}
				}

				return false;
			}


			if (sState.StateDef == def.transformedSapienceState)
			{

				ThoughtDef thoughtDef = null;

				for (var i = 0; i < 2; i++)
				{
					PawnGenerationRequest request = TransformerUtility.GenerateRandomPawnFromAnimal(transformedPawn);
					Pawn pawnTf = PawnGenerator.GeneratePawn(request);

					if (pawnTf.needs.food != null)
						pawnTf.needs.food.CurLevel = transformedPawn.needs.food?.CurInstantLevel ?? pawnTf.needs.food.MaxLevel;

					if (pawnTf.needs.rest != null)
						pawnTf.needs.rest.CurLevel = transformedPawn.needs.rest?.CurLevel ?? pawnTf.needs.rest.MaxLevel;

					var spawnedPawn = (Pawn)GenSpawn.Spawn(pawnTf, transformedPawn.PositionHeld, transformedPawn.MapHeld);
					spawnedPawn.apparel.DestroyAll();
					spawnedPawn.equipment.DestroyAllEquipment();
					for (var j = 0; j < 10; j++)
					{
						IntermittentMagicSprayer.ThrowMagicPuffDown(spawnedPawn.Position.ToVector3(), spawnedPawn.MapHeld);
						IntermittentMagicSprayer.ThrowMagicPuffUp(spawnedPawn.Position.ToVector3(), spawnedPawn.MapHeld);
					}

					_scratchArray[i] = spawnedPawn;
				}
				PawnRelationDef relationDef;
				bool relationIsMergeMate = thoughtDef == def.revertedThoughtGood;
				relationDef = relationIsMergeMate ? TfRelationDefOf.MergeMate : TfRelationDefOf.ExMerged;
				_scratchArray[0].relations.AddDirectRelation(relationDef, _scratchArray[1]);

				CheckForBrainDamage(transformedPawn, _scratchArray[0], _scratchArray[1]);
				transformedPawn.Destroy();



				return true;
			}

			return false;
		}

		private readonly Pawn[] _scratchArray = new Pawn[2];
	}
}