// DebugMenu_AddMutations.cs created by Iron Wolf for Pawnmorph on 05/17/2020 5:31 PM
// last updated 05/17/2020  5:31 PM

using System.Linq;
using JetBrains.Annotations;
using LudeonTK;
using Pawnmorph.Hediffs;
using UnityEngine;
using Verse;

namespace Pawnmorph.DebugUtils
{
	internal class DebugMenu_AddMutations : Dialog_DebugOptionLister
	{
		public DebugMenu_AddMutations([NotNull] Pawn pawn)
		{
			_pawn = pawn;
		}

		[NotNull] private readonly Pawn _pawn;

		void AddMutationAction([NotNull] MutationDef mutationDef)
		{
			Find.WindowStack.Add(new DebugMenu_AddMutation(mutationDef, _pawn));
		}

		protected override void DoListingItems(Rect inRect, float columnWidth)
		{
			var grouping = MutationDef.AllMutations.SelectMany(x => x.ClassInfluences.Select(y => (x, y))).GroupBy(m => m.y, m => m.x);

			foreach (IGrouping<AnimalClassBase, MutationDef> group in grouping)
			{
				var label = group.Key.defName;
				DebugLabel(label, columnWidth);
				foreach (MutationDef mutationDef in group)
				{
					var mDef = mutationDef;
					DebugAction(mDef.defName, columnWidth, () => AddMutationAction(mDef), false);
				}
			}
		}
	}
}