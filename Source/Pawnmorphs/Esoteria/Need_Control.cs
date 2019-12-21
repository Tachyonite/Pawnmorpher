// Need_Control.cs modified by Iron Wolf for Pawnmorph on 12/07/2019 1:48 PM
// last updated 12/07/2019  1:49 PM

using System;
using System.Collections.Generic;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using static Pawnmorph.InstinctUtilities;

namespace Pawnmorph
{
    /// <summary>
    ///     need that represents a sapient animal's control or humanity left
    /// </summary>
    public class Need_Control : Need_Seeker
    {
        private float _seekerLevel;

        private SapienceLevel _currentLevel;

        private float? _maxLevelCached = null;


        /// <summary>
        ///     Initializes a new instance of the <see cref="Need_Control" /> class.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        public Need_Control(Pawn pawn) : base(pawn)
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

        /// <summary>
        ///     Gets the current instant level.
        /// </summary>
        /// <value>
        ///     The current instant level.
        /// </value>
        public override float CurInstantLevel => _seekerLevel;

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
        ///     Draws the GUI.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="maxThresholdMarkers">The maximum threshold markers.</param>
        /// <param name="customMargin">The custom margin.</param>
        /// <param name="drawArrows">if set to <c>true</c> [draw arrows].</param>
        /// <param name="doTooltip">if set to <c>true</c> [do tooltip].</param>
        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1,
                                       bool drawArrows = true,
                                       bool doTooltip = true)
        {
            
            if (threshPercents == null || _maxLevelCached == null)
            {
                _maxLevelCached = _maxLevelCached ?? MaxLevel;
                float mLevel = _maxLevelCached.Value;
                threshPercents = threshPercents ?? new List<float>();
                foreach (VTuple<SapienceLevel, float> sapienceLevelThreshold in FormerHumanUtilities.SapienceLevelThresholds)
                {
                    float thresh = sapienceLevelThreshold.second / mLevel;
                    if (thresh > 1) continue;
                    threshPercents.Add(thresh);
                }
            }
            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
        }

        /// <summary>
        ///     Exposes the data.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _seekerLevel, nameof(_seekerLevel), -1);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (_seekerLevel < 0)
                    _seekerLevel = CurLevel;
                _currentLevel = FormerHumanUtilities.GetQuantizedSapienceLevel(_seekerLevel);
            }
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
                float instinctChange = GetInstinctChangePerTick(pawn) * TimeMetrics.TICKS_PER_REAL_SECOND;
                if (Mathf.Abs(instinctChange) > EPSILON) AddInstinctChange(Mathf.CeilToInt(instinctChange));

                SapienceLevel sLevel = FormerHumanUtilities.GetQuantizedSapienceLevel(CurLevel);
                if (sLevel != _currentLevel)
                {
                    _currentLevel = sLevel;
                    OnSapienceLevelChanges();
                }

                if (_currentLevel == SapienceLevel.Feral && Rand.MTBEventOccurs(4, 60000f, 30f)) TriggerPermanentlyFeralChange();
            }
        }

        /// <summary>
        ///     Notifies that the cached maximum level is dirty
        /// </summary>
        public void NotifyMaxLevelDirty()
        {
            _maxLevelCached = null;
        }


        /// <summary>
        ///     Sets the initial level.
        /// </summary>
        public override void SetInitialLevel()
        {
            CurLevelPercentage = 1;
            _seekerLevel = MaxLevel;
        }


        private void OnSapienceLevelChanges()
        {
            Hediff fHediff = pawn.health.hediffSet.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman);
            if (fHediff == null) return;

            switch (_currentLevel)
            {
                case SapienceLevel.Sapient:
                case SapienceLevel.MostlySapient:
                case SapienceLevel.Conflicted:
                case SapienceLevel.MostlyFeral:
                    fHediff.Severity = 1;
                    break;
                case SapienceLevel.Feral:
                    fHediff.Severity = 0.5f;
                    break;
                case SapienceLevel.PermanentlyFeral:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            pawn.needs?.AddOrRemoveNeedsAsAppropriate();
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
        }

        private void TriggerPermanentlyFeralChange()
        {
            FormerHumanUtilities.MakePermanentlyFeral(pawn);
        }

        /// <summary>
        /// Gets the seeker level.
        /// </summary>
        /// <value>
        /// The seeker level.
        /// </value>
        public float SeekerLevel => _seekerLevel; 

        /// <summary>
        /// Sets the initial level.
        /// </summary>
        /// <param name="sapiencePercent">The sapience level.</param>
        public void SetInitialLevel(float sapiencePercent)
        {
            _seekerLevel = Mathf.Clamp(sapiencePercent, 0, 1) * MaxLevel;
            CurLevel = _seekerLevel; 
        }
    }
}