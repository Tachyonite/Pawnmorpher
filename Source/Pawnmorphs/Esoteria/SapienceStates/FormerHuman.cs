// FormerHuman.cs created by Iron Wolf for Pawnmorph on 04/25/2020 9:33 AM
// last updated 04/25/2020  9:34 AM

using System;
using System.Linq;
using Pawnmorph.TfSys;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.SapienceStates
{
    /// <summary>
    /// sapience state for former humans 
    /// </summary>
    /// <seealso cref="Pawnmorph.SapienceState" />
    public class FormerHuman : SapienceState
    {
        private const float PERMANENTLY_FERAL_MTB = 3; 



        /// <summary>
        /// called after every tick 
        /// </summary>
        public override void Tick()
        {
            if (_countdownStarted && Pawn.IsHashIntervalTick(60)  && RandUtilities.MtbDaysEventOccured(PERMANENTLY_FERAL_MTB))
            {
                _countdownStarted = false; 
                MakePermanentlyFeral();
            }
        }

        internal void MakePermanentlyFeral()
        {
            Hediff fHediff;

            if (StateDef.forcedHediff != null)
            {
                fHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(StateDef.forcedHediff); 
            }
            else
            {
                fHediff = null; 
            }

            //transfer relationships back if possible 
            var gComp = Find.World.GetComponent<PawnmorphGameComp>();
            Pawn oPawn = gComp.GetTransformedPawnContaining(Pawn)?.Item1?.OriginalPawns.FirstOrDefault();
            if (oPawn == Pawn) oPawn = null;

            Pawn_RelationsTracker aRelations = Pawn.relations;
            if (aRelations != null && oPawn != null) FormerHumanUtilities.TransferRelationsToOriginal(oPawn, Pawn);

            Pawn.health.AddHediff(TfHediffDefOf.PermanentlyFeral);
            if(fHediff != null)
                Pawn.health.RemoveHediff(fHediff);

            var loader = Find.World.GetComponent<PawnmorphGameComp>();
            TransformedPawn inst = loader.GetTransformedPawnContaining(Pawn)?.Item1;
            var singleInst = inst as TransformedPawnSingle; //hacky, need to come up with a better solution 
            foreach (Pawn instOriginalPawn in inst?.OriginalPawns ?? Enumerable.Empty<Pawn>()
            ) //needed to handle merges correctly 
                ReactionsHelper.OnPawnPermFeral(instOriginalPawn, Pawn,
                                                singleInst?.reactionStatus ?? FormerHumanReactionStatus.Wild);

            //remove the original and destroy the pawns 
            foreach (Pawn instOriginalPawn in inst?.OriginalPawns ?? Enumerable.Empty<Pawn>()) instOriginalPawn.Destroy();

            if (inst != null) loader.RemoveInstance(inst);

            if (inst != null || Pawn.Faction == Faction.OfPlayer)
                Find.LetterStack.ReceiveLetter("LetterHediffFromPermanentTFLabel".Translate(Pawn.LabelShort).CapitalizeFirst(),
                                               "LetterHediffFromPermanentTF".Translate(Pawn.LabelShort).CapitalizeFirst(),
                                               LetterDefOf.NegativeEvent, Pawn);

            Pawn.needs?.AddOrRemoveNeedsAsAppropriate(); //make sure any comps get added/removed as appropriate 
            PawnComponentsUtility.AddAndRemoveDynamicComponents(Pawn);
        }

        /// <summary>
        ///     Gets the current intelligence.
        /// </summary>
        /// <value>
        ///     The current intelligence.
        /// </value>
        public override Intelligence CurrentIntelligence {
            get
            {

                switch (Tracker.SapienceLevel)
                {
                    case SapienceLevel.Sapient:
                    case SapienceLevel.MostlySapient:
                        return Intelligence.Humanlike;
                        break;
                    case SapienceLevel.Conflicted:
                        return Intelligence.ToolUser; 
                    case SapienceLevel.MostlyFeral:
                    case SapienceLevel.Feral:
                    case SapienceLevel.PermanentlyFeral:
                        return Intelligence.Animal;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


            }}

        /// <summary>
        ///     called when a pawn enters this sapience state
        /// </summary>
        public override void Enter()
        {
            if (StateDef.forcedHediff != null)
                Pawn.health.AddHediff(StateDef.forcedHediff); 
        }

        /// <summary>
        /// called when the pawn exits this state 
        /// </summary>
        public override void Exit()
        {
            if (_subscribed)
            {
                var need = Tracker.SapienceNeed;
                if (need != null)
                {
                    need.SapienceLevelChanged -= OnSapienceLevelChanged; 
                }
            }
        }

        /// <summary>
        ///     called to save/load all data.
        /// </summary>
        protected override void ExposeData()
        {
            Scribe_Values.Look(ref _countdownStarted, "countdownStarted");
        }


        void SubscribeToEvents()
        {
            if (_subscribed) return; 
            var need = Tracker?.SapienceNeed;
            if (need == null) return;

            need.SapienceLevelChanged += OnSapienceLevelChanged;
            _subscribed = true; 
        }

        private bool _countdownStarted; 

        private void OnSapienceLevelChanged(Need_Control sender, Pawn pawn, SapienceLevel sapienceLevel)
        {
            if (sapienceLevel == SapienceLevel.Feral)
            {
                _countdownStarted = true; 
            }

            if(PawnUtility.ShouldSendNotificationAbout(pawn))
                SendFHLetter(pawn, sapienceLevel);
        }

        private static void SendFHLetter(Pawn pawn, SapienceLevel sapienceLevel)
        {
            // the translation keys should be $SapienceLevel_TransitionLabel and $SapienceLevel_TransitionContent
            string translationLabel = "FormerHuman" + sapienceLevel + "_Transition";
            string letterLabelKey = translationLabel + "Label";
            string letterContentKey = translationLabel + "Content";
            TaggedString letterContent, letterLabel;


            if (letterLabelKey.TryTranslate(out letterLabel) && letterContentKey.TryTranslate(out letterContent))
            {
                letterLabel = letterLabel.AdjustedFor(pawn);
                letterContent = letterContent.AdjustedFor(pawn);

                Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.NeutralEvent, new LookTargets(pawn));
            }
        }

        private bool _subscribed; 




        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        /// this is always called before enter and after loading a pawn
        protected override void Init()
        {
            SubscribeToEvents();
        }
    }
}