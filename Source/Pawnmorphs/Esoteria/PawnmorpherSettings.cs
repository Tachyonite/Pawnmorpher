using System.Collections.Generic;
using Pawnmorph.DebugUtils;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// the mod settings 
    /// </summary>
    /// <seealso cref="Verse.ModSettings" />
    public class PawnmorpherSettings : ModSettings
    {
        private const bool DEFAULT_FALLOUT_SETTING = true;

        /// <summary>
        /// if the mutagen ship part should be enabled 
        /// </summary>
        public bool enableMutagenShipPart = true;
        /// <summary>
        /// if mutagenic diseases are enabled 
        /// </summary>
        public bool enableMutagenDiseases = true;
        /// <summary>
        /// if mutanite meteors are enabled 
        /// </summary>
        public bool enableMutagenMeteor = true;
        /// <summary>if wild former humans are enabled</summary>
        public bool enableWildFormers = true;
        /// <summary>if mutagenic fallout is enabled</summary>
        public bool enableFallout = DEFAULT_FALLOUT_SETTING;
        /// <summary>if slurry pipe leak is enabled</summary>
        public bool enableMutagenLeak = true;
        /// <summary>the chance for a transforming pawn to turn into an animal</summary>
        public float transformChance = 50f;
        /// <summary>the chance for new animals to be former humans</summary>
        public float formerChance = 0.02f;
        /// <summary>The partial chance</summary>
        public float partialChance = 5f;

        /// <summary>
        /// if true failed chaobulb harvests can give mutagenic buildup
        /// </summary>
        public bool hazardousChaobulbs = true; 

        /// <summary>
        /// if The injectors require tagging the associated animal first
        /// </summary>
        public bool injectorsRequireTagging = true; 

        /// <summary>
        /// The maximum mutation thoughts that can be active at once 
        /// </summary>
        public int maxMutationThoughts=3;

        /// <summary>
        /// if true, the chamber database will ignore storage restrictions, used for debugging 
        /// </summary>
        public bool chamberDatabaseIgnoreStorageLimit; 


        /// <summary>
        /// the chance an tf'd enemy or neutral pawn will go manhunter 
        /// </summary>
        public float manhunterTfChance = 0;
        /// <summary>
        /// The chance a friendly pawn will go manhunter when tf'd 
        /// </summary>
        public float friendlyManhunterTfChance = 0; 

        /// <summary>
        /// The current log level
        /// </summary>
        public LogLevel logLevel = LogLevel.Warnings;

        /// <summary>
        /// List of races whitelisted to have visible mutations.
        /// </summary>
        public List<string> visibleRaces;

        /// <summary>
        /// Dictionary of morphdef and selected replacement racedef
        /// </summary>
        public Dictionary<string, string> raceReplacements;

        /// <summary>
        /// Dictionary of morphdef and selected replacement racedef
        /// </summary>
        public Dictionary<string, string> animalAssociations;

		/// <summary>
		/// List of blacklisted animal types.
		/// </summary>
		public List<string> animalBlacklist;

		/// <summary>
		/// Dictionary of optional patches explicitly enabled or disabled.
		/// </summary>
		public Dictionary<string, bool> optionalPatches;


        /// <summary>
        /// The saved genebank window size
        /// </summary>
        public Vector2? GenebankWindowSize;

        /// <summary>
        /// The saved genebank window location
        /// </summary>
        public Vector2? GenebankWindowLocation;


		/// <summary>
		/// The saved genebank font size
		/// </summary>
		public Verse.GameFont? GenebankWindowFont;

		/// <summary> The part that writes our settings to file. Note that saving is by ref. </summary>
		public override void ExposeData()
        {
            Scribe_Values.Look(ref enableFallout, nameof(enableFallout), DEFAULT_FALLOUT_SETTING);
            Scribe_Values.Look(ref enableMutagenLeak, "enableMutagenLeak", true);
            Scribe_Values.Look(ref enableMutagenShipPart, "enableMutagenShipPart", true);
            Scribe_Values.Look(ref enableMutagenDiseases, "enableMutagenDiseases", true);
            Scribe_Values.Look(ref enableMutagenMeteor, "enableMutagenMeteor", true);
            Scribe_Values.Look(ref enableWildFormers, "enableWildFormers", true);
            Scribe_Values.Look(ref transformChance, "transformChance");
            Scribe_Values.Look(ref formerChance, "formerChance");
            Scribe_Values.Look(ref partialChance, "partialChance");
            Scribe_Values.Look(ref maxMutationThoughts, nameof(maxMutationThoughts), 1);
            Scribe_Values.Look(ref injectorsRequireTagging, nameof(injectorsRequireTagging)); 
            Scribe_Values.Look(ref logLevel, nameof(logLevel), LogLevel.Warnings, true); 
            Scribe_Values.Look(ref manhunterTfChance, nameof(manhunterTfChance));
            Scribe_Values.Look(ref friendlyManhunterTfChance, nameof(friendlyManhunterTfChance));
            Scribe_Values.Look(ref chamberDatabaseIgnoreStorageLimit, nameof(chamberDatabaseIgnoreStorageLimit));
            Scribe_Values.Look(ref hazardousChaobulbs, nameof(hazardousChaobulbs), true);


			Scribe_Values.Look(ref GenebankWindowSize, nameof(GenebankWindowSize));
			Scribe_Values.Look(ref GenebankWindowLocation, nameof(GenebankWindowLocation));
			Scribe_Values.Look(ref GenebankWindowFont, nameof(GenebankWindowFont));

			Scribe_Collections.Look(ref visibleRaces, nameof(visibleRaces));
            Scribe_Collections.Look(ref raceReplacements, nameof(raceReplacements));
            Scribe_Collections.Look(ref optionalPatches, nameof(optionalPatches));
            Scribe_Collections.Look(ref animalAssociations, nameof(animalAssociations));
			Scribe_Collections.Look(ref animalBlacklist, nameof(animalBlacklist));

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (formerChance > 1) formerChance /= 100f; 
            }

            base.ExposeData();
        }
    }
}
