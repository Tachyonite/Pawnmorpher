using Pawnmorph.DebugUtils;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// the mod settings 
    /// </summary>
    /// <seealso cref="Verse.ModSettings" />
    public class PawnmorpherSettings : ModSettings
    {
        private const bool DEFAULT_FALLOUT_SETTING = false;

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
        /// <summary>the chance for a transforming pawn to turn into an animal</summary>
        public float transformChance = 50f;
        /// <summary>the chance for new animals to be former humans</summary>
        public float formerChance = 2f;
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

        /// <summary> The part that writes our settings to file. Note that saving is by ref. </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableFallout, nameof(enableFallout), DEFAULT_FALLOUT_SETTING); 
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
            base.ExposeData();
        }
    }
}
