using Pawnmorph.Hediffs;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// hediff comp to add a single mutation then remove the parent hediff 
	/// </summary>
	/// <seealso cref="Verse.HediffComp" />
	public class HediffComp_Single : HediffComp, ITfHediffObserverComp
	{
		/// <summary>The stacks</summary>
		public int stacks = 1;

		/// <summary>called to expose the data in this comp</summary>
		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Values.Look(ref stacks, nameof(stacks), 1);
		}

		/// <summary>
		/// Gets extra label contents for the parent hediff.
		/// </summary>
		/// <value>
		/// The comp label in brackets extra.
		/// </value>
		public override string CompLabelInBracketsExtra => "x" + stacks;

		/// <summary>
		/// Gets a value indicating whether the parent hediff should be removed.
		/// </summary>
		/// <value>
		///   <c>true</c> if the parent hediff should be removed; otherwise, <c>false</c>.
		/// </value>
		public override bool CompShouldRemove => stacks <= 0;

		/// <summary>called after the parent is merged with the other hediff</summary>
		/// <param name="other">The other.</param>
		public override void CompPostMerged(Hediff other)
		{
			base.CompPostMerged(other);

			var comp = other.TryGetComp<HediffComp_Single>();
			var oStacks = stacks;
			stacks = Mathf.Min(Props.maxStacks, stacks + comp.stacks);
			if (oStacks != stacks && other is Hediff_MutagenicBase mBase)
			{
				mBase.ClearCaches();
			}
		}
		/// <summary>Gets the properties.</summary>
		/// <value>The properties.</value>
		public HediffCompProperties_Single Props
		{
			get
			{
				return (HediffCompProperties_Single)props;
			}
		}

		/// <summary>
		/// called when the morph hediff is about to start visiting body parts.
		/// </summary>
		public void Init()
		{

		}

		/// <summary>
		/// called when the hediff stage changes.
		/// </summary>
		public void StageChanged()
		{

		}

		/// <summary>
		/// called when the morph tf observes the give body part record on the given pawn
		/// </summary>
		/// <param name="record">The record observed. if null a observing whole body hediffs</param>
		public void Observe(BodyPartRecord record)
		{

		}

		/// <summary>
		/// called after the given mutation is added to the pawn.
		/// </summary>
		/// <param name="newMutation">The new mutation.</param>
		public void MutationAdded(Hediff_AddedMutation newMutation)
		{
			stacks--;
		}
	}
}
