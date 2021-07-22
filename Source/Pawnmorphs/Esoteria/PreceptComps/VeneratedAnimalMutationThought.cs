// VeneratedAnimalMutationThought.cs created by Iron Wolf for Pawnmorph on 07/22/2021 7:05 AM
// last updated 07/22/2021  7:05 AM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Thoughts.Precept;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.PreceptComps
{
    /// <summary>
    ///     precept comp for giving a thought based on a venerated animal mutation
    /// </summary>
    /// <seealso cref="RimWorld.PreceptComp" />
    public class VeneratedAnimalMutationThought : PreceptComp
    {
        /// <summary>
        ///     The thought definition to give
        /// </summary>
        public ThoughtDef thoughtDef;

        /// <summary>
        /// gets all configuration errors with this instance.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors(PreceptDef parent)
        {
            foreach (string configError in base.ConfigErrors(parent)) yield return configError;

            if (thoughtDef == null) yield return "no thought def set";
        }

        /// <summary>
        /// called when a pawn with an ideo with the given precept takes an action or has an action done to them 
        /// </summary>
        /// <param name="ev">The ev.</param>
        /// <param name="precept">The precept.</param>
        /// <param name="canApplySelfTookThoughts">if set to <c>true</c> [can apply self took thoughts].</param>
        public override void Notify_MemberTookAction(HistoryEvent ev, Precept precept, bool canApplySelfTookThoughts)
        {
            base.Notify_MemberTookAction(ev, precept, canApplySelfTookThoughts);
            if (!canApplySelfTookThoughts) return; 

            if (ev.def != PMHistoryEventDefOf.MutationGained && ev.def != PMHistoryEventDefOf.MutationLost) return;


            Pawn dooer = ev.GetDoer();
            var mut = ev.GetArg<Hediff_AddedMutation>(PMHistoryEventArgsNames.MUTATION);

            Ideo ideo = dooer.Ideo;
            if (ideo == null) return;
            ThingDef animal = null;
            foreach (ThingDef thingDef in ideo.VeneratedAnimals.MakeSafe())
                if (mut.Def.AssociatedAnimals.Contains(thingDef))
                    animal = thingDef;

            if (animal == null) return;

            MutationMemory_VeneratedAnimal thought = PMThoughtUtilities.CreateVeneratedAnimalMemory(thoughtDef, animal, precept);
            dooer.TryGainMemory(thought);
        }
    }
}