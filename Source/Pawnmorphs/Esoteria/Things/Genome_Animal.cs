using Pawnmorph.Chambers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Things
{
	/// <summary>
	/// A genome thing class that selects a random animal when created and stores it on the thing instance.
	/// </summary>
	internal class Genome_Animal : ThingWithComps
	{
		PawnKindDef _animalKind;

		/// <summary>
		/// Gets the contained animal kind of this genome.
		/// </summary>
		public PawnKindDef AnimalKind => _animalKind;

		public override void PostMake()
		{
			base.PostMake();
			SelectAnimal();
		}

		private void SelectAnimal()
		{
			IReadOnlyList<PawnKindDef> animals = null;
			var modExtension = def.GetModExtension<ModExtensions.AnimalFilterModExtension>();
			if (modExtension != null)
			{
				if (modExtension.allowNormal && modExtension.allowRestricted)
					animals = FormerHumanUtilities.AllFormerHumanPawnkindDefs;
				else if (modExtension.allowRestricted)
					animals = FormerHumanUtilities.AllRestrictedFormerHumanPawnkindDefs;
				else if (modExtension.allowNormal)
					animals = FormerHumanUtilities.AllRegularFormerHumanPawnkindDefs;
			}

			if (animals == null || animals.Count == 0)
				animals = FormerHumanUtilities.AllRegularFormerHumanPawnkindDefs;

			_animalKind = animals.Where(x => x.race.IsTaggable()).RandomElement();
		}

		public override bool CanStackWith(Thing other)
		{
			if (base.CanStackWith(other) == false)
				return false;

			if (other is Genome_Animal animalGenome)
			{
				if (animalGenome._animalKind == _animalKind)
					return true;
			}
			return false;
		}

		public override string LabelNoCount
		{
			get
			{
				return $"{base.LabelNoCount} ({_animalKind.LabelCap})";
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref _animalKind, "animalKind");

			if (_animalKind == null)
				SelectAnimal();
		}
	}
}
