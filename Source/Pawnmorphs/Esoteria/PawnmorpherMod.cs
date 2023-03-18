using System.Collections.Generic;
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
		/// A convenience property to get the settings statically
		/// </summary>
		/// <value>The settings.</value>
		public static PawnmorpherSettings Settings => LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>();

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
			
			Listing_Standard checkBoxSection = listingStandard.BeginSection(10 * Text.LineHeight);
			checkBoxSection.ColumnWidth = (checkBoxSection.ColumnWidth - 14) / 2;
			checkBoxSection.CheckboxLabeled("enableFalloutCheckboxLabel".Translate(), ref settings.enableFallout, "enableFalloutCheckboxTooltip".Translate());
			checkBoxSection.CheckboxLabeled("enableMutagenLeakCheckboxLabel".Translate(), ref settings.enableMutagenLeak, "enableMutagenLeakCheckboxTooltip".Translate());
			checkBoxSection.CheckboxLabeled("enableMutagenCheckboxLabel".Translate(), ref settings.enableMutagenShipPart, "enableMutagenCheckboxTooltip".Translate());
			checkBoxSection.CheckboxLabeled("enableMutagenDiseasesCheckboxLabel".Translate(), ref settings.enableMutagenDiseases, "enableMutagenDiseasesCheckboxTooltip".Translate());
			checkBoxSection.CheckboxLabeled("enableMutagenMeteorCheckboxLabel".Translate(), ref settings.enableMutagenMeteor, "enableMutagenMeteorCheckboxTooltip".Translate());
			checkBoxSection.CheckboxLabeled("enableWildFormersCheckboxLabel".Translate(), ref settings.enableWildFormers, "enableWildFormersCheckboxTooltip".Translate());
			checkBoxSection.CheckboxLabeled("ChamberDatabaseIgnoresDataLimit".Translate(),
											ref settings.chamberDatabaseIgnoreStorageLimit,
											"ChamberDatabaseIgnoresDataLimitTooltip".Translate());
			checkBoxSection.CheckboxLabeled("PMInjectorsRequireTagging".Translate(), ref settings.injectorsRequireTagging,
											"PMInjectorsRequireTaggingTooltip".Translate());

			checkBoxSection.CheckboxLabeled("PMHazardousChaobulbs".Translate(), ref settings.hazardousChaobulbs, "PMHazardousChaobulbsTooltip".Translate());
			checkBoxSection.CheckboxLabeled("PMGenerateEndoGenesForAliens".Translate(), ref settings.generateEndoGenesForAliens, "PMGenerateEndoGenesForAliensTooltip".Translate());

			if (checkBoxSection.ButtonText("PMEnableMutationVisualsButton".Translate()))
				ShowVisibleRaceSelection();

			if (checkBoxSection.ButtonText("PMRaceReplacementButton".Translate()))
				ShowRaceReplacements();

			if (checkBoxSection.ButtonText("PMOptionalPatchesButton".Translate()))
				ShowOptionalPatches();

			if (checkBoxSection.ButtonText("PMAnimalAssociationsButton".Translate()))
				ShowAnimalAssociations();

			if (checkBoxSection.ButtonText("PMBlacklistFormerHumansButton".Translate()))
				ShowAnimalBlacklist();

			listingStandard.EndSection(checkBoxSection);


			listingStandard.Label($"{"transformChanceSliderLabel".Translate()}: {settings.transformChance.ToString("F1")}%");
			settings.transformChance = listingStandard.Slider(settings.transformChance, 0f, 100f);
			listingStandard.Label($"{"formerChanceSliderLabel".Translate()}: {settings.formerChance.ToStringByStyle(ToStringStyle.PercentTwo)}");
			settings.formerChance = listingStandard.Slider(settings.formerChance, 0f, 1f);
			listingStandard.Label($"{"partialChanceSliderLabel".Translate()}: {settings.partialChance.ToString("F1")}%");
			settings.partialChance = listingStandard.Slider(settings.partialChance, 0f, 100f);
			listingStandard.Label($"{"maxMutationThoughtsSliderLabel".Translate()}: {settings.maxMutationThoughts}");
			settings.maxMutationThoughts = (int)listingStandard.Slider(settings.maxMutationThoughts, 1, 10);

			listingStandard
			   .Label($"{nameof(PawnmorpherSettings.manhunterTfChance).Translate()}: {settings.manhunterTfChance.ToStringByStyle(ToStringStyle.PercentOne)}");
			settings.manhunterTfChance = listingStandard.Slider(settings.manhunterTfChance, 0, 1f);

			if (settings.manhunterTfChance > FormerHumanUtilities.MANHUNTER_EPSILON)
			{
				listingStandard
				   .Label($"{nameof(PawnmorpherSettings.friendlyManhunterTfChance).Translate()}: {settings.friendlyManhunterTfChance.ToStringByStyle(ToStringStyle.PercentOne)}");
				settings.friendlyManhunterTfChance = listingStandard.Slider(settings.friendlyManhunterTfChance, 0, 1f);

			}

			listingStandard
			   .Label($"{nameof(PawnmorpherSettings.hostileKeepFactionTfChance).Translate()}: {settings.hostileKeepFactionTfChance.ToStringByStyle(ToStringStyle.PercentOne)}");
			settings.hostileKeepFactionTfChance = listingStandard.Slider(settings.hostileKeepFactionTfChance, 0, 1f);

			if (Prefs.DevMode)
			{
				listingStandard.Label($"logging level:{settings.logLevel}");
				float f = (float)((int)settings.logLevel);
				var maxLevel = (int)LogLevel.Pedantic;
				f = listingStandard.Slider(maxLevel - f, 0, maxLevel);
				settings.logLevel = (LogLevel)Mathf.FloorToInt(Mathf.Clamp(maxLevel - f, 0, maxLevel));
			}

			listingStandard.End();

			base.DoSettingsWindowContents(inRect);
		}

		private void ShowVisibleRaceSelection()
		{
			if (settings.visibleRaces == null)
				settings.visibleRaces = new List<string>();

			UserInterface.Settings.Dialog_VisibleRaceSelection raceSelection = new UserInterface.Settings.Dialog_VisibleRaceSelection(settings.visibleRaces);
			Find.WindowStack.Add(raceSelection);
		}

		private void ShowRaceReplacements()
		{
			if (settings.raceReplacements == null)
				settings.raceReplacements = new Dictionary<string, string>();

			UserInterface.Settings.Dialog_RaceReplacements raceReplacements = new UserInterface.Settings.Dialog_RaceReplacements(settings.raceReplacements);
			Find.WindowStack.Add(raceReplacements);
		}

		private void ShowOptionalPatches()
		{
			if (settings.optionalPatches == null)
				settings.optionalPatches = new Dictionary<string, bool>();

			UserInterface.Settings.Dialog_OptionalPatches raceReplacements = new UserInterface.Settings.Dialog_OptionalPatches(settings.optionalPatches);
			Find.WindowStack.Add(raceReplacements);
		}


		private void ShowAnimalAssociations()
		{
			if (settings.animalAssociations == null)
				settings.animalAssociations = new Dictionary<string, string>();

			UserInterface.Settings.Dialog_AnimalAssociations animalAssociations = new UserInterface.Settings.Dialog_AnimalAssociations(settings.animalAssociations);
			Find.WindowStack.Add(animalAssociations);
		}


		private void ShowAnimalBlacklist()
		{
			UserInterface.Settings.Dialog_BlacklistAnimal animalAssociations = new UserInterface.Settings.Dialog_BlacklistAnimal(settings.animalBlacklist);
			Find.WindowStack.Add(animalAssociations);
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
