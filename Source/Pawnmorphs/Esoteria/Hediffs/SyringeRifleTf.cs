// SyringeRifleTf.cs created by Iron Wolf for Pawnmorph on 05/17/2020 7:39 PM
// last updated 05/17/2020  7:39 PM

using Pawnmorph.TfSys;
using Pawnmorph.ThingComps;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Verse.Hediff" />
	public class SyringeRifleTf : MorphTf
	{
		private PawnKindDef _chosenKind;


		/// <summary>
		/// Gets a value indicating whether this instance should be removed.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance should be removed; otherwise, <c>false</c>.
		/// </value>
		public override bool ShouldRemove => MutationStatValue <= 0;


		/// <summary>
		/// Gets a random pawnkind animal
		/// </summary>
		private PawnKindDef ChooseRandomAnimal()
		{
			return FormerHumanUtilities.AllRegularFormerHumanPawnkindDefs.RandomElement();
		}


		/// <summary>
		/// Initializes this instance with the given weapon
		/// </summary>
		/// <param name="weapon">The weapon.</param>
		public void Initialize(Thing weapon)
		{
			_chosenKind = weapon?.TryGetComp<AnimalSelectorComp>()?.ChosenKind;
			if (_chosenKind == null)
			{
				_chosenKind = ChooseRandomAnimal();
			}

			ResetMutationCaches();
		}

		/// <summary>
		///     Called when the stage changes.
		/// </summary>
		/// <param name="currentStage">The last stage.</param>
		protected override void OnStageChanged(HediffStage currentStage)
		{
			base.OnStageChanged(currentStage);
			if (currentStage == def.stages[def.stages.Count - 1]) DoTf();
		}


		private void DoTf()
		{
			if (_chosenKind == null)
			{
				_chosenKind = ChooseRandomAnimal();
			}
			var tfRequest = new TransformationRequest(_chosenKind, pawn);
			var res = MutagenDefOf.defaultMutagen.MutagenCached.Transform(tfRequest);
			if (res != null)
			{
				Find.World.GetComponent<PawnmorphGameComp>().AddTransformedPawn(res);
			}
		}


		/// <summary>
		/// Gets the kind of the chosen.
		/// </summary>
		/// <value>
		/// The kind of the chosen.
		/// </value>
		public PawnKindDef ChosenKind => _chosenKind;

		/// <summary>
		/// Exposes the data.
		/// </summary>
		public override void ExposeData()
		{
			Scribe_Defs.Look(ref _chosenKind, "chosenKind");
			base.ExposeData();
		}
	}
}