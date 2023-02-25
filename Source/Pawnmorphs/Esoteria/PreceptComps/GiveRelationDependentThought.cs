using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph.PreceptComps
{
	/// <summary>
	/// precept comp for giving relationship dependent thoughts to pawns 
	/// </summary>
	/// <seealso cref="RimWorld.PreceptComp_Thought" />
	public class GiveRelationDependentThought : PreceptComp_Thought
	{
		/// <summary>
		/// The event definition this comp looks for 
		/// </summary>
		public HistoryEventDef eventDef;

		/// <summary>
		/// Gets the traits affecting this precept comp
		/// </summary>
		/// <value>
		/// The traits affecting.
		/// </value>
		public override IEnumerable<TraitRequirement> TraitsAffecting => ThoughtUtility.GetNullifyingTraits(thought);


		/// <summary>
		/// Notifies the member witnessed action.
		/// </summary>
		/// <param name="ev">The ev.</param>
		/// <param name="precept">The precept.</param>
		/// <param name="member">The member.</param>
		public override void Notify_MemberWitnessedAction(HistoryEvent ev, Precept precept, Pawn member)
		{


			if (ev.def != eventDef)
				return;

			Pawn victimPawn;
			bool flag = ev.args.TryGetArg(HistoryEventArgsNames.Doer, out victimPawn);
			if (!flag) //In case something goes wrong. But it should not.
				return;

			if (victimPawn == member)
				return;

			var relation = victimPawn.GetRelation(member.Faction);

			if (relation == ColonyRelation.PrisonerGuilty && thought.stages.Count <= (int)relation)
				relation = ColonyRelation.Prisoner; //make the prisoner guilty variation optional 

			var stage = (int)relation;

			Thought_Memory thought_Memory = ThoughtMaker.MakeThought(thought, precept);
			thought_Memory.SetForcedStage(Math.Min(stage, thought.stages.Count - 1));
			member.TryGainMemory(thought_Memory);
		}
	}
}
