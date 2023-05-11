// RandomMorphTransformationStage.cs modified by Iron Wolf for Pawnmorph on 01/25/2020 11:52 AM
// last updated 01/25/2020  11:52 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// transformation stage that picks a random set of mutations for each pawn 
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.TransformationStageBase" />
	public class RandomMorphTransformationStage : TransformationStageBase
	{




		/// <summary>
		/// returns all configuration errors in this stage
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			if (morph == null)
				yield return "morph field not set";
		}

		/// <summary>
		/// The morph or class to pick from 
		/// </summary>
		public AnimalClassBase morph;

		/// <summary>
		/// how far up the class tree to take mutations from 
		/// </summary>
		public int height;

		/// <summary>
		/// if true, this stage can give restricted mutations 
		/// </summary>
		public bool allowRestricted;

		[Unsaved, NotNull] private readonly Dictionary<Thing, List<MutationEntry>> _cache = new Dictionary<Thing, List<MutationEntry>>();

		[Unsaved] private List<MorphDef> _morphs;
		/// <summary>
		/// a list of morph categories not to include 
		/// </summary>
		public List<MorphCategoryDef> categoryBlackList = new List<MorphCategoryDef>();

		[NotNull]
		List<MorphDef> AllMorphs
		{
			get
			{
				if (_morphs == null)
				{
					_morphs = new List<MorphDef>();
					foreach (MorphDef morphDef in morph.GetAllMorphsInClass())
					{
						//if this morph is in any of the black listed categories ignore it 
						if (morphDef.categories.MakeSafe().Any(c => categoryBlackList.Contains(c)))
							continue;
						_morphs.Add(morphDef);
					}

				}

				return _morphs;
			}
		}


		/// <summary>
		/// how fast the mutation types change 
		/// </summary>
		public const int CYCLE_RATE = TimeMetrics.TICKS_PER_DAY * 2 / 3;

		/// <summary>
		/// Gets the entries for the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="source"></param>
		/// <returns></returns>
		public override IEnumerable<MutationEntry> GetEntries(Pawn pawn, Hediff source)
		{
			if (_cache.TryGetValue(pawn, out List<MutationEntry> lst))
				return lst;
			lst = new List<MutationEntry>();

			int seed;

			unchecked
			{
				seed = pawn.thingIDNumber + (Find.TickManager.TicksAbs / CYCLE_RATE);
			}



			Rand.PushState(seed);
			try
			{
				var rMorph = AllMorphs.RandomElement();



				IReadOnlyList<MutationDef> allMutations;

				HashSet<BodyPartDef> chosenParts = new HashSet<BodyPartDef>();
				if (height > 0)
				{
					AnimalClassBase cBase = rMorph;
					int i = 0;
					while (cBase != null && i < height)
					{
						var parent = cBase.ParentClass;
						if (parent == null) break;
						i++;
						cBase = parent;
					}

					cBase = cBase ?? rMorph;
					allMutations = cBase.GetAllMutationIn();
				}
				else
					allMutations = rMorph.AllAssociatedMutations;

				List<BodyPartDef> partDefsScratch = new List<BodyPartDef>();

				foreach (MutationDef mutation in allMutations) //get all mutations from the randomly picked morph 
				{
					partDefsScratch.Clear();
					if (!allowRestricted && mutation.IsRestricted) continue;

					if (!mutation.parts.NullOrEmpty() && Rand.Chance(0.9f))
					{
						partDefsScratch.AddRange(mutation.parts.Where(p => !chosenParts.Contains(p)));
						if (partDefsScratch.Count == 0) continue;

						var rPart = partDefsScratch.RandomElement();
						chosenParts.Add(rPart);
					}

					var mEntry = new MutationEntry
					{
						addChance = mutation.defaultAddChance,
						blocks = mutation.defaultBlocks,
						mutation = mutation
					};
					lst.Add(mEntry);
				}

				_cache[pawn] = lst; //cache the results so we only have to do this once per pawn 
				return lst;
			}
			finally
			{
				Rand.PopState();
			}
		}
	}
}