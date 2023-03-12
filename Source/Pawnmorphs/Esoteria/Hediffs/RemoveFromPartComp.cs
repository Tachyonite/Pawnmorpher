// RemoveFromPartComp.cs modified by Iron Wolf for Pawnmorph on 09/25/2019 5:42 PM
// last updated 09/25/2019  5:42 PM

using System.Linq;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	///     comp that removes other mutations when it's parent is added
	/// </summary>
	/// <seealso>
	///     <cref>Pawnmorph.Utilities.HediffCompBase{Pawnmorph.Hediffs.RemoveFromPartCompProperties}</cref>
	/// </seealso>
	public class RemoveFromPartComp : HediffCompBase<RemoveFromPartCompProperties>
	{

		private int _addedTick = -1;

		/// <summary>
		///     Gets the tick (time) this comp was added.
		/// </summary>
		/// <value>
		///     The added tick.
		/// </value>
		public int AddedTick => _addedTick;

		/// <summary>
		///     Gets the layer.
		/// </summary>
		/// <value>
		///     The layer.
		/// </value>
		public MutationLayer Layer => Props.layer;

		/// <summary>
		///     exposes all data for this comp.
		/// </summary>
		public override void CompExposeData()
		{
			Scribe_Values.Look(ref _addedTick, "addedTick", -1, true);
			Scribe_Values.Look(ref _shouldRemove, nameof(CompShouldRemove));
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (_addedTick == -1)
				{
					_addedTick = Find.TickManager.TicksAbs;

				}
				RemoveOtherMutations();
			}


		}

		/// <summary>
		///     called after this comp is created .
		/// </summary>
		public override void CompPostMake()
		{
			base.CompPostMake();

			_addedTick = Current.ProgramState != ProgramState.Playing ? 0 : Find.TickManager.TicksAbs;
		}

		/// <summary>
		///     called after this instance was added to the pawn.
		/// </summary>
		/// <param name="dinfo">The damage info.</param>
		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			base.CompPostPostAdd(dinfo);
			if (_shouldRemove) return;
			RemoveOtherMutations();
		}

		private bool _shouldRemove;

		/// <summary>
		/// Gets a value indicating whether the parent hediff should be removed.
		/// </summary>
		/// <value>
		///   <c>true</c> if hediff should be removed; otherwise, <c>false</c>.
		/// </value>
		public override bool CompShouldRemove => _shouldRemove;

		private void RemoveOtherMutations()
		{
			foreach (Hediff_AddedMutation otherHediff in Pawn
														.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>()
														.Where(m => m != parent && m.Part == parent.Part))
			{
				var oComp = otherHediff.TryGetComp<RemoveFromPartComp>();
				if (oComp == null) continue; //the hediffs must have this comp to 

				if (oComp._addedTick > _addedTick)
					continue; //the part to be removed must be older or the same age as this comp 
				if (oComp.Props.layer == Props.layer)
				{
					oComp._shouldRemove = true;
				}
			}
		}
	}

	/// <summary>
	///     properties for the comp that removes other mutations when it's parent is added
	/// </summary>
	/// <seealso>
	///     <cref>Pawnmorph.Utilities.HediffCompPropertiesBase{Pawnmorph.Hediffs.RemoveFromPartComp}</cref>
	/// </seealso>
	public class RemoveFromPartCompProperties : HediffCompPropertiesBase<RemoveFromPartComp>
	{
		/// <summary>
		///     The layer to check for mutations
		/// </summary>
		public MutationLayer layer;
	}

	/// <summary>
	///     flags representing the 'layer' a mutation resides on
	/// </summary>
	public enum MutationLayer
	{

		/// <summary>
		///     the mutation affects the core of the part
		/// </summary>
		Core = 1,

		/// <summary>
		///     mutation affects the surface of a part
		/// </summary>
		Skin = 2
	}
}