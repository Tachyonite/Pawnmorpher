// Pawnmorpher_InteractionWeightLogDialogue.cs modified by Iron Wolf for Pawnmorph on 08/31/2019 2:08 PM
// last updated 08/31/2019  2:08 PM

using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using Pawnmorph.Social;
using RimWorld;
using UnityEngine;
using Verse;

#pragma warning disable 01591
namespace Pawnmorph.DebugUtils
{
	public class Pawnmorpher_InteractionWeightLogDialogue : Dialog_DebugOptionLister
	{
		private List<PMInteractionDef> _interactionDefs;
		private List<Pawn> _colonists;

		public Pawnmorpher_InteractionWeightLogDialogue()
		{
			_interactionDefs = DefDatabase<InteractionDef>.AllDefs.OfType<PMInteractionDef>().ToList();
			_colonists = PawnsFinder.AllMaps_FreeColonists.ToList();
		}



		void ListInteractionWeights(PMInteractionDef def)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine($"listings for {def.defName}");
			var worker = def.Worker;
			for (int i = 0; i < _colonists.Count; i++)
			{
				var colonistI = _colonists[i];
				builder.AppendLine($"weights for {colonistI.Name} as the initiator");
				for (int j = 0; j < _colonists.Count; j++)
				{
					if (i == j) continue;

					var colonistJ = _colonists[j];

					var weight = worker.RandomSelectionWeight(colonistI, colonistJ);
					builder.AppendLine($"\trecipient:{colonistJ.Name} weight:{weight}");



				}
			}

			Log.Message(builder.ToString());
		}

		/// <summary>
		/// Does the listing items.
		/// </summary>
		protected override void DoListingItems(Rect inRect, float columnWidth)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				foreach (PMInteractionDef pmInteractionDef in _interactionDefs)
				{
					DebugAction(pmInteractionDef.defName, columnWidth, () => ListInteractionWeights(pmInteractionDef), false);
				}
			}
		}
	}
}