using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// the mod class
    /// </summary>
    /// <seealso cref="Verse.Mod" />
    public class PawnmorpherMod : Mod
    {
        PawnmorpherSettings settings;
        /// <summary>
        /// Initializes a new instance of the <see cref="PawnmorpherMod"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        public PawnmorpherMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<PawnmorpherSettings>();
        }

        /// <summary>Writes the settings.</summary>
        public override void WriteSettings()
        {
            base.WriteSettings();
            PawnmorpherModInit.NotifySettingsChanged();
        }

        /// <param name="inRect"> A Unity Rect with the size of the settings window. </param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("enableFalloutCheckboxLabel".Translate(), ref settings.enableFallout, "enableFalloutCheckboxTooltip".Translate());
            listingStandard.CheckboxLabeled("enableMutagenCheckboxLabel".Translate(), ref settings.enableMutagenShipPart, "enableMutagenCheckboxTooltip".Translate());
            listingStandard.CheckboxLabeled("enableMutagenDiseasesCheckboxLabel".Translate(), ref settings.enableMutagenDiseases, "enableMutagenDiseasesCheckboxTooltip".Translate());
            listingStandard.CheckboxLabeled("enableMutagenMeteorCheckboxLabel".Translate(), ref settings.enableMutagenMeteor, "enableMutagenMeteorCheckboxTooltip".Translate());
            listingStandard.CheckboxLabeled("enableWildFormersCheckboxLabel".Translate(), ref settings.enableWildFormers, "enableWildFormersCheckboxTooltip".Translate());
            listingStandard.GapLine();
            listingStandard.Label($"{"transformChanceSliderLabel".Translate()}: {settings.transformChance.ToString("F1")}%");
            settings.transformChance = listingStandard.Slider(settings.transformChance, 0f, 100f);
            listingStandard.Label($"{"formerChanceSliderLabel".Translate()}: {settings.formerChance.ToString("F1")}%");
            settings.formerChance = listingStandard.Slider(settings.formerChance, 0f, 100f);
            listingStandard.Label($"{"partialChanceSliderLabel".Translate()}: {settings.partialChance.ToString("F1")}%");
            settings.partialChance = listingStandard.Slider(settings.partialChance, 0f, 100f);
            listingStandard.Label($"{"maxMutationThoughtsSliderLabel".Translate()}: {settings.maxMutationThoughts}");
            settings.maxMutationThoughts = (int)listingStandard.Slider(settings.maxMutationThoughts, 1, 10);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings. <br />
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns> The (translated) mod name. </returns>
        public override string SettingsCategory()
        {
            return "PawnmorpherModName".Translate();
        }
    }
}
