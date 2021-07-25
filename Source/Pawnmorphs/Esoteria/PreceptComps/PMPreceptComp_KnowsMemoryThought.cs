using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph.PreceptComps
{
    class PMPreceptComp_KnowsMemoryThought : PreceptComp_Thought
    {
        public HistoryEventDef eventDef;

        public override IEnumerable<TraitRequirement> TraitsAffecting => ThoughtUtility.GetNullifyingTraits(thought);

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

            int stage = 0;
            if (!member.IsColonist)
                stage = 0;
            else if (victimPawn.IsColonist)
                stage = 1;
            else if (victimPawn.IsPrisonerOfColony)
                stage = 2;
            else if (victimPawn.guest?.HostFaction == Faction.OfPlayer)
                stage = 3;

            Thought_Memory thought_Memory = ThoughtMaker.MakeThought(thought, precept);
            thought_Memory.SetForcedStage(Math.Min(stage, thought.stages.Count - 1));

        }
    }
}
