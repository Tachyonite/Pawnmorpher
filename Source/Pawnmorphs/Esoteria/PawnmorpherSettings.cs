using Verse;

namespace Pawnmorph
{
    public class PawnmorpherSettings : ModSettings
    {
        private const bool DEFAULT_FALLOUT_SETTING = false; 

        public bool enableMutagenShipPart = true;
        public bool enableMutagenDiseases = true;
        public bool enableMutagenMeteor = true;
        public bool enableWildFormers = true;
        public bool enableFallout = DEFAULT_FALLOUT_SETTING; 
        public float transformChance = 50f;
        public float formerChance = 2f;
        public float partialChance = 5f;
        public int maxMutationThoughts=3; 

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
            base.ExposeData();
        }
    }
}
