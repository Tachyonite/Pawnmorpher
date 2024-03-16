using System;
using System.Collections.Generic;
using System.Linq;
using AlienRace;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface.Preview
{
	internal class HumanlikePreview : ThingPreview
	{
		Pawn _pawn;

		/// <summary>
		/// Gets the gender of the preview pawn.
		/// </summary>
		public Gender Gender => _pawn.gender;

		/// <summary>
		/// Adjusts the text to match preview pawn.
		/// </summary>
		/// <param name="input">The input text to format.</param>
		/// <returns></returns>
		public string AdjustText(string input)
		{
			return input.AdjustedFor(_pawn);
		}

		/// <summary>
		/// Sets the preview gender.
		/// </summary>
		/// <param name="gender">The gender.</param>
		/// <param name="bodyType">Type of the body.</param>
		public void SetGender(Gender gender, BodyTypeDef bodyType = null)
		{
			if (bodyType == null)
			{
				// Default to base gendered.
				if (gender == Gender.Male)
					bodyType = BodyTypeDefOf.Male;
				else
					bodyType = BodyTypeDefOf.Female;
			}

			_pawn.story.bodyType = bodyType;
			_pawn.gender = gender;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HumanlikePreview"/> class.
		/// </summary>
		/// <param name="height">The preview texture height.</param>
		/// <param name="width">The preview texture width.</param>
		/// <param name="race">The pawn race to render.</param>
		/// <exception cref="System.ArgumentNullException">Race cannot be null.</exception>
		public HumanlikePreview(int height, int width, ThingDef_AlienRace race)
			: base(height, width)
		{
			if (race == null)
				throw new ArgumentNullException(nameof(race));

			_pawn = new Pawn();
			_pawn.def = race;
			_pawn.gender = Gender.Male;
			_pawn.kindDef = PawnKindDefOf.Colonist;
			PawnComponentsUtility.CreateInitialComponents(_pawn);

			_pawn.Name = new NameTriple("", "X", "");
			_pawn.story.birthLastName = "";
			_pawn.story.Title = "";
			_pawn.story.bodyType = race.alienRace?.generalSettings?.alienPartGenerator?.bodyTypes[0] ?? BodyTypeDefOf.Male;
			_pawn.story.headType = race.alienRace?.generalSettings?.alienPartGenerator?.HeadTypes[0] ?? HeadTypeDefOf.Skull;
			_pawn.story.hairDef = HairDefOf.Shaved;
			_pawn.ageTracker.AgeBiologicalTicks = _pawn.ageTracker.AdultMinAgeTicks + 1;
			_pawn.InitializeComps();
			_pawn.GetComp<AlienPartGenerator.AlienComp>().OverwriteColorChannel("skin", Color.white, Color.white);
			_pawn.GetComp<AlienPartGenerator.AlienComp>().OverwriteColorChannel("hair", Color.gray, Color.gray);
			Thing = _pawn;
		}

		protected override void OnRefresh()
		{
			if (_pawn == null)
				return;

			_pawn.Drawer.renderer.renderTree.SetDirty();
		}

		/// <summary>
		/// Adds a mutation to a specific body part.
		/// </summary>
		/// <param name="hediff">The hediff.</param>
		/// <param name="bodyPart">The body part.</param>
		public void AddMutation(MutationDef hediff, BodyPartDef bodyPart)
		{
			IEnumerable<BodyPartRecord> parts = _pawn.RaceProps.body.GetPartsWithDef(bodyPart);

			foreach (BodyPartRecord part in parts)
			{
				Hediff mutation = _pawn.health.AddHediff(hediff, part);
				mutation.Severity = hediff.maxSeverity;
			}
		}

		/// <summary>
		/// Sets the severity of all mutations of specific type.
		/// </summary>
		/// <param name="mutation">The mutation type to change.</param>
		/// <param name="severity">The severity level to change mutations to.</param>
		public void SetSeverity(MutationDef mutation, float severity)
		{
			IEnumerable<Hediff_AddedMutation> mutations = _pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>().Where(x => x.Def == mutation);
			foreach (Hediff_AddedMutation item in mutations)
			{
				item.Severity = severity;
			}
		}

		/// <summary>
		/// Adds the specified mutation to all body parts that support it.
		/// </summary>
		/// <param name="hediff">The mutation to add.</param>
		public void AddMutation(MutationDef hediff)
		{
			foreach (BodyPartDef part in hediff.parts)
				AddMutation(hediff, part);
		}

		/// <summary>
		/// Clears all mutations.
		/// </summary>
		public void ClearMutations()
		{
			_pawn.health.RemoveAllHediffs();
		}
	}
}
