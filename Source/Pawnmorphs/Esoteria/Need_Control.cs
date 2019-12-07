// Need_Control.cs modified by Iron Wolf for Pawnmorph on 12/07/2019 1:48 PM
// last updated 12/07/2019  1:49 PM

using RimWorld;
using UnityEngine;
using Verse;
using static Pawnmorph.InstinctUtilities;

namespace Pawnmorph
{
    /// <summary>
    ///     need that represents a sapient animal's control or humanity left
    /// </summary>
    public class Need_Control : Need
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Need_Control"/> class.
        /// </summary>
        public Need_Control() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Need_Control"/> class.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        public Need_Control(Pawn pawn):base(pawn)
        {

        }

        /// <summary>
        ///     Gets the maximum level.
        /// </summary>
        /// <value>
        ///     The maximum level.
        /// </value>
        public override float MaxLevel =>
            Mathf.Max(CalculateNetResistance(pawn) / AVERAGE_RESISTANCE, 0.01f); //this should never be zero 


        /// <summary>
        ///     Adds the instinct change to this need
        /// </summary>
        /// <param name="instinctChange">The instinct change.</param>
        public void AddInstinctChange(int instinctChange)
        {
            CurLevel += CalculateControlChange(pawn, instinctChange) / AVERAGE_RESISTANCE;
        }

        /// <summary>
        ///     called every so often by the need manager.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void NeedInterval()
        {
            //empty 
        }

        /// <summary>
        ///     Sets the initial level.
        /// </summary>
        public override void SetInitialLevel()
        {
            CurLevelPercentage = 1;
        }
    }
}