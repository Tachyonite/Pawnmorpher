// Comp_MutationSeverityAdjust.cs modified by Iron Wolf for Pawnmorph on 12/01/2019 8:55 AM
// last updated 12/01/2019  8:55 AM

using System;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff comp that acts like severity per day but if affected by the 'Mutation Adaptability Stat'
    /// </summary>
    /// <seealso cref="Pawnmorph.Utilities.HediffCompBase{T}" />
    public class Comp_MutationSeverityAdjust : HediffComp_SeverityPerDay
    {
        private bool _halted; 


        [NotNull]
        CompProperties_MutationSeverityAdjust  Props
        {
            get
            {
                try
                {
                    var p =  (CompProperties_MutationSeverityAdjust) props;
                    if (p == null)
                        throw new InvalidOperationException($"props on {parent.def.defName} on {Pawn.Name} has not props");
                    return p; 
                }
                catch (InvalidCastException e) //just incase the properties get miss assigned somehow 
                {
                    throw new InvalidCastException($"could not convert hediff comp props of type {props.GetType().Name} to {nameof(CompProperties_MutationSeverityAdjust)}",e);
                }
            }
        }



        /// <summary>
        /// Gets the natural severity limit.
        /// </summary>
        /// this value is the value the attached hediff should reach if the pawn has had the mutation for an 'infinite' amount of time 
        /// <value>
        /// The natural severity limit.
        /// </value>
        public float NaturalSeverityLimit
        {
            get { return Props.GetNaturalSeverityLimitFor(Pawn); }
        }

        /// <summary>
        /// called to save/load data for this comp.
        /// </summary>
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref _halted, nameof(_halted));
            Scribe_Values.Look(ref _curStageIndex, nameof(_curStageIndex)); 
        }

        private int _curStageIndex=-1;
        /// <summary>
        /// called every tick 
        /// </summary>
        /// <param name="severityAdjustment">The severity adjustment.</param>
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            var sIndex = parent.CurStageIndex;
            if (sIndex != _curStageIndex)
            {
                _curStageIndex = sIndex; 
                CheckIfHalted(); 
            }
        }

        /// <summary>
        /// called when the parent is merged with a new hediff of the same type 
        /// </summary>
        /// <param name="other">The other.</param>
        public override void CompPostMerged(Hediff other)
        {
            base.CompPostMerged(other);
            _halted = false;  //restart adjusting if halted 
        }
        const float EPSILON = 0.001f;

        float GetChanceMult()
        {
            var sVal = Pawn.GetStatValue(PMStatDefOf.MutationAdaptability);

            var r = MutationUtilities.MaxMutationAdaptabilityValue - MutationUtilities.MinMutationAdaptabilityValue;
            sVal = Mathf.Abs(MutationUtilities.AverageMutationAdaptabilityValue - sVal)/r*2; //converts the raw stat value to a value representing the distance the state 
            //is from the default value, normalized for the total range of the stat 
            sVal -= r / 2;//shift the range of the modified stat value from [0,r/2] to [-r/2,0]
            sVal *= -2 / r; //shift the range again to [0,1], where a default stat value is 0, and either min or max is 0 
            sVal = Mathf.Max(sVal, 0); //make sure it doesn't go below zero, can happen if the default is not the center point of min and max 
            return sVal; 
        }

        //disabled because it caused large amounts of lag, enable only if debugging 
#if false
        /// <summary>
        /// gets the debug string 
        /// </summary>
        /// <returns></returns>
        public override string CompDebugString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(base.CompDebugString());
            builder.AppendLine($"stop multiplier is {GetChanceMult()}");
            return builder.ToString(); 
        }

#endif

        private void CheckIfHalted()
        {
            if (parent.CurStage is MutationStage mStage)
            {
                var stopChance = mStage.stopChance * GetChanceMult();
                if (Rand.Value < stopChance)
                {
                    _halted = true; 
                }
            }
            else
                return; 
        }


        /// <summary>
        /// get the change in severity per day 
        /// </summary>
        /// <returns></returns>
        protected override float SeverityChangePerDay()
        {
            if (_halted) return 0; 
            float statValue = Pawn.GetStatValue(PMStatDefOf.MutationAdaptability);
            float maxSeverity = Mathf.Max(statValue+1, 1);
            float minSeverity = Mathf.Min(statValue,0); //have the stat influence how high or low the severity can be 
            var sMult = Props.statEffectMult * (statValue+1);

            if (parent.Severity > maxSeverity) return 0;
            if (parent.Severity < minSeverity) return 0; 


            return base.SeverityChangePerDay() * sMult; 
        }
    }

    /// <summary>
    /// comp properties for mutation adjust hediff comp 
    /// </summary>
    /// <seealso cref="Pawnmorph.Utilities.HediffCompPropertiesBase{T}" />
    public class CompProperties_MutationSeverityAdjust : HediffCompProperties_SeverityPerDay
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompProperties_MutationSeverityAdjust"/> class.
        /// </summary>
        public CompProperties_MutationSeverityAdjust()
        {
            compClass = typeof(Comp_MutationSeverityAdjust); 
        }

        /// <summary>
        /// The stat effect multiplier
        /// </summary>
        /// values less then 1 will make the mutation adaptability stat have less of an effect
        /// values greater then 1 will increase it's effect
        public float statEffectMult = 1;

        private const float EPSILON = 0.01f;
        /// <summary>
        /// Gets the natural severity limit for the given pawn
        /// </summary>
        /// this value is the value the attached hediff should reach if the pawn has had the mutation for an 'infinite' amount of time 
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        public float GetNaturalSeverityLimitFor([NotNull] Pawn pawn)
        {
            float sVal = pawn.GetStatValue(PMStatDefOf.MutationAdaptability);
            float sMul = (sVal + 1) * statEffectMult;
            if (sMul < -EPSILON) return sVal;
            if (sMul > EPSILON) return sVal+1;
            return 0;
        }
    }
}