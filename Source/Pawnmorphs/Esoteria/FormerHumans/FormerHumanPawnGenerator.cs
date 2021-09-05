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
        public static Pawn GenerateRandomHumanForm(Pawn animal, PawnGenerationSettings settings = default)
        {
            if (settings.BioAge == null)
                settings.BioAge = TransformerUtility.ConvertAge(animal, ThingDefOf.Human.race);
            return GenerateRandomHumanForm(settings);
        }

        /// <summary>
        ///     Generates the random unmerged humans for the given merged animal
        /// </summary>
        /// <param name="mergedAnimal">The animal.</param>
        /// <param name="p1Settings">Optional traits for the first pawn</param>
        /// <param name="p2Settings">Optional traits for the first pawn</param>
        /// <returns></returns>
        public static (Pawn p1, Pawn p2) GenerateRandomUnmergedHumans(Pawn mergedAnimal,
            PawnGenerationSettings p1Settings = default, PawnGenerationSettings p2Settings = default)
        {
            float convertedAge = TransformerUtility.ConvertAge(mergedAnimal, ThingDefOf.Human.race);

            // If bio ages aren't already set, ensure they average out to the animal's age
            p1Settings.BioAge = p1Settings.BioAge ?? Rand.Range(0.7f, 1.3f) * convertedAge;
            p2Settings.BioAge = p2Settings.BioAge ?? 2 * convertedAge - p1Settings.BioAge;

            Pawn p1 = GenerateRandomHumanForm(p1Settings);
            Pawn p2 = GenerateRandomHumanForm(p2Settings);

            return (p1, p2);
        }

        /// <summary>
        ///     Generates a random pawn to be used as a former human's human form
        /// </summary>
        /// <param name="settings">The settings of the generated pawn</param>
        /// <returns></returns>
        public static Pawn GenerateRandomHumanForm(PawnGenerationSettings settings = default)
        {
            PawnKindDef kind = settings.PawnKind ?? PawnKindDefOf.SpaceRefugee;
            Faction faction = settings.Faction ?? DownedRefugeeQuestUtility.GetRandomFactionForRefugee();

            var request = new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, -1,
                    fixedBiologicalAge: settings.BioAge, fixedChronologicalAge: settings.ChronoAge,
                    fixedBirthName: settings.FirstName, fixedLastName: settings.LastName,
                    fixedGender: settings.Gender);
            return PawnGenerator.GeneratePawn(request);
        }
    }

    /// <summary>
    /// Struct to hold all the requested settings of a former human.
    /// Any null setting will be randomized by the generator
    /// </summary>
    public struct PawnGenerationSettings
    {
        private float? bioAge;

        /// <summary>
        /// The biological age of the pawn, if set
        /// </summary>
        /// <value>The bio age.</value>
        public float? BioAge
        {
            get {

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
