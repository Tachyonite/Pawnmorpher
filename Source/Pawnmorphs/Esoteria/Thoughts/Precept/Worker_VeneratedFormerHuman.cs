// Worker_VeneratedFormerHuman.cs created by Iron Wolf for Pawnmorph on 07/21/2021 12:26 PM
// last updated 07/21/2021  12:26 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts.Precept
{
	/// <summary>
	///     thought worker for former humans of venerated animals tied to a specific precept
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker_Precept" />
	public class Worker_VeneratedFormerHuman : ThoughtWorker_Precept
	{
		private readonly Dictionary<Ideo, bool> _cache = new Dictionary<Ideo, bool>();

		/// <summary>
		///     if this thought should be active
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			SapienceLevel? st = p.GetQuantizedSapienceLevel();
			if (st == null) return false;
			if (!def.IsValidFor(p)) return false;
			if (!p.IsFormerHuman()) return false;
			Ideo pIdeo = p.Ideo;
			if (pIdeo == null) return false;
			if (ContainsOverride(pIdeo)) return false;
			if (pIdeo.IsVeneratedAnimal(p) != true) return false;
			return ThoughtState.ActiveAtStage(Mathf.Min(def.stages.Count - 1, (int)st.Value));
		}

		private bool ContainsOverride([NotNull] Ideo ideo)
		{
			if (_cache.TryGetValue(ideo, out bool res)) return res;
			res = (ideo.PreceptsListForReading?.SelectMany(pre => pre.def.comps.MakeSafe())
					   .OfType<PreceptComp_SituationalThought>()).Any(pc => pc.thought.workerClass
																		 == typeof(Worker_VeneratedFormerHuman) && pc.thought != def);
			_cache[ideo] = res;
			return res;
		}
	}
}