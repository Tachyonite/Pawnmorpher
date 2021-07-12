// Comp_MutationSeverityAdjust.cs modified by Iron Wolf for Pawnmorph on 12/01/2019 8:55 AM
// last updated 12/01/2019  8:55 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     hediff comp that acts like severity per day but if affected by the 'Mutation Adaptability Stat'
    /// </summary>
    /// <seealso cref="Pawnmorph.Utilities.HediffCompBase{T}" />
    public class Comp_MutationSeverityAdjust : HediffComp_SeverityPerDay
    {
        private const float EPSILON = 0.001f;
        private bool _halted;

        private int _curStageIndex = -1;

        private bool _checkForReversionHediff;


        /// <summary>
        ///     Gets the natural severity limit.
        /// </summary>
        /// this value is the value the attached hediff should reach if the pawn has had the mutation for an 'infinite' amount of time
        /// <value>
        ///     The natural severity limit.
        /// </value>
        public float NaturalSeverityLimit => Props.GetNaturalSeverityLimitFor(Pawn);

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Comp_MutationSeverityAdjust" /> is halted.
        /// </summary>
        /// <value>
        ///     <c>true</c> if halted; otherwise, <c>false</c>.
        /// </value>
        public bool Halted
        {
            get => _halted;
            set
            {
                if (_halted != value)
                {
                    foreach (Hediff_AddedMutation allSimilarMutation in GetAllSimilarMutations())
                    {
                        Comp_MutationSeverityAdjust sevComp = allSimilarMutation.SeverityAdjust;
                        if (sevComp != null)
                        {
                            sevComp._halted =
                                value; //use the private variable so we don't trigger an infinite loop of comps setting each other
                            allSimilarMutation.Severity = parent.Severity; //make sure they have the same severity to 
                        }
                    }

                    _halted = value;
                }
            }
        }

        
        
        /// <summary>
        ///     Gets a value indicating whether the parent hediff should be removed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [comp should remove]; otherwise, <c>false</c>.
        /// </value>
        public override bool CompShouldRemove => ShouldRemove.Value;

        /// <summary>
        ///     Gets the change per day.
        /// </summary>
        /// <value>
        ///     The change per day.
        /// </value>
        public float ChangePerDay => SeverityChangePerDay();

        [NotNull]
        private CompProperties_MutationSeverityAdjust Props
        {
            get
            {
                try
                {
                    var p = (CompProperties_MutationSeverityAdjust) props;
                    if (p == null)
                        throw new InvalidOperationException($"props on {parent.def.defName} on {Pawn.Name} has not props");
                    return p;
                }
                catch (InvalidCastException e) //just incase the properties get miss assigned somehow 
                {
                    throw new
                        InvalidCastException($"could not convert hediff comp props of type {props.GetType().Name} to {nameof(CompProperties_MutationSeverityAdjust)}",
                                             e);
                }
            }
        }

        // prioritize updating spawned pawns over world pawns 
        private int TickRate => Pawn.SpawnedOrAnyParentSpawned ? 25 : 70;

        // These values only get recalculated every so often because of how expensive they are to calculate
        //TODO make some sort of unified caching system for stat lookups, easy way to cause lots of lag 
        private readonly Cached<float> StatAdjust;
        private readonly Cached<float> MutationAdaptability;
        private readonly Cached<bool> ShouldRemove;

        public Comp_MutationSeverityAdjust()
        {
            StatAdjust = new Cached<float>(() => Pawn.GetStatValue(PMStatDefOf.MutagenSensitivity));
            MutationAdaptability = new Cached<float>(() => Pawn.GetStatValue(PMStatDefOf.MutationAdaptability));
            ShouldRemove = new Cached<bool>(() => parent.CurStageIndex == 0 && SeverityChangePerDay() < 0);
        }

        /// <summary>
        ///     called to save/load data for this comp.
        /// </summary>
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref _halted, nameof(_halted));
            Scribe_Values.Look(ref _curStageIndex, nameof(_curStageIndex));
        }

        /// <summary>
        ///     called when the parent is merged with a new hediff of the same type
        /// </summary>
        /// <param name="other">The other.</param>
        public override void CompPostMerged(Hediff other)
        {
            base.CompPostMerged(other);
            Restart(); //restart adjusting if halted
        }

        /// <summary>
        ///     called every tick
        /// </summary>
        /// <param name="severityAdjustment">The severity adjustment.</param>
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            int sIndex = parent.CurStageIndex;
            if (sIndex != _curStageIndex)
            {
                _curStageIndex = sIndex;
                CheckIfHalted();
            }

            int tickRate = TickRate; //prioritize updating spawned pawns over world pawns 

            if (Pawn.IsHashIntervalTick(tickRate))
            {
                StatAdjust.Recalculate();
                MutationAdaptability.Recalculate();
                ShouldRemove.Recalculate();
            }
        }

        /// <summary>
        ///     Recalcs the adjust speed.
        /// </summary>
        public void RecalcAdjustSpeed()
        {
            _checkForReversionHediff = true;
        }

        /// <summary>
        ///     Restarts this instance.
        /// </summary>
        public void Restart()
        {
            Halted = false;
            StatAdjust.Recalculate();
            MutationAdaptability.Recalculate();
        }

        /// <summary>
        ///     get the change in severity per day
        /// </summary>
        /// <returns></returns>
        public override float SeverityChangePerDay()
        {
            if (_checkForReversionHediff || Pawn.IsHashIntervalTick(TickRate))
            {
                _checkForReversionHediff = false;
                bool hasReversionHediff = Pawn.health?.hediffSet?.HasHediff(MorphTransformationDefOf.PM_Reverting) == true;
                if (hasReversionHediff) return -1;
            }

            if (Halted) return 0;
            float statValue = MutationAdaptability.Value;
            float maxSeverity = Mathf.Max(statValue + 1, 1);
            float minSeverity = Mathf.Min(statValue, 0); //have the stat influence how high or low the severity can be 
            float sMult = Props.statEffectMult * (statValue + 1);
            float sevPerDay = base.SeverityChangePerDay() * sMult;
            //make sure the severity can only stay between the max and min 
            if (parent.Severity > maxSeverity) sevPerDay = Mathf.Min(0, sevPerDay);
            if (parent.Severity < minSeverity) sevPerDay = Mathf.Max(0, sevPerDay);


            return sevPerDay * Mathf.Max(StatAdjust.Value, 0); //take the mutagen sensitivity stat into account 
        }

        private void CheckIfHalted()
        {
            if (parent.CurStage is MutationStage mStage)
            {
                float stopChance = mStage.stopChance * GetChanceMult() * Pawn.GetStatValue(PMStatDefOf.MutationHaltChance);

                if (stopChance < 0.01f) return;
                if (Rand.Value < stopChance) Halted = true;
            }
        }

        [NotNull]
        private IEnumerable<Hediff_AddedMutation> GetAllSimilarMutations()
        {
            List<Hediff> hediffSet = Pawn.health?.hediffSet?.hediffs;
            foreach (Hediff_AddedMutation hediff in hediffSet.MakeSafe().OfType<Hediff_AddedMutation>())
                if (hediff != parent && hediff.def == parent.def)
                    yield return hediff;
        }

        private float GetChanceMult()
        {
            float sVal = MutationAdaptability.Value;

            float r = MutationUtilities.MaxMutationAdaptabilityValue - MutationUtilities.MinMutationAdaptabilityValue;
            sVal = Mathf.Abs(MutationUtilities.AverageMutationAdaptabilityValue - sVal)
                 / r
                 * 2; //converts the raw stat value to a value representing the distance the state 
            //is from the default value, normalized for the total range of the stat 
            sVal -= r / 2; //shift the range of the modified stat value from [0,r/2] to [-r/2,0]
            sVal *= -2 / r; //shift the range again to [0,1], where a default stat value is 0, and either min or max is 0 
            sVal = Mathf.Max(sVal, 0); //make sure it doesn't go below zero, can happen if the default is not the center point of min and max
            return sVal;
        }
    }

    /// <summary>
    ///     comp properties for mutation adjust hediff comp
    /// </summary>
    /// <seealso cref="Pawnmorph.Utilities.HediffCompPropertiesBase{T}" />
    public class CompProperties_MutationSeverityAdjust : HediffCompProperties_SeverityPerDay
    {
        private const float EPSILON = 0.01f;

        /// <summary>
        ///     The stat effect multiplier
        /// </summary>
        /// values less then 1 will make the mutation adaptability stat have less of an effect
        /// values greater then 1 will increase it's effect
        public float statEffectMult = 1;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompProperties_MutationSeverityAdjust" /> class.
        /// </summary>
        public CompProperties_MutationSeverityAdjust()
        {
            compClass = typeof(Comp_MutationSeverityAdjust);
        }

        /// <summary>
        ///     Gets the natural severity limit for the given pawn
        /// </summary>
        /// this value is the value the attached hediff should reach if the pawn has had the mutation for an 'infinite' amount of time
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        public float GetNaturalSeverityLimitFor([NotNull] Pawn pawn)
        {
            float sVal = pawn.GetStatValue(PMStatDefOf.MutationAdaptability);
            float sMul = (sVal + 1) * statEffectMult;
            if (sMul < -EPSILON) return sVal;
            if (sMul > EPSILON) return sVal + 1;
            return 0;
        }
    }
}