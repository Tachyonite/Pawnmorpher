// Comp_SapientAnimal.cs modified by Iron Wolf for Pawnmorph on 12/05/2019 7:04 PM
// last updated 12/05/2019  7:04 PM

using System;
using JetBrains.Annotations;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
    /// <summary>
    /// component for controlling instinct and mental breaks of sapient animals 
    /// </summary>
    /// <seealso cref="Verse.ThingComp" />
    public class Comp_SapientAnimal : ThingComp
    {
     
        private SapientAnimalMentalBreaker _mentalBreaker;

        /// <summary>
        /// Gets the mental breaker.
        /// </summary>
        /// <value>
        /// The mental breaker.
        /// </value>
        [NotNull]
        public SapientAnimalMentalBreaker MentalBreaker => _mentalBreaker; 

        [NotNull]
        Pawn Pawn
        {
            get
            {
                try
                {
                    var p = (Pawn) parent;
                    if (p == null) throw new ArgumentException(nameof(parent));
                    return p; 
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidCastException($"on {parent.Label}",e);
                }
            }
        }

        private int _instinctLevelRaw;


        /// <summary>
        /// call this to notify the comp that the attached pawn has recovered from the given mental state 
        /// </summary>
        /// <param name="state">The state.</param>
        public void Notify_RecoveredFromState([NotNull] MentalState state)
        {
            _mentalBreaker?.NotifyRecoveredFromMentalBreak();
        }


        /// <summary>
        /// called every tick 
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();
            _mentalBreaker?.Tick();
        }

        /// <summary>
        /// Initializes this comp
        /// </summary>
        /// <param name="props">The props.</param>
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            _mentalBreaker = _mentalBreaker ?? new SapientAnimalMentalBreaker(Pawn); 
        }

        /// <summary>
        /// called to expose this instances data.
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Deep.Look(ref _mentalBreaker, "mentalBreaker", Pawn); 
            
        }

        /// <summary>
        /// Gets or sets the 'instinct level'.
        /// </summary>
        /// <value>
        /// the instinct level 
        /// </value>
        public int InstinctLevel
        {
            get { return _instinctLevelRaw; }
            set { _instinctLevelRaw = Mathf.Max(value, 0); }
        }
    }
}