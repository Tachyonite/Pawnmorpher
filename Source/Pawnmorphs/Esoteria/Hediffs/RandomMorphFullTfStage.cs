// RandomMorphFullTfStage.cs modified by Iron Wolf for Pawnmorph on 01/25/2020 12:02 PM
// last updated 01/25/2020  12:02 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// transformation stage that picks a random morph to turn the pawn into
	/// </summary>
	/// <seealso cref="Verse.HediffStage" />
	/// <seealso cref="Pawnmorph.Hediffs.IExecutableStage" />
	/// <seealso cref="Pawnmorph.Hediffs.IInitializable" />
	public class RandomMorphFullTfStage : HediffStage, IExecutableStage, IInitializable
	{
		/// <summary>
		/// The morph or class to pick random morphs from 
		/// </summary>
		public AnimalClassBase morph;

		[Unsaved] private List<MorphDef> _morphs;

		/// <summary>
		/// The change chance
		/// </summary>
		public float changeChance = 100;

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
		MorphDef GetMorphFor(Pawn pawn)
		{
			int seed;

			unchecked
			{
				seed = pawn.thingIDNumber + (Find.TickManager.TicksAbs / RandomMorphTransformationStage.CYCLE_RATE);
			}



			Rand.PushState(seed);
			try
			{
				return AllMorphs.RandomElement();
			}
			finally
			{
				Rand.PopState();
			}
		}



		/// <summary>
		/// Gets all Configuration errors in this instance.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> ConfigErrors()
		{
			if (morph == null)
			{
				yield return $"field '{nameof(morph)}' was not set";
			}
		}



		/// <summary>called when the given hediff enters this stage</summary>
		/// <param name="hediff">The hediff.</param>
		public void EnteredStage(Hediff hediff)
		{
			if (!(Rand.Range(0, 100) < changeChance)) return;

			MorphDef morphFor = GetMorphFor(hediff.pawn);
			var transformer = morphFor?.fullTransformation?.GetAllTransformers().FirstOrDefault();
			if (transformer == null)
			{
				Log.Error($"could not find transformer for morph {morphFor?.defName ?? "NULL"} with full tf {morphFor?.fullTransformation?.defName ?? "NULL"}");
				return;
			}
			transformer.TransformPawn(hediff.pawn, hediff);
		}
	}
}