// Driver_RecruitSapientFormerHuman.cs created by Iron Wolf for Pawnmorph on 03/15/2020 3:45 PM
// last updated 03/15/2020  3:45 PM

using System;
using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
    /// <summary>
    /// job driver for recruiting a sapient former human
    /// </summary>
    /// <seealso cref="Pawnmorph.Jobs.Driver_InteractFormerHuman" />
    public class Driver_RecruitSapientFormerHuman : Driver_InteractFormerHuman
    {
        /// <summary>
        /// Gets a value indicating whether this instance can interact now.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can interact now; otherwise, <c>false</c>.
        /// </value>
        protected override bool CanInteractNow => !TameUtility.TriedToTameTooRecently(Animal);

        protected override Toil FinalInteractToil()
        {
            return TryRecruit(TargetIndex.A);
        }


        Toil TryRecruit(TargetIndex recruiteeInd)
        {
            Toil toil = new Toil();
            toil.initAction = (Action)(() =>
                                          {
                                              Pawn actor = toil.actor;
                                              Pawn thing = (Pawn)actor.jobs.curJob.GetTarget(recruiteeInd).Thing;
                                              if (!thing.Spawned || !thing.Awake())
                                                  return;
                                              InteractionDef intDef = PMInteractionDefOf.FormerHumanTameAttempt; 
                                              actor.interactions.TryInteractWith(thing, intDef);
                                          });
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 350;
            return toil;
        }

        /// <summary>
        /// Makes the new toils.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Toil> MakeNewToils()
        {
            foreach (Toil item in base.MakeNewToils().MakeSafe()) yield return item;
            this.FailOn(() => Map.designationManager.DesignationOn(Animal, PMDesignationDefOf.RecruitSapientFormerHuman) == null && !OnLastToil);
        }

        /// <summary>
        /// gets the Feeds toils.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Toil> FeedToils()
        {
            return Enumerable.Empty<Toil>();
        }
    }
}