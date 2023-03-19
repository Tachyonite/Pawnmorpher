using System.Collections.Generic;
using Pawnmorph.DebugUtils;
using Pawnmorph.UserInterface;
using Pawnmorph.UserInterface.TreeBox;
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

		List<TreeNode_FilterBox> _nodes;
		FilterTreeBox _treeBox;
		Vector2 _sliderScrollPosition;

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
			if (_treeBox == null)
			{
				_nodes = GenerateTreeNodes();
				_treeBox = new FilterTreeBox(_nodes);
			}


			Rect optionList = new Rect(inRect);
			optionList.width = (optionList.width - 10) / 2;
			optionList.height = 10 * Text.LineHeight;
			Widgets.DrawMenuSection(optionList);
			_treeBox.Draw(optionList.ContractedBy(4));

			optionList.x = optionList.xMax + 10;

			Listing_Standard checkBoxSection = new Listing_Standard();
			Widgets.DrawMenuSection(optionList);
			checkBoxSection.Begin(optionList.ContractedBy(4));

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

			checkBoxSection.End();




			Rect sliderSectionRect = new Rect(inRect);
			sliderSectionRect.y = optionList.yMax + 4;
			sliderSectionRect.height = inRect.height - optionList.height - 4;
			Listing_Standard sliderSection = new Listing_Standard();

			int sliders = 14;
			if (Prefs.DevMode)
				sliders = 16;
			Rect viewRect = new Rect(0, 0, sliderSectionRect.width - 20, (sliders + 1) * (Text.LineHeight + sliderSection.verticalSpacing));
			Widgets.BeginScrollView(sliderSectionRect, ref _sliderScrollPosition, viewRect);

			sliderSection.Begin(viewRect);

			sliderSection.Label($"{"transformChanceSliderLabel".Translate()}: {settings.transformChance.ToString("F1")}%");
			settings.transformChance = sliderSection.Slider(settings.transformChance, 0f, 100f);
			sliderSection.Label($"{"formerChanceSliderLabel".Translate()}: {settings.formerChance.ToStringByStyle(ToStringStyle.PercentTwo)}");
			settings.formerChance = sliderSection.Slider(settings.formerChance, 0f, 1f);
			sliderSection.Label($"{"partialChanceSliderLabel".Translate()}: {settings.partialChance.ToString("F1")}%");
			settings.partialChance = sliderSection.Slider(settings.partialChance, 0f, 100f);
			sliderSection.Label($"{"maxMutationThoughtsSliderLabel".Translate()}: {settings.maxMutationThoughts}");
			settings.maxMutationThoughts = (int)sliderSection.Slider(settings.maxMutationThoughts, 1, 10);

			sliderSection
			   .Label($"{nameof(PawnmorpherSettings.manhunterTfChance).Translate()}: {settings.manhunterTfChance.ToStringByStyle(ToStringStyle.PercentOne)}");
			settings.manhunterTfChance = sliderSection.Slider(settings.manhunterTfChance, 0, 1f);

			if (settings.manhunterTfChance > FormerHumanUtilities.MANHUNTER_EPSILON)
			{
				sliderSection
				   .Label($"{nameof(PawnmorpherSettings.friendlyManhunterTfChance).Translate()}: {settings.friendlyManhunterTfChance.ToStringByStyle(ToStringStyle.PercentOne)}");
				settings.friendlyManhunterTfChance = sliderSection.Slider(settings.friendlyManhunterTfChance, 0, 1f);

			}

			sliderSection
			   .Label($"{nameof(PawnmorpherSettings.hostileKeepFactionTfChance).Translate()}: {settings.hostileKeepFactionTfChance.ToStringByStyle(ToStringStyle.PercentOne)}");
			settings.hostileKeepFactionTfChance = sliderSection.Slider(settings.hostileKeepFactionTfChance, 0, 1f);

			if (Prefs.DevMode)
			{
				sliderSection.Label($"logging level:{settings.logLevel}");
				float f = (float)((int)settings.logLevel);
				var maxLevel = (int)LogLevel.Pedantic;
				f = sliderSection.Slider(maxLevel - f, 0, maxLevel);
				settings.logLevel = (LogLevel)Mathf.FloorToInt(Mathf.Clamp(maxLevel - f, 0, maxLevel));
			}

			sliderSection.End();
			Widgets.EndScrollView();
			base.DoSettingsWindowContents(inRect);
		}

		private List<TreeNode_FilterBox> GenerateTreeNodes()
		{
			List<TreeNode_FilterBox> result = new List<TreeNode_FilterBox>();

			TreeNode_FilterBox coreNode = new TreeNode_FilterBox("Core");
			coreNode.AddChild("enableFalloutCheckboxLabel", "enableFalloutCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableFallout, x.height));
			coreNode.AddChild("enableMutagenLeakCheckboxLabel", "enableMutagenLeakCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableMutagenLeak, x.height));
			coreNode.AddChild("enableMutagenCheckboxLabel", "enableMutagenCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableMutagenShipPart, x.height));
			coreNode.AddChild("enableMutagenDiseasesCheckboxLabel", "enableMutagenDiseasesCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableMutagenDiseases, x.height));
			coreNode.AddChild("enableMutagenMeteorCheckboxLabel", "enableMutagenMeteorCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableMutagenMeteor, x.height));
			coreNode.AddChild("enableWildFormersCheckboxLabel", "enableWildFormersCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableWildFormers, x.height));
			coreNode.AddChild("ChamberDatabaseIgnoresDataLimit", "ChamberDatabaseIgnoresDataLimitTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.chamberDatabaseIgnoreStorageLimit, x.height));
			coreNode.AddChild("PMInjectorsRequireTagging", "PMInjectorsRequireTaggingTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.injectorsRequireTagging, x.height));
			coreNode.AddChild("PMHazardousChaobulbs", "PMHazardousChaobulbsTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.hazardousChaobulbs, x.height));

			result.Add(coreNode);

			return result;
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
