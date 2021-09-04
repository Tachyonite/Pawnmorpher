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
        /// <param name="fixedChronoAge">Optional fixed chronological age.</param>
        /// <param name="fixedFirstName">Optional fixed first name.</param>
        /// <param name="fixedLastName">Optional fixed last name.</param>
        /// <param name="fixedOriginalGender">Optional fixed gender.</param>
        /// <returns></returns>
        public static Pawn GenerateRandomHumanForm(Pawn animal,
                float? fixedChronoAge = null, string fixedFirstName = null,
                string fixedLastName = null, Gender? fixedOriginalGender = null)
        {
            float convertedAge = TransformerUtility.ConvertAge(animal, ThingDefOf.Human.race);
            return GenerateRandomHumanForm(convertedAge, fixedChronoAge,
                fixedFirstName, fixedLastName, fixedOriginalGender);
        }

        /// <summary>
        ///     Generates the random unmerged humans for the given merged animal
        /// </summary>
        /// <param name="mergedAnimal">The animal.</param>
        /// <returns></returns>
        public static (Pawn p1, Pawn p2) GenerateRandomUnmergedHumans(Pawn mergedAnimal)
        {

            float convertedAge = TransformerUtility.ConvertAge(mergedAnimal, ThingDefOf.Human.race);

            // Generate random ages that average together to the given bio age
            float p1Age = Rand.Range(0.7f, 1.3f) * convertedAge;
            float p2Age = 2 * convertedAge - p1Age;

            Pawn p1 = GenerateRandomHumanForm(p1Age);
            Pawn p2 = GenerateRandomHumanForm(p2Age);

            return (p1, p2);
        }

        /// <summary>
        ///     Generates a random pawn to be used as a former human's human form
        /// </summary>
        /// <param name="bioAge">The biological age of the pawn.</param>
        /// <param name="fixedChronoAge">Optional fixed chronological age.</param>
        /// <param name="fixedFirstName">Optional fixed first name.</param>
        /// <param name="fixedLastName">Optional fixed last name.</param>
        /// <param name="fixedOriginalGender">Optional fixed gender.</param>
        /// <returns></returns>
        public static Pawn GenerateRandomHumanForm(float bioAge,
                float? fixedChronoAge = null, string fixedFirstName = null,
                string fixedLastName = null, Gender? fixedOriginalGender = null)
        {
            // Ensure the pawn is not too young
            bioAge = Mathf.Max(bioAge, FormerHumanUtilities.MIN_FORMER_HUMAN_AGE);

            PawnKindDef pawnKind = PawnKindDefOf.SpaceRefugee;

            PawnKindDef kind = pawnKind;
            Faction faction = DownedRefugeeQuestUtility.GetRandomFactionForRefugee();

            var request = new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, -1,
                                                    fixedBiologicalAge: bioAge, fixedChronologicalAge: fixedChronoAge,
                                                    fixedBirthName: fixedFirstName, fixedLastName: fixedLastName,
                                                    fixedGender: fixedOriginalGender);
            return PawnGenerator.GeneratePawn(request);
        }
    }
}
