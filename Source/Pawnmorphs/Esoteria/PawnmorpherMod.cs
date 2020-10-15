using Pawnmorph.DebugUtils;
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
            listingStandard.CheckboxLabeled("ChamberDatabaseIgnoresDataLimit".Translate(),
                                            ref settings.chamberDatabaseIgnoreStorageLimit,
                                            "ChamberDatabaseIgnoresDataLimitTooltip".Translate());
            listingStandard.CheckboxLabeled("PMInjectorsRequireTagging".Translate(), ref settings.injectorsRequireTagging,
                                            "PMInjectorsRequireTaggingTooltip".Translate()); 

            listingStandard.GapLine();
            listingStandard.Label($"{"transformChanceSliderLabel".Translate()}: {settings.transformChance.ToString("F1")}%");
            settings.transformChance = listingStandard.Slider(settings.transformChance, 0f, 100f);
            listingStandard.Label($"{"formerChanceSliderLabel".Translate()}: {settings.formerChance.ToString("F1")}%");
            settings.formerChance = listingStandard.Slider(settings.formerChance, 0f, 100f);
            listingStandard.Label($"{"partialChanceSliderLabel".Translate()}: {settings.partialChance.ToString("F1")}%");
            settings.partialChance = listingStandard.Slider(settings.partialChance, 0f, 100f);
            listingStandard.Label($"{"maxMutationThoughtsSliderLabel".Translate()}: {settings.maxMutationThoughts}");
            settings.maxMutationThoughts = (int)listingStandard.Slider(settings.maxMutationThoughts, 1, 10);

            listingStandard
               .Label($"{nameof(PawnmorpherSettings.manhunterTfChance).Translate()}: {settings.manhunterTfChance.ToStringByStyle(ToStringStyle.PercentOne)}"); 
            settings.manhunterTfChance = listingStandard.Slider(settings.manhunterTfChance, 0 ,1f);

            if (settings.manhunterTfChance > FormerHumanUtilities.MANHUNTER_EPSILON)
            {
                listingStandard
                   .Label($"{nameof(PawnmorpherSettings.friendlyManhunterTfChance).Translate()}: {settings.manhunterTfChance.ToStringByStyle(ToStringStyle.PercentOne)}");
                settings.friendlyManhunterTfChance = listingStandard.Slider(settings.friendlyManhunterTfChance, 0, 1f);

            }


            if (Prefs.DevMode)
            {
                listingStandard.Label($"logging level:{settings.logLevel}");
                float f = (float) ((int) settings.logLevel);
                var maxLevel = (int) LogLevel.Pedantic;
                f = listingStandard.Slider(maxLevel - f, 0, maxLevel);
                settings.logLevel = (LogLevel) Mathf.FloorToInt(Mathf.Clamp(f, 0, maxLevel));
            }

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
