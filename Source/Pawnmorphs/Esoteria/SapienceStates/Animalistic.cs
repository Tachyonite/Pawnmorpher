// Animalistic.cs created by Iron Wolf for Pawnmorph on 04/25/2020 2:17 PM
// last updated 04/25/2020  2:17 PM

using System;
using RimWorld;
using Verse;

namespace Pawnmorph.SapienceStates
{
    /// <summary>
    /// sapience state for 'animalistic' humanoids 
    /// </summary>
    /// <seealso cref="Pawnmorph.SapienceState" />
    public class Animalistic : SapienceState
    {
        /// <summary>
        /// Gets a value indicating whether this state makes the pawn count as a 'former human'.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this state makes the pawn count as a 'former human'; otherwise, <c>false</c>.
        /// </value>
        public override bool IsFormerHuman => false; 

        /// <summary>
        ///     Gets the current intelligence.
        /// </summary>
        /// <value>
        ///     The current intelligence.
        /// </value>
        public override Intelligence CurrentIntelligence
        {
            get
            {
                switch (CurrentSapience)
                {
                    case SapienceLevel.Sapient:
                    case SapienceLevel.MostlySapient:
                    case SapienceLevel.Conflicted:
                        return Intelligence.Humanlike;
                    case SapienceLevel.MostlyFeral:
                    case SapienceLevel.Feral:
                    case SapienceLevel.PermanentlyFeral:
                        return Intelligence.Animal;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///     called after every tick
        /// </summary>
        public override void Tick()
        {
        }

        /// <summary>
        ///     called to save/load all data.
        /// </summary>
        protected override void ExposeData()
        {
            
        }

        /// <summary>
        /// Adds the or remove dynamic components.
        /// </summary>
        public override void AddOrRemoveDynamicComponents()
        {
            switch (CurrentSapience)
            {
                case SapienceLevel.Sapient:
                case SapienceLevel.MostlySapient:
                case SapienceLevel.Conflicted:
                    InitHumanlikeComps();
                    break;
                case SapienceLevel.MostlyFeral:
                    InitMostlyFeralComps();
                    break;
                case SapienceLevel.Feral:
                case SapienceLevel.PermanentlyFeral:
                    SetupFeralComps(); 
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InitMostlyFeralComps()
        {
            AddMostlyFeralComps();
            AddHumanlikeComps();
        }

        private void InitHumanlikeComps()
        {
            RemoveAnimalComps();
            AddHumanlikeComps();            
        }

        void AddHumanlikeComps()
        {
            Pawn.drafter = Pawn.drafter ?? new Pawn_DraftController(Pawn);

            Pawn.equipment = Pawn.equipment ?? new Pawn_EquipmentTracker(Pawn);
            Pawn.apparel = Pawn.apparel ?? new Pawn_ApparelTracker(Pawn);
            Pawn.workSettings = Pawn.workSettings ?? new Pawn_WorkSettings(Pawn); 
        }

        private void SetupFeralComps()
        {
            AddMostlyFeralComps();
            if (Pawn.drafter != null)
            {
                Pawn.drafter.Drafted = false;
                Pawn.drafter = null; 
            }

            Pawn.workSettings = null;
            IntVec3 pawnPosition = Pawn.Position;
            if (Pawn.Map != null)
            {
                
                Pawn.apparel?.DropAll(pawnPosition, Pawn.Faction != Faction.OfPlayer); 
                Pawn.equipment?.DropAllEquipment(pawnPosition, Pawn.Faction == Faction.OfPlayer);
            }
            else
            {
                Pawn.apparel?.DestroyAll();
                Pawn.equipment?.DestroyAllEquipment(); 
            }

            Pawn.equipment = null; 
            Pawn.apparel = null;
        }

        private void AddMostlyFeralComps()
        {
            Pawn.training = Pawn.training ?? new Pawn_TrainingTracker(Pawn);
        }

        private void RemoveAnimalComps()
        {
            Pawn.training = null;

        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        /// this is always called before enter and after loading a pawn
        protected override void Init()
        {
            
        }
    }
}