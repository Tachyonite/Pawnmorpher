using System;
using System.Collections.Generic;
using System.Linq;
using Pawnmorph.DebugUtils;
using Pawnmorph.HPatches.Optional;
using Pawnmorph.UserInterface;
using Pawnmorph.UserInterface.TreeBox;
using RimWorld;
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

		public static PawnmorphGameComp WorldComp;

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
				_treeBox.ExpandAll();
			}

			Listing_Standard checkBoxSection = new Listing_Standard();

			Rect optionList = new Rect(inRect);
			optionList.height = 3 * (30 + checkBoxSection.verticalSpacing) + 8;
			Widgets.DrawMenuSection(optionList);
			checkBoxSection.Begin(optionList.ContractedBy(4));
			checkBoxSection.ColumnWidth = (checkBoxSection.ColumnWidth - 14) / 2;
			if (checkBoxSection.ButtonText("PMEnableMutationVisualsButton".Translate()))
				ShowVisibleRaceSelection();

			if (checkBoxSection.ButtonText("PMRaceReplacementButton".Translate()))
				ShowRaceReplacements();

			if (checkBoxSection.ButtonText("PMOptionalPatchesButton".Translate()))
				ShowOptionalPatches();

			checkBoxSection.NewColumn();

			if (checkBoxSection.ButtonText("PMAnimalAssociationsButton".Translate()))
				ShowAnimalAssociations();

			if (checkBoxSection.ButtonText("PMBlacklistFormerHumansButton".Translate()))
				ShowAnimalBlacklist();


			checkBoxSection.End();



			Rect sliderSectionRect = new Rect(inRect);
			sliderSectionRect.y = optionList.yMax;
			sliderSectionRect.height = inRect.height - optionList.height;

			_treeBox.Draw(sliderSectionRect.ContractedBy(4));
			base.DoSettingsWindowContents(inRect);
		}

		private List<TreeNode_FilterBox> GenerateTreeNodes()
		{
			List<TreeNode_FilterBox> result = new List<TreeNode_FilterBox>();

			TreeNode_FilterBox coreNode = new TreeNode_FilterBox(ExpansionDefOf.Core.label);
			coreNode.AddChild("enableFalloutCheckboxLabel", "enableFalloutCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableFallout, x.height));
			coreNode.AddChild("enableMutagenLeakCheckboxLabel", "enableMutagenLeakCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableMutagenLeak, x.height));
			coreNode.AddChild("enableMutagenCheckboxLabel", "enableMutagenCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableMutagenShipPart, x.height));
			coreNode.AddChild("enableMutagenDiseasesCheckboxLabel", "enableMutagenDiseasesCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableMutagenDiseases, x.height));
			coreNode.AddChild("enableMutagenMeteorCheckboxLabel", "enableMutagenMeteorCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableMutagenMeteor, x.height));
			coreNode.AddChild("enableWildFormersCheckboxLabel", "enableWildFormersCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableWildFormers, x.height));
			coreNode.AddChild("enableMutationAdaptedStageLabelCheckboxLabel", "enableMutationAdaptedStageLabelCheckboxTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.enableMutationAdaptedStageLabel, x.height));
			coreNode.AddChild("PMHazardousChaobulbs", "PMHazardousChaobulbsTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.hazardousChaobulbs, x.height));
			coreNode.AddChild("PMGenerateEndoGenesForAliens", "PMGenerateEndoGenesForAliensTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.generateEndoGenesForAliens, x.height));

			coreNode.AddChild("transformChanceSliderLabel", null, (in Rect x) => Widgets.HorizontalSlider(x, ref settings.transformChance, new FloatRange(0, 100), settings.transformChance.ToString("F1") + "%"), true);
			coreNode.AddChild("formerChanceSliderLabel", null, (in Rect x) => Widgets.HorizontalSlider(x, ref settings.formerChance, new FloatRange(0, 1), settings.formerChance.ToStringByStyle(ToStringStyle.PercentTwo)), true);
			coreNode.AddChild("partialChanceSliderLabel", null, (in Rect x) => Widgets.HorizontalSlider(x, ref settings.partialChance, new FloatRange(0, 100), settings.partialChance.ToString("F1") + "%"), true);
			coreNode.AddChild("maxMutationThoughtsSliderLabel", null, (in Rect x) => settings.maxMutationThoughts = (int)Widgets.HorizontalSlider(x, (float)settings.maxMutationThoughts, 0, 10, true, label: settings.maxMutationThoughts.ToString(), roundTo: 0), true);


			TreeNode_FilterBox manhunterChanceNode = coreNode.AddChild(nameof(PawnmorpherSettings.manhunterTfChance), null, null, true);
			TreeNode_FilterBox friendlyManhunterChanceNode = coreNode.AddChild(nameof(PawnmorpherSettings.friendlyManhunterTfChance), null, (in Rect x) => Widgets.HorizontalSlider(x, ref settings.friendlyManhunterTfChance, new FloatRange(0, 1), settings.friendlyManhunterTfChance.ToStringByStyle(ToStringStyle.PercentOne)), true);

			manhunterChanceNode.Callback = (in Rect x) =>
			{
				Widgets.HorizontalSlider(x, ref settings.manhunterTfChance, new FloatRange(0, 1), settings.manhunterTfChance.ToStringByStyle(ToStringStyle.PercentOne));
				friendlyManhunterChanceNode.Enabled = settings.manhunterTfChance > FormerHumanUtilities.MANHUNTER_EPSILON;
			};

			coreNode.AddChild(nameof(PawnmorpherSettings.hostileKeepFactionTfChance), null, (in Rect x) => Widgets.HorizontalSlider(x, ref settings.hostileKeepFactionTfChance, new FloatRange(0, 1), settings.hostileKeepFactionTfChance.ToStringByStyle(ToStringStyle.PercentOne)), true);

			coreNode.AddChild(nameof(PawnmorpherSettings.mutationLogLength), "mutationLogLengthTooltip", (in Rect x) => settings.mutationLogLength = (int)Widgets.HorizontalSlider(x, settings.mutationLogLength, 0, 30, label: settings.mutationLogLength.ToString(), roundTo: 0));


			// Sequencer node
			TreeNode_FilterBox sequencerNode = coreNode.AddChild("PMSequencer");
			sequencerNode.AddChild("PMSequencerMultipler", "PMSequencerMultiplerTooltip", callback: (in Rect x) => settings.SequencingMultiplier = Widgets.HorizontalSlider(x, settings.SequencingMultiplier, 0.1f, 10, true, settings.SequencingMultiplier.ToStringPercent(), 0.1f.ToStringPercent(), 10f.ToStringPercent(), 0.1f));
			sequencerNode.AddChild("PMSequencerAutoAnimalGenome", "PMSequencerAutoAnimalGenomeTooltip", callback: (in Rect x) => Widgets.Checkbox(x.position, ref settings.AutoSequenceAnimalGenome));

			result.Add(coreNode);


			// Add any additional configurable objects.
			foreach (Interfaces.IConfigurableObject configurableObject in settings.GetAllConfigurableObjects())
			{
				TreeNode_FilterBox objectNode = new TreeNode_FilterBox(configurableObject.Caption);
				configurableObject.GenerateMenu(objectNode);

				if (objectNode.children.Count > 0)
					result.Add(objectNode);
			}





			TreeNode_FilterBox debugNode = new TreeNode_FilterBox("PMDebug".Translate());


			debugNode.AddChild("ChamberDatabaseIgnoresDataLimit", "ChamberDatabaseIgnoresDataLimitTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.chamberDatabaseIgnoreStorageLimit, x.height));
			debugNode.AddChild("PMInjectorsRequireTagging", "PMInjectorsRequireTaggingTooltip", (in Rect x) => Widgets.Checkbox(x.position, ref settings.injectorsRequireTagging, x.height));

			debugNode.AddChild("PMDebugLevel", null, (in Rect x) =>
			{
				var maxLevel = (int)LogLevel.Pedantic;
				float f = (float)settings.logLevel;
				settings.logLevel = (LogLevel)Mathf.RoundToInt(Widgets.HorizontalSlider(x, f, 0, maxLevel, true, label: settings.logLevel.ToString()));
			}, true);


			result.Add(debugNode);


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
