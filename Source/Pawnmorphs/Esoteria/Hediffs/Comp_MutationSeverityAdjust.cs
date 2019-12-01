// Comp_MutationSeverityAdjust.cs modified by Iron Wolf for Pawnmorph on 12/01/2019 8:55 AM
// last updated 12/01/2019  8:55 AM

using System;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
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
        /// get the change in severity per day 
        /// </summary>
        /// <returns></returns>
        protected override float SeverityChangePerDay()
        {
            var statValue = Props.statEffectMult * Pawn.GetStatValue(PMStatDefOf.MutationAdaptability);
            return base.SeverityChangePerDay() * statValue; 
        }
    }

    /// <summary>
    /// comp properties for mutation adjust hediff comp 
    /// </summary>
    /// <seealso cref="Pawnmorph.Utilities.HediffCompPropertiesBase{T}" />
    public class CompProperties_MutationSeverityAdjust : HediffCompProperties_SeverityPerDay
    {
        /// <summary>
        /// The stat effect multiplier
        /// </summary>
        /// values less then 1 will make the mutation adaptability stat have less of an effect
        /// values greater then 1 will increase it's effect
        public float statEffectMult = 1;

        
    }
}