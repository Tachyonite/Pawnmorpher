using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.FormerHumans
{
	/// <summary>
	/// Static class to generate random human forms for former humans
	/// </summary>
	public static class FormerHumanPawnGenerator
	{
		/// <summary>
		///     Generates a random pawn to be used as the given animal's human form
		/// </summary>
		/// <param name="animal">The animal.</param>
		/// <param name="settings">Optional settings for the pawn</param>
		/// <returns></returns>
		public static Pawn GenerateRandomHumanForm(Pawn animal, FHGenerationSettings settings = default)
		{
			if (settings.BioAge == null)
				settings.BioAge = TransformerUtility.ConvertAge(animal, ThingDefOf.Human.race);

			Pawn pawn = GenerateRandomPawn(settings);
			AddMorphMutationsToPawn(pawn, animal);

			return pawn;
		}

		/// <summary>
		///     Generates the random unmerged humans for the given merged animal
		/// </summary>
		/// <param name="mergedAnimal">The animal.</param>
		/// <param name="p1Settings">Optional settings for the first pawn</param>
		/// <param name="p2Settings">Optional settings for the first pawn</param>
		/// <returns></returns>
		public static (Pawn p1, Pawn p2) GenerateRandomUnmergedHumans(Pawn mergedAnimal,
			FHGenerationSettings p1Settings = default, FHGenerationSettings p2Settings = default)
		{
			float convertedAge = TransformerUtility.ConvertAge(mergedAnimal, ThingDefOf.Human.race);

			// If bio ages aren't already set, ensure they average out to the animal's age
			p1Settings.BioAge = p1Settings.BioAge ?? Rand.Range(0.7f, 1.3f) * convertedAge;
			p2Settings.BioAge = p2Settings.BioAge ?? 2 * convertedAge - p1Settings.BioAge;

			Pawn p1 = GenerateRandomPawn(p1Settings);
			Pawn p2 = GenerateRandomPawn(p2Settings);
			AddMorphMutationsToPawn(p1, mergedAnimal);
			AddMorphMutationsToPawn(p2, mergedAnimal);

			return (p1, p2);
		}

		/// <summary>
		///     Generates a random pawn to be used as a former human's human form
		/// </summary>
		/// <param name="settings">The settings of the generated pawn</param>
		/// <returns></returns>
		public static Pawn GenerateRandomPawn(in FHGenerationSettings settings = default)
		{
			PawnKindDef kind = settings.PawnKind ?? PawnKindDefOf.SpaceRefugee;
			Faction faction = settings.Faction ?? DownedRefugeeQuestUtility.GetRandomFactionForRefugee();

			var request = new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, -1,
					fixedBiologicalAge: settings.BioAge, fixedChronologicalAge: settings.ChronoAge,
					fixedBirthName: settings.FirstName, fixedLastName: settings.LastName,
					fixedGender: settings.Gender, colonistRelationChanceFactor: settings.ColonistRelationChanceFactor ?? 1);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			TransformerUtility.HandleApparelAndEquipment(pawn, null);

			return pawn;
		}

		/// <summary>
		/// Adds all the morph mutations to the pawn.
		/// </summary>
		/// <param name="humanForm">Human form.</param>
		/// <param name="animal">Animal to add mutations for.</param>
		private static void AddMorphMutationsToPawn(Pawn humanForm, Pawn animal)
		{
			MorphDef morph = animal.def.TryGetBestMorphOfAnimal();
			if (morph != null)
			{
				if (morph.IsChimera())
					AddRandomMutationsToPawn(humanForm); // TODO these need to be set to natural max
				else
					MutationUtilities.AddAllMorphMutations(humanForm, morph, MutationUtilities.AncillaryMutationEffects.None)
									 .SetAllToNaturalMax();
			}

			MutationTracker mTracker = humanForm.GetMutationTracker();
			if (mTracker != null)
			{
				mTracker.RecalculateMutationInfluences();
				humanForm.CheckRace(false, false, false);
			}
		}

		/// <summary>
		/// Adds random mutations to this pawn to for being a chaomorph
		/// </summary>
		/// <param name="lPawn">L pawn.</param>
		private static void AddRandomMutationsToPawn(Pawn lPawn)
		{
			//give at least as many mutations as there are slots, plus some more to make it a bit more chaotic 
			int mutationsToAdd = Mathf.CeilToInt(MorphUtilities.GetMaxInfluenceOfRace(lPawn.def)) + 10;
			var mutations = MutationUtilities.AllNonRestrictedMutations.ToList();

			var addList = new List<BodyPartRecord>();
			var addedList = new List<BodyPartRecord>();

			var bodyParts = lPawn.health.hediffSet.GetAllNonMissingWithoutProsthetics().ToList();


			var i = 0;
			MutationUtilities.AncillaryMutationEffects aEffects = MutationUtilities.AncillaryMutationEffects.None;
			while (i < mutationsToAdd)
			{
				addList.Clear();
				MutationDef rM = mutations.RandomElementWithFallback();
				if (rM == null) break;
				mutations.Remove(rM);

				//handle whole body mutations first 
				if (rM.parts == null)
				{
					if (MutationUtilities.AddMutation(lPawn, rM, ancillaryEffects: aEffects)) i++;
					continue;
				}

				//get the body parts to add to 

				//how many parts to grab 
				//+3 is to make it more likely that all parts will be added 
				int countToAdd = Rand.Range(1, rM.parts.Count + 3);
				countToAdd = Mathf.Min(countToAdd, rM.parts.Count); //make sure it's less then or equal to 
				foreach (BodyPartRecord record in bodyParts)
				{
					if (!rM.parts.Any(p => p == record.def)) continue;

					if (addedList.Contains(record)) continue;

					addedList.Add(record);
					addList.Add(record);

					if (addList.Count >= countToAdd) break;
				}


				MutationResult res = MutationUtilities.AddMutation(lPawn, rM, addList, aEffects);
				if (res) i++; //only increment if we actually added any mutations 
			}
		}

	}

	/// <summary>
	/// Struct to hold all the requested settings of a former human.
	/// Any null setting will be randomized by the generator
	/// </summary>
	public struct FHGenerationSettings
	{
		private float? bioAge;

		/// <summary>
		/// The biological age of the pawn, if set
		/// </summary>
		/// <value>The bio age.</value>
		public float? BioAge
		{
			get
			{

				if (bioAge == null)
					return null;
				// Make sure the returned bio age is never less than the minimum
				return Mathf.Max((float)bioAge, FormerHumanUtilities.MIN_FORMER_HUMAN_AGE);
			}
			set => bioAge = value;
		}

		private float? chronoAge;

		/// <summary>
		/// The chronological age of the pawn, if set
		/// </summary>
		/// <value>The chrono age.</value>
		public float? ChronoAge
		{
			get
			{
				if (chronoAge == null)
					return null;

				// Make sure the returned chrono age is never less than the minimum or the bio age
				float age = Mathf.Max((float)chronoAge, FormerHumanUtilities.MIN_FORMER_HUMAN_AGE);
				if (BioAge != null)
					age = Mathf.Max(age, (float)BioAge);

				return age;
			}
			set => chronoAge = value;
		}

		/// <summary>
		/// The fixed first name of the pawn, if set
		/// </summary>
		/// <value>The first name.</value>
		public string FirstName { get; set; }

		/// <summary>
		/// The fixed first name of the pawn, if set
		/// </summary>
		/// <value>The first name.</value>
		public string LastName { get; set; }

		///<summary>
		/// multiplier on the chance the former human is related to a colonist 
		/// </summary>
		public float? ColonistRelationChanceFactor { get; set; }

		/// <summary>
		/// The fixed gender of the pawn, if set
		/// </summary>
		/// <value>The first name.</value>
		public Gender? Gender { get; set; }

		/// <summary>
		/// The fixed pawnkind of the pawn, if set
		/// </summary>
		/// <value>The first name.</value>
		public PawnKindDef PawnKind { get; set; }

		/// <summary>
		/// The fixed faction of the pawn, if set
		/// </summary>
		/// <value>The first name.</value>
		public Faction Faction { get; set; }
	}
}
