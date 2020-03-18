// LordJob_FormAndSendSapientAnimalCaravan.cs created by Iron Wolf for Pawnmorph on 03/18/2020 12:37 PM
// last updated 03/18/2020  12:37 PM

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace Pawnmorph.Caravans
{
    /// <summary>
    /// lord job for caravans with sapient animals in them 
    /// </summary>
    /// <seealso cref="Verse.AI.Group.LordJob" />
    public class LordJob_FormAndSendSapientAnimalCaravan : LordJob_FormAndSendCaravan
    {
     
        


        private IntVec3 meetingPoint;

        private IntVec3 exitSpot;

        private int startingTile;

        private int destinationTile;

        private bool caravanSent;

        private LordToil gatherAnimals;

        private LordToil gatherAnimals_pause;

        private LordToil gatherItems;

        private LordToil gatherItems_pause;

        private LordToil gatherSlaves;

        private LordToil gatherSlaves_pause;

        private LordToil gatherDownedPawns;

        private LordToil gatherDownedPawns_pause;

        private LordToil leave;

        private LordToil leave_pause;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LordJob_FormAndSendSapientAnimalCaravan" /> class.
        /// </summary>
        public LordJob_FormAndSendSapientAnimalCaravan()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LordJob_FormAndSendSapientAnimalCaravan" /> class.
        /// </summary>
        /// <param name="transferables">The transferables.</param>
        /// <param name="downedPawns">The downed pawns.</param>
        /// <param name="meetingPoint">The meeting point.</param>
        /// <param name="exitSpot">The exit spot.</param>
        /// <param name="startingTile">The starting tile.</param>
        /// <param name="destinationTile">The destination tile.</param>
        public LordJob_FormAndSendSapientAnimalCaravan(List<TransferableOneWay> transferables, List<Pawn> downedPawns,
                                                       IntVec3 meetingPoint,
                                                       IntVec3 exitSpot, int startingTile, int destinationTile)
        {
            this.transferables = transferables;
            this.downedPawns = downedPawns;
            this.meetingPoint = meetingPoint;
            this.exitSpot = exitSpot;
            this.startingTile = startingTile;
            this.destinationTile = destinationTile;
        }

        /// <summary>
        ///     Gets a value indicating whether this job is gathering items.
        /// </summary>
        /// <value>
        ///     <c>true</c> if is gathering items; otherwise, <c>false</c>.
        /// </value>
        public bool GatheringItemsNow => lord.CurLordToil == gatherItems;

        /// <summary>
        ///     Gets a value indicating whether this job is allowed to start new gatherings .
        /// </summary>
        /// <value>
        ///     <c>true</c> if is allowed to start new gatherings; otherwise, <c>false</c>.
        /// </value>
        public override bool AllowStartNewGatherings => false;

        /// <summary>
        ///     Gets a value indicating whether [never in restraints].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [never in restraints]; otherwise, <c>false</c>.
        /// </value>
        public override bool NeverInRestraints => true;

        /// <summary>
        ///     Gets a value indicating whether a flee toil should be added.
        /// </summary>
        /// <value>
        ///     <c>true</c> if a flee toil should be added; otherwise, <c>false</c>.
        /// </value>
        public override bool AddFleeToil => false;

        /// <summary>
        ///     Gets the status.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        public string Status
        {
            get
            {
                LordToil curLordToil = lord.CurLordToil;
                if (curLordToil == gatherAnimals) return "FormingCaravanStatus_GatheringAnimals".Translate();
                if (curLordToil == gatherAnimals_pause) return "FormingCaravanStatus_GatheringAnimals_Pause".Translate();
                if (curLordToil == gatherItems) return "FormingCaravanStatus_GatheringItems".Translate();
                if (curLordToil == gatherItems_pause) return "FormingCaravanStatus_GatheringItems_Pause".Translate();
                if (curLordToil == gatherSlaves) return "FormingCaravanStatus_GatheringSlaves".Translate();
                if (curLordToil == gatherSlaves_pause) return "FormingCaravanStatus_GatheringSlaves_Pause".Translate();
                if (curLordToil == gatherDownedPawns) return "FormingCaravanStatus_GatheringDownedPawns".Translate();
                if (curLordToil == gatherDownedPawns_pause) return "FormingCaravanStatus_GatheringDownedPawns_Pause".Translate();
                if (curLordToil == leave) return "FormingCaravanStatus_Leaving".Translate();
                if (curLordToil == leave_pause) return "FormingCaravanStatus_Leaving_Pause".Translate();
                return "FormingCaravanStatus_Waiting".Translate();
            }
        }

        /// <summary>
        ///     Determines whether the given pawn can can open any doors
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns>
        ///     <c>true</c> if this instance with the given pawn can can open any doors  otherwise, <c>false</c>.
        /// </returns>
        public override bool CanOpenAnyDoor(Pawn p)
        {
            return true;
        }

        /// <summary>
        ///     Creates the graph.
        /// </summary>
        /// <returns></returns>
        public override StateGraph CreateGraph()
        {
            var stateGraph = new StateGraph();
            gatherAnimals = new LordToil_PrepareCaravanSA_GatherAnimals(meetingPoint);
            stateGraph.AddToil(gatherAnimals);
            gatherAnimals_pause = new LordToil_PrepareCaravan_Pause();
            stateGraph.AddToil(gatherAnimals_pause);
            gatherItems = new LordToil_PrepareCaravan_GatherItems(meetingPoint);
            stateGraph.AddToil(gatherItems);
            gatherItems_pause = new LordToil_PrepareCaravan_Pause();
            stateGraph.AddToil(gatherItems_pause);
            gatherSlaves = new LordToil_PrepareCaravan_GatherSlaves(meetingPoint);
            stateGraph.AddToil(gatherSlaves);
            gatherSlaves_pause = new LordToil_PrepareCaravan_Pause();
            stateGraph.AddToil(gatherSlaves_pause);
            gatherDownedPawns = new LordToil_PrepareCaravan_GatherDownedPawns(meetingPoint, exitSpot);
            stateGraph.AddToil(gatherDownedPawns);
            gatherDownedPawns_pause = new LordToil_PrepareCaravan_Pause();
            stateGraph.AddToil(gatherDownedPawns_pause);
            var lordToil_PrepareCaravan_Wait = new LordToil_PrepareCaravan_Wait(meetingPoint);
            stateGraph.AddToil(lordToil_PrepareCaravan_Wait);
            var lordToil_PrepareCaravan_Pause = new LordToil_PrepareCaravan_Pause();
            stateGraph.AddToil(lordToil_PrepareCaravan_Pause);
            leave = new LordToil_PrepareCaravan_Leave(exitSpot);
            stateGraph.AddToil(leave);
            leave_pause = new LordToil_PrepareCaravan_Pause();
            stateGraph.AddToil(leave_pause);
            var lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            var transition = new Transition(gatherAnimals, gatherItems); 
            transition.AddTrigger(new Trigger_Memo("AllAnimalsGathered"));
            stateGraph.AddTransition(transition);
            var transition2 = new Transition(gatherItems, gatherSlaves);
            transition2.AddTrigger(new Trigger_Memo("AllItemsGathered"));
            transition2.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition2);
            var transition3 = new Transition(gatherSlaves, gatherDownedPawns);
            transition3.AddTrigger(new Trigger_Memo("AllSlavesGathered"));
            transition3.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition3);
            var transition4 = new Transition(gatherDownedPawns, lordToil_PrepareCaravan_Wait);
            transition4.AddTrigger(new Trigger_Memo("AllDownedPawnsGathered"));
            transition4.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition4);
            var transition5 = new Transition(lordToil_PrepareCaravan_Wait, leave);
            transition5.AddTrigger(new Trigger_NoPawnsVeryTiredAndSleeping());
            transition5.AddPostAction(new TransitionAction_WakeAll());
            stateGraph.AddTransition(transition5);
            var transition6 = new Transition(leave, lordToil_End);
            transition6.AddTrigger(new Trigger_Memo("ReadyToExitMap"));
            transition6.AddPreAction(new TransitionAction_Custom(SendCaravan));
            stateGraph.AddTransition(transition6);
            Transition transition7 = PauseTransition(gatherAnimals, gatherAnimals_pause);
            stateGraph.AddTransition(transition7);
            Transition transition8 = UnpauseTransition(gatherAnimals_pause, gatherAnimals);
            stateGraph.AddTransition(transition8);
            Transition transition9 = PauseTransition(gatherItems, gatherItems_pause);
            stateGraph.AddTransition(transition9);
            Transition transition10 = UnpauseTransition(gatherItems_pause, gatherItems);
            stateGraph.AddTransition(transition10);
            Transition transition11 = PauseTransition(gatherSlaves, gatherSlaves_pause);
            stateGraph.AddTransition(transition11);
            Transition transition12 = UnpauseTransition(gatherSlaves_pause, gatherSlaves);
            stateGraph.AddTransition(transition12);
            Transition transition13 = PauseTransition(gatherDownedPawns, gatherDownedPawns_pause);
            stateGraph.AddTransition(transition13);
            Transition transition14 = UnpauseTransition(gatherDownedPawns_pause, gatherDownedPawns);
            stateGraph.AddTransition(transition14);
            Transition transition15 = PauseTransition(leave, leave_pause);
            stateGraph.AddTransition(transition15);
            Transition transition16 = UnpauseTransition(leave_pause, leave);
            stateGraph.AddTransition(transition16);
            Transition transition17 = PauseTransition(lordToil_PrepareCaravan_Wait, lordToil_PrepareCaravan_Pause);
            stateGraph.AddTransition(transition17);
            Transition transition18 = UnpauseTransition(lordToil_PrepareCaravan_Pause, lordToil_PrepareCaravan_Wait);
            stateGraph.AddTransition(transition18);
            return stateGraph;
        }

        /// <summary>
        ///     Exposes the data for this job
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref transferables, "transferables", LookMode.Deep);
            Scribe_Collections.Look(ref downedPawns, "downedPawns", LookMode.Reference);
            Scribe_Values.Look(ref meetingPoint, "meetingPoint");
            Scribe_Values.Look(ref exitSpot, "exitSpot");
            Scribe_Values.Look(ref startingTile, "startingTile");
            Scribe_Values.Look(ref destinationTile, "destinationTile");
            if (Scribe.mode == LoadSaveMode.PostLoadInit) downedPawns.RemoveAll(x => x.DestroyedOrNull());
        }

        /// <summary>
        ///     Gets the report.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        public override string GetReport(Pawn pawn)
        {
            return "LordReportFormingCaravan".Translate();
        }
        /// <summary>
        /// called every tick
        /// </summary>
        public override void LordJobTick()
        {
            base.LordJobTick();
            for (int num = downedPawns.Count - 1; num >= 0; num--)
                if (downedPawns[num].Destroyed)
                {
                    downedPawns.RemoveAt(num);
                }
                else if (!downedPawns[num].Downed)
                {
                    lord.AddPawn(downedPawns[num]);
                    downedPawns.RemoveAt(num);
                }
        }

        /// <summary>
        /// Notifies this lord that a pawn has been added
        /// </summary>
        /// <param name="p">The p.</param>
        public override void Notify_PawnAdded(Pawn p)
        {
            base.Notify_PawnAdded(p);
            ReachabilityUtility.ClearCacheFor(p);
        }

        /// <summary>
        /// Notifies the pawn lost.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="condition">The condition.</param>
        public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
        {
            base.Notify_PawnLost(p, condition);
            ReachabilityUtility.ClearCacheFor(p);
            if (!caravanSent)
            {
                if (condition == PawnLostCondition.IncappedOrKilled && p.Downed) downedPawns.Add(p);
                CaravanFormingUtility.RemovePawnFromCaravan(p, lord, false);
            }
        }

        private Transition PauseTransition(LordToil from, LordToil to)
        {
            var transition = new Transition(from, to);
            transition.AddPreAction(new TransitionAction_Message("MessageCaravanFormationPaused".Translate(),
                                                                 MessageTypeDefOf.NegativeEvent,
                                                                 () =>
                                                                     lord.ownedPawns.FirstOrDefault(x => x
                                                                                                       .InMentalState)));
            transition.AddTrigger(new Trigger_MentalState());
            transition.AddPostAction(new TransitionAction_EndAllJobs());
            return transition;
        }

        private void SendCaravan()
        {
            caravanSent = true;
            CaravanFormingUtility
               .FormAndCreateCaravan(lord.ownedPawns.Concat(downedPawns.Where(x => JobGiver_PrepareCaravan_GatherDownedPawns.IsDownedPawnNearExitPoint(x, exitSpot))),
                                     lord.faction, Map.Tile, startingTile, destinationTile);
        }

        private Transition UnpauseTransition(LordToil from, LordToil to)
        {
            var transition = new Transition(from, to);
            transition.AddPreAction(new TransitionAction_Message("MessageCaravanFormationUnpaused".Translate(),
                                                                 MessageTypeDefOf.SilentInput));
            transition.AddTrigger(new Trigger_NoMentalState());
            transition.AddPostAction(new TransitionAction_EndAllJobs());
            return transition;
        }
    }
}