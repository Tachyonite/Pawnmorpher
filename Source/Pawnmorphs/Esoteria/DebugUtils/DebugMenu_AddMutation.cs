// DebugMenue_AddMutation.cs created by Iron Wolf for Pawnmorph on 05/16/2020 8:59 AM
// last updated 05/16/2020  8:59 AM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LudeonTK;
using Pawnmorph.Hediffs;
using UnityEngine;
using Verse;

namespace Pawnmorph.DebugUtils
{
	internal class DebugMenu_AddMutation : Dialog_DebugOptionLister
	{
		[NotNull]
		private readonly MutationDef _mutationDef;
		[NotNull]
		private readonly Pawn _pawn;

		public DebugMenu_AddMutation([NotNull] MutationDef mDef, [NotNull] Pawn pawn)
		{
			_mutationDef = mDef ?? throw new ArgumentNullException(nameof(mDef));
			_pawn = pawn ?? throw new ArgumentNullException(nameof(pawn));
		}
		[NotNull]
		IEnumerable<(string label, Action action)> GenerateActions()
		{
			var health = _pawn.health;
			if (_mutationDef.parts == null)
			{
				yield return ("none", () => AddMutation(null));
				yield break;
			}

			yield return ("all", AddAllMutations);

			foreach (BodyPartRecord partR in _pawn.GetAllNonMissingParts())
			{
				if (_mutationDef.parts.Contains(partR.def) && !_pawn.health.hediffSet.HasHediff(_mutationDef, partR))
				{
					var pR = partR;
					yield return (partR.Label, () => AddMutation(pR));
				}
			}
		}

		void AddMutation([CanBeNull] BodyPartRecord record)
		{
			if (record == null)
			{
				var h = HediffMaker.MakeHediff(_mutationDef, _pawn);
				_pawn.health.AddHediff(h);
				return;
			}

			MutationUtilities.AddMutation(_pawn, _mutationDef, record, MutationUtilities.AncillaryMutationEffects.Default);
		}


		void AddAllMutations()
		{
			MutationUtilities.AddMutation(_pawn, _mutationDef);
		}

		protected override void DoListingItems(Rect inRect, float columnWidth)
		{
			foreach ((string label, Action action) in GenerateActions())
			{
				DebugAction(label, columnWidth, action, false);
			}
		}

		public override bool IsDebug => true;

	}
}