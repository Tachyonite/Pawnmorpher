// Comp_MutationSeverityAdjust.cs modified by Iron Wolf for Pawnmorph on 12/01/2019 8:55 AM
// last updated 12/01/2019  8:55 AM

using System;
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
        /// get the change in severity per day 
        /// </summary>
        /// <returns></returns>
        protected override float SeverityChangePerDay()
        {
            float statValue = Pawn.GetStatValue(PMStatDefOf.MutationAdaptability);
            float maxSeverity = Mathf.Max(statValue, 1);
            float minSeverity = Mathf.Min(statValue - 1,0); //have the stat influence how high or low the severity can be 
            var sMult = Props.statEffectMult * statValue;

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
            var sVal = pawn.GetStatValue(PMStatDefOf.MutationAdaptability);
            var sMul = sVal * statEffectMult * severityPerDay;
            if (sMul < -EPSILON) return sVal - 1;
            if (sMul > EPSILON) return sVal;
            return 0; 
        }
    }
}