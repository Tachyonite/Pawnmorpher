using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Hybrids;
using Verse;

namespace Pawnmorph.Social
{
	/// <summary>
	/// A def that describes interaction weights for pawnmorpher-specific interactions.
	/// This allows interactions to be weighted based on the mutations of the initatior
	/// and/or recipient of the interaction.  Interactions can also require specific
	/// mutations and/or morphs before they will trigger at all.
	/// </summary>
	public class PMInteractionWeightsDef : Def
	{
		/// <summary>If specified, at least one of these mutations is required for this interaction to trigger.
		/// Does not affect the weight of the interaction.</summary>
		public List<HediffDef> requiredMutationsAny;
		/// <summary>If specified, all of these mutations are required before this interaction triggers.
		/// Does not affect the weight of the interaction.</summary>
		public List<HediffDef> requiredMutationsAll;
		/// <summary>If specified, only a pawn that's one of these kinds of morph can trigger this interaction.
		/// Does not affect the weight of the interaction.</summary>
		public List<MorphDef> restrictedToMorphs;

		/// <summary>Weight added to the interaction for each instance of a given mutation</summary>
		public Dictionary<HediffDef, float> mutationWeights = new Dictionary<HediffDef, float>();
		/// <summary>Weight added to the interaction for being a specific kind of morphs</summary>
		public Dictionary<MorphDef, float> morphWeights = new Dictionary<MorphDef, float>();


		private HashSet<HediffDef> _hediffs = new HashSet<HediffDef>(20);


		/// <summary>
		/// Gets the total interaction weight for the given pawn based on this def.
		/// The higher the weight the more likely this interaction is going to be picked.
		/// The weight is relative to the vanilla version of the interaction.  For example,
		/// a weight of 2 means the interaction will happen twice as often as the vanilla version.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		public float GetTotalWeight(Pawn pawn)
		{
			_hediffs.Clear();

			for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
			{
				_hediffs.Add(pawn.health.hediffSet.hediffs[i].def);
			}
			
			if (requiredMutationsAny != null && !requiredMutationsAny.Any(_hediffs.Contains)) 
				return 0;

			if (requiredMutationsAll != null && !requiredMutationsAll.All(_hediffs.Contains)) 
				return 0;


			MorphDef morphDef = pawn.def.GetMorphOfRace();
			if (restrictedToMorphs != null && !restrictedToMorphs.Contains(morphDef)) 
				return 0;

			float weightFromMutations = _hediffs.Sum(m => mutationWeights.TryGetValue(m, 0));
			float weightFromMorph = (morphDef != null) ? morphWeights.TryGetValue(morphDef, 0) : 0;

			return weightFromMutations + weightFromMorph;
		}
	}
}
