// Worker_HasAspect.cs modified by Iron Wolf for Pawnmorph on 09/28/2019 7:42 AM
// last updated 09/28/2019  7:42 AM

using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// aspect worker for a thought that appears when the pawn has a certain aspect 
	/// </summary>
	public class Worker_HasAspect : ThoughtWorker
	{
		/// <summary>Gets the definition.</summary>
		/// <value>The definition.</value>
		public Def_AspectThought Def
		{
			get
			{
				try
				{
					return (Def_AspectThought)def;
				}
				catch (InvalidCastException)
				{
					Log.Error($"unable to cast {def.GetType().Name} to {nameof(Def_AspectThought)}");
					throw;
				}
			}
		}

		/// <summary>gets the current thought state of the given pawn</summary>
		/// <param name="p">The pawn</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			var aspectTracker = p.GetAspectTracker();
			if (aspectTracker == null) return false;
			Aspect aspect = aspectTracker.GetAspect(Def.aspect);
			if (aspect == null) return false;
			var n = Mathf.Min(aspect.StageIndex, def.stages.Count - 1);
			return ThoughtState.ActiveAtStage(n);

		}
	}
}