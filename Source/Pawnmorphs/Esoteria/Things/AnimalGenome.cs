using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Things
{
	internal class AnimalGenome : Thing
	{
		PawnKindDef _animalKind;

		public override void PostMake()
		{
			base.PostMake();
			IReadOnlyList<PawnKindDef> animals = FormerHumanUtilities.AllRegularFormerHumanPawnkindDefs;
			if (def is AnimalGenomeDef genomeDef)
			{
				if (genomeDef.restricted)
					animals = FormerHumanUtilities.AllRestrictedFormerHumanPawnkindDefs;
			}

			_animalKind = animals.RandomElement();
		}

		public override bool CanStackWith(Thing other)
		{
			if (base.CanStackWith(other) == false)
				return false;

			if (other is AnimalGenome animalGenome)
			{
				if (animalGenome._animalKind == _animalKind)
					return true;
			}
			return false;
		}

		public override string LabelCapNoCount
		{
			get
			{
				return $"{base.LabelCapNoCount} ({_animalKind.LabelCap})";
			}
		}

		public override string LabelCap => base.LabelCap;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref _animalKind, "animalKind");
		}
	}
}
