// AspectGiver.cs modified by Iron Wolf for Pawnmorph on 12/16/2019 11:53 AM
// last updated 12/16/2019  11:53 AM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// base class for all 'aspect givers'
	/// </summary>
	public abstract class AspectGiver
	{
		/// <summary>
		/// Gets the aspects available to be given to pawns.
		/// </summary>
		/// <value>
		/// The available aspects.
		/// </value>
		[NotNull]
		public abstract IEnumerable<AspectDef> AvailableAspects { get; }

		/// <summary>
		/// Tries to give aspects to the given pawn 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="outList">if not null, all given aspects will be placed into the list</param>
		/// <returns>if any aspects were successfully given to the pawn</returns>
		public abstract bool TryGiveAspects([NotNull] Pawn pawn, [CanBeNull] List<Aspect> outList = null);



		/// <summary>
		/// Applies the aspect to the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="aspect">The aspect.</param>
		/// <param name="stageIndex">Index of the stage.</param>
		/// <param name="outLst">The out LST.</param>
		/// <returns>
		/// if the aspect was successfully added or not
		/// </returns>
		protected virtual bool ApplyAspect([NotNull] Pawn pawn, [NotNull] AspectDef aspect, int stageIndex, [CanBeNull] List<Aspect> outLst)
		{
			var aspectTracker = pawn.GetAspectTracker();
			if (aspectTracker == null) return false;
			if (HasConflictingAspect(aspectTracker, aspect)) return false;
			if (aspectTracker.Contains(aspect)) return false; //do not add the same aspect multiple times
			if (!aspect.IsValidFor(pawn)) return false; //check for any other def restrictions 
			if (pawn.story?.traits != null && !CheckPawnTraits(pawn.story.traits, aspect)) return false;  //check pawn traits 
			var aInst = aspect.CreateInstance();
			outLst?.Add(aInst);
			aspectTracker.Add(aInst, stageIndex);
			return true;
		}

		/// <summary>
		/// get all configuration errors with this instance 
		/// </summary>
		/// <returns></returns>
		[NotNull]
		public virtual IEnumerable<string> ConfigErrors()
		{
			yield break;
		}


		/// <summary>
		/// Determines whether the specified tracker has a conflicting aspect.
		/// </summary>
		/// <param name="tracker">The tracker.</param>
		/// <param name="testAspect">The test aspect.</param>
		/// <returns>
		///   <c>true</c> if the specified tracker has conflicting aspect ; otherwise, <c>false</c>.
		/// </returns>
		protected bool HasConflictingAspect([NotNull] AspectTracker tracker, [NotNull] AspectDef testAspect)
		{

			foreach (AspectDef conflict in testAspect.conflictingAspects)
			{
				if (tracker.Contains(conflict)) return true;
			}

			return false;
		}

		/// <summary>
		/// Checks if the pawn has any traits that block the given aspect
		/// </summary>
		/// <param name="traitSet">The trait set.</param>
		/// <param name="testAspect">The test aspect.</param>
		/// <returns>if given trait set is valid for the given aspect</returns>
		protected bool CheckPawnTraits([NotNull] TraitSet traitSet, [NotNull] AspectDef testAspect)
		{
			foreach (TraitDef traitDef in testAspect.requiredTraits.MakeSafe())
			{
				if (!traitSet.HasTrait(traitDef)) return false;
			}

			foreach (var traitDef in testAspect.conflictingTraits.MakeSafe())
			{
				if (traitSet.HasTrait(traitDef)) return false;
			}

			return true;
		}
	}
}