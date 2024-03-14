// TransformedPawn.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 7:26 PM
// last updated 08/13/2019  8:16 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.ThingComps;
using Pawnmorph.Thoughts;
using RimWorld;
using Verse;

namespace Pawnmorph.TfSys
{
	/// <summary>
	///     base class for storing a "transformed pawn" in such a way that the original can be retried
	/// </summary>
	public abstract class TransformedPawn : IExposable
	{
		/// <summary>The mutagen definition</summary>
		public MutagenDef mutagenDef;


		private int? _transformedTick;

		/// <summary>
		///     Initializes a new instance of the <see cref="TransformedPawn" /> class.
		/// </summary>
		protected TransformedPawn()
		{
			_transformedTick = null;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="TransformedPawn" /> class.
		/// </summary>
		/// <param name="transformedTick">timestamp the pawns were transformed.</param>
		protected TransformedPawn(int? transformedTick)
		{
			_transformedTick = transformedTick;
		}

		/// <summary>Exposes the data.</summary>
		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref mutagenDef, nameof(mutagenDef));

			Scribe_Values.Look(ref _transformedTick, nameof(TransformedTick));
		}


		/// <summary>
		///     Gets tick the pawns were transformed .
		/// </summary>
		/// <value>
		///     the tick the pawns were transformed. A null value indicates the pawn(s) were transformed an unknown amount of time
		///     in the past
		/// </value>
		public int? TransformedTick => _transformedTick;

		/// <summary>
		///     Gets the faction that turned this pawn into an animal.
		/// </summary>
		/// <value>
		///     The faction responsible.
		/// </value>
		[CanBeNull]
		public abstract Faction FactionResponsible { get; }

		//using enumerable for these was for planned features of insects that never happend 
		//these should be changed for efficiency 

		/// <summary>
		///     Gets the original pawns.
		/// </summary>
		/// <value>
		///     The original pawns.
		/// </value>
		[NotNull]
		public abstract IEnumerable<Pawn> OriginalPawns { get; }


		/// <summary>
		///     Gets the transformed pawns.
		/// </summary>
		/// <value>
		///     The transformed pawns.
		/// </value>
		[NotNull]
		public abstract IEnumerable<Pawn> TransformedPawns { get; }


		/// <summary>
		///     Returns true if this instance is valid.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is valid; otherwise, <c>false</c>.
		/// </value>
		public bool IsValid
		{
			get
			{
				foreach (Pawn originalPawn in OriginalPawns)
					if (originalPawn.DestroyedOrNull())
						return false;

				foreach (Pawn transformedPawn in TransformedPawns)
					if (transformedPawn.DestroyedOrNull())
						return false;

				return true;
			}
		}


		/// <summary>
		///     Gets a value indicating whether this instance can be reverted.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can be reverted; otherwise, <c>false</c>.
		/// </value>
		public abstract bool CanRevert { get; }


		//some debug stuff         
		/// <summary>Gets the originals debug string.</summary>
		/// <value>The originals debug string.</value>
		public IEnumerable<string> OriginalsDebugString
		{
			get { return OriginalPawns.Select(p => p?.ThingID ?? "[Null]"); }
		}

		/// <summary>Gets the transformed debug string.</summary>
		/// <value>The transformed debug string.</value>
		public IEnumerable<string> TransformedDebugString
		{
			get { return TransformedPawns.Select(p => p?.ThingID ?? "[Null]"); }
		}

		/// <summary>Creates a new transformed pawn instance out of the given original pawn and transformed pawn.</summary>
		/// for backwards compatibility with old saves, should not be used in new code
		/// <param name="original">The original.</param>
		/// <param name="transformed">The transformed.</param>
		/// <returns></returns>
		public static TransformedPawn Create(Pawn original, Pawn transformed)
		{
			return new
				TransformedPawnSingle //just using some factory methods for now, want to move this somewhere else eventually

			{
				animal = transformed,
				original = original
			};
		}

		/// <summary>Creates a new TransformedPawn instance out of the given original pawns and the specified transformed pawn</summary>
		/// for backwards compatibility with old saves, should not be used with new code
		/// <param name="originals">The originals.</param>
		/// <param name="transformed">The transformed.</param>
		/// <returns></returns>
		public static TransformedPawn Create(IEnumerable<Pawn> originals, Pawn transformed)
		{
			return new MergedPawns
			{
				originals = originals.ToList(),
				meld = transformed
			};
		}

		/// <summary>Create a new TransformedPawn instance from the given original pawns and the transformed pawn</summary>
		/// for backwards compatibility with old saves, should not be used with new code
		/// <param name="original0">The original0.</param>
		/// <param name="original1">The original1.</param>
		/// <param name="meld">The meld.</param>
		/// <returns></returns>
		public static TransformedPawn Create(Pawn original0, Pawn original1, Pawn meld)
		{
			return new MergedPawns
			{
				meld = meld,
				originals = new List<Pawn> { original0, original1 }
			};
		}

		/// <summary>Creates the specified inst.</summary>
		/// for backwards compatibility with old saves, should not be used with new code
		/// <param name="inst">The inst.</param>
		/// <returns></returns>
		[Obsolete]
		public static TransformedPawn Create(PawnMorphInstance inst)
		{
			return new TransformedPawnSingle
			{
				original = inst.origin,
				animal = inst.replacement
			};
		}

		/// <summary>Creates the specified inst.</summary>
		/// for backwards compatibility with old saves, should not be used with new code
		/// <param name="inst">The inst.</param>
		/// <returns></returns>
		[Obsolete]
		public static TransformedPawn Create(PawnMorphInstanceMerged inst)
		{
			return Create(inst.origin, inst.origin2, inst.replacement);
		}

		/// <summary>
		///     Gets the status of the given pawn with regards to this instance
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>if the pawn is the original pawn, transformed pawn, or null if neither</returns>
		public TransformedStatus? GetStatus(Pawn pawn)
		{
			//checking thingID manually because SaveReference seems to make new pawns when loading sometimes 
			//this is hacky, but can't come up with a better solution

			foreach (Pawn originalPawn in OriginalPawns)
				if (originalPawn == pawn || originalPawn?.ThingID == pawn?.ThingID)
					return TransformedStatus.Original;

			foreach (Pawn tfPawn in TransformedPawns)
				if (tfPawn == pawn || tfPawn?.ThingID == pawn?.ThingID)
					return TransformedStatus.Transformed;

			return null;
		}

		/// <summary>generates a debug string</summary>
		/// <returns></returns>
		public string ToDebugString()
		{
			return
				$"originals [{string.Join(",", OriginalsDebugString.ToArray())}]\ntransformed [{string.Join(",", TransformedDebugString.ToArray())}]";
		}

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return
				$"[{string.Join(",", OriginalsDebugString.ToArray())}] => [{string.Join(",", TransformedDebugString.ToArray())}]";
		}
	}


	/// <summary>
	///     transformed pawn instance for a single original-animal pair
	/// </summary>
	/// <seealso cref="Pawnmorph.TfSys.TransformedPawn" />
	public class TransformedPawnSingle : TransformedPawn
	{
		/// <summary>The original pawn</summary>
		public Pawn original;

		/// <summary>The transformed pawn</summary>
		public Pawn animal;

		/// <summary>
		///     The reaction status
		/// </summary>
		public FormerHumanReactionStatus reactionStatus;

		/// <summary>
		///     The faction responsible for turning this pawn into an animal
		/// </summary>
		public Faction factionResponsible;

		/// <summary>
		///     Initializes a new instance of the <see cref="TransformedPawnSingle" /> class.
		/// </summary>
		public TransformedPawnSingle()
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="TransformedPawnSingle" /> class.
		/// </summary>
		/// <param name="transformedTick">timestamp the pawns were transformed.</param>
		public TransformedPawnSingle(int? transformedTick) : base(transformedTick)
		{
		}

		/// <summary>
		///     Gets the faction that turned this pawn into an animal.
		/// </summary>
		/// <value>
		///     The faction responsible.
		/// </value>
		public override Faction FactionResponsible => factionResponsible;

		/// <summary>Gets the original pawns.</summary>
		/// <value>The original pawns.</value>
		public override IEnumerable<Pawn> OriginalPawns
		{
			get { yield return original; }
		}

		/// <summary>Gets the transformed pawns.</summary>
		/// <value>The transformed pawns.</value>
		public override IEnumerable<Pawn> TransformedPawns
		{
			get { yield return animal; }
		}

		/// <summary>
		///     Gets a value indicating whether this instance can be reverted.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can be reverted; otherwise, <c>false</c>.
		/// </value>
		public override bool CanRevert
		{
			get
			{
				SapienceTracker tracker = animal.GetSapienceTracker();
				if (tracker == null) return false;
				return tracker.CurrentState?.StateDef == mutagenDef?.transformedSapienceState && !tracker.IsPermanentlyFeral;
			}
		}


		/// <summary>Exposes the data.</summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref original, true, nameof(original));
			Scribe_References.Look(ref animal, nameof(animal));
			Scribe_References.Look(ref factionResponsible, nameof(factionResponsible));
			Scribe_Values.Look(ref reactionStatus, nameof(reactionStatus));
		}
	}


	/// <summary>
	///     TransformedPawn instance for merged pawns
	/// </summary>
	/// <seealso cref="Pawnmorph.TfSys.TransformedPawn" />
	public class MergedPawns : TransformedPawn
	{
		/// <summary>
		///     The faction responsible for turning this pawn into an animal
		/// </summary>
		public Faction factionResponsible;

		/// <summary>The original pawns</summary>
		public List<Pawn> originals;

		/// <summary>The resultant meld</summary>
		public Pawn meld;

		/// <summary>
		///     Initializes a new instance of the <see cref="MergedPawns" /> class.
		/// </summary>
		public MergedPawns()
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="MergedPawns" /> class.
		/// </summary>
		/// <param name="transformedTick">timestamp the pawns were transformed.</param>
		public MergedPawns(int? transformedTick) : base(transformedTick)
		{
		}

		/// <summary>
		///     Gets the faction that turned this pawn into an animal.
		/// </summary>
		/// <value>
		///     The faction responsible.
		/// </value>
		public override Faction FactionResponsible => factionResponsible;

		/// <summary>Gets the original pawns.</summary>
		/// <value>The original pawns.</value>
		public override IEnumerable<Pawn> OriginalPawns => originals ?? Enumerable.Empty<Pawn>();

		/// <summary>Gets the transformed pawns.</summary>
		/// <value>The transformed pawns.</value>
		public override IEnumerable<Pawn> TransformedPawns
		{
			get { yield return meld; }
		}

		/// <summary>
		///     Gets a value indicating whether this instance can be reverted.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can be reverted; otherwise, <c>false</c>.
		/// </value>
		public override bool CanRevert
		{
			get
			{
				SapienceTracker sTracker = meld?.GetSapienceTracker();
				if (sTracker == null) return false;
				return meld.GetSapienceState()?.StateDef == mutagenDef?.transformedSapienceState && !sTracker.IsPermanentlyFeral;
			}
		}


		/// <summary>Exposes the data.</summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref originals, nameof(originals), true, LookMode.Deep);
			Scribe_References.Look(ref meld, nameof(meld), true);
			Scribe_References.Look(ref factionResponsible, nameof(factionResponsible));
		}
	}
}