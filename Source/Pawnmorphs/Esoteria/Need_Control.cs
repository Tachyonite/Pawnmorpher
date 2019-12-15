// Need_Control.cs modified by Iron Wolf for Pawnmorph on 12/07/2019 1:48 PM
// last updated 12/07/2019  1:49 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using static Pawnmorph.InstinctUtilities;

namespace Pawnmorph
{
    /// <summary>
    ///     need that represents a sapient animal's control or humanity left
    /// </summary>
    public class Need_Control : Need_Seeker 
    {

        
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
            Mathf.Max(CalculateNetResistance(pawn) / AVERAGE_MAX_SAPIENCE, 0.01f); //this should never be zero 


        private float _seekerLevel;


        /// <summary>
        /// Gets the current instant level.
        /// </summary>
        /// <value>
        /// The current instant level.
        /// </value>
        public override float CurInstantLevel => _seekerLevel; 

        /// <summary>
        /// Exposes the data.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _seekerLevel, nameof(_seekerLevel), -1);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && _seekerLevel < 0)
                _seekerLevel = CurLevel; 

        }

        /// <summary>
        ///     Adds the instinct change to this need
        /// </summary>
        /// <param name="instinctChange">The instinct change.</param>
        public void AddInstinctChange(int instinctChange)
        {
            _seekerLevel += CalculateControlChange(pawn, instinctChange) / AVERAGE_MAX_SAPIENCE;
            _seekerLevel = Mathf.Clamp(_seekerLevel, 0, MaxLevel); 
        }

        /// <summary>
        ///     called every so often by the need manager.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void NeedInterval()
        {
            base.NeedInterval();
            if (pawn.IsHashIntervalTick(TimeMetrics.TICKS_PER_REAL_SECOND)) //just every second or so 
            {
                var instinctChange = GetInstinctChangePerTick(pawn) * TimeMetrics.TICKS_PER_REAL_SECOND;
                Log.Message($"{instinctChange} change to {pawn.Name}");
                if (Mathf.Abs(instinctChange) > EPSILON)
                {
                    AddInstinctChange(Mathf.CeilToInt(instinctChange)); 
                }
            }
        }

        /// <summary>
        /// Draws the GUI.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="maxThresholdMarkers">The maximum threshold markers.</param>
        /// <param name="customMargin">The custom margin.</param>
        /// <param name="drawArrows">if set to <c>true</c> [draw arrows].</param>
        /// <param name="doTooltip">if set to <c>true</c> [do tooltip].</param>
        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1, bool drawArrows = true,
                                       bool doTooltip = true)
        {
            threshPercents = threshPercents ?? new List<float>(); 
            foreach (VTuple<SapienceLevel, float> sapienceLevelThreshold in FormerHumanUtilities.SapienceLevelThresholds)
            {
                threshPercents.Add(sapienceLevelThreshold.second / MaxLevel); 
            }


            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
        }


        /// <summary>
        ///     Sets the initial level.
        /// </summary>
        public override void SetInitialLevel()
        {
            CurLevelPercentage = 1;
            _seekerLevel = MaxLevel; 
            Log.Message($"{pawn.Name} has need control level of {CurLevel}");
        }
    }
}