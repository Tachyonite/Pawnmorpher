// Comp_MutagenicInfecter.cs created by Iron Wolf for Pawnmorph on 02/07/2021 3:07 PM
// last updated 02/07/2021  3:07 PM

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Verse.HediffComp_Infecter" />
    public class Comp_MutagenicInfecter : HediffComp
    {
        private const int INJURY_CHECK_PERIOD = 60;

        private static readonly SimpleCurve InfectionChanceFactorFromTendQualityCurve = new SimpleCurve
        {
            new CurvePoint(0f, 0.7f),
            new CurvePoint(1f, 0.4f)
        };

        private static readonly SimpleCurve InfectionChanceFactorFromSeverityCurve = new SimpleCurve
        {
            new CurvePoint(1f, 0.1f),
            new CurvePoint(12f, 1f)
        };

        private float _infectionChanceFactorFromTendRoom = 1f;


        private bool _hasInjury;

        private int _ticksUntilInfect = -1;

        private CompProps_MutagenicInfecter Props => (CompProps_MutagenicInfecter) props;

        /// <summary>
        ///     Comps the expose data.
        /// </summary>
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref _infectionChanceFactorFromTendRoom, "infectionChanceFactor");
            Scribe_Values.Look(ref _ticksUntilInfect, "ticksUntilInfect", -1);
            Scribe_Values.Look(ref _hasInjury, "hasInjury");
        }


        /// <summary>
        ///     Comps the post merged.
        /// </summary>
        /// <param name="other">The other.</param>
        public override void CompPostMerged(Hediff other)
        {
            base.CompPostMerged(other);
            ResetTimer();
        }

        /// <summary>
        ///     Comps the post post add.
        /// </summary>
        /// <param name="dinfo">The dinfo.</param>
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (!MutagenDefOf.defaultMutagen.CanInfect(parent.pawn)) _ticksUntilInfect = -2;

            if (parent.IsPermanent())
            {
                _ticksUntilInfect = -2;
                return;
            }

            if (parent.Part.def.IsSolid(parent.Part, Pawn.health.hediffSet.hediffs))
            {
                _ticksUntilInfect = -2;
                return;
            }

            if (Pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(parent.Part))
            {
                _ticksUntilInfect = -2;
                return;
            }

            float num = Props.infectionChance;
            if (Pawn.RaceProps.Animal) num *= 0.1f;
            if (Rand.Value <= num)
                _ticksUntilInfect = HealthTuning.InfectionDelayRange.RandomInRange;
            else
                _ticksUntilInfect = -2;
        }

        /// <summary>
        ///     Comps the post tick.
        /// </summary>
        /// <param name="severityAdjustment">The severity adjustment.</param>
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (parent?.pawn?.IsHashIntervalTick(INJURY_CHECK_PERIOD) == true)
            {
                List<Hediff> hediffs = parent.pawn?.health?.hediffSet?.hediffs;
                if (hediffs == null) return;

                _hasInjury = false;
                foreach (Hediff hediff in hediffs)
                    if (hediff is Hediff_Injury injury)
                    {
                        var comp = injury.TryGetComp<HediffComp_Infecter>();
                        if (comp != null)
                        {
                            _hasInjury = true;
                            break;
                        }
                    }
            }

            if (!_hasInjury) return;

            if (_ticksUntilInfect > 0)
            {
                _ticksUntilInfect--;
                if (_ticksUntilInfect == 0) CheckMakeInfection();
            }
        }


        /// <summary>
        ///     Comps the tended new temporary.
        /// </summary>
        /// <param name="quality">The quality.</param>
        /// <param name="maxQuality">The maximum quality.</param>
        /// <param name="batchPosition">The batch position.</param>
        public override void CompTended_NewTemp(float quality, float maxQuality, int batchPosition = 0)
        {
            base.CompTended_NewTemp(quality, maxQuality, batchPosition);
            if (Pawn.Spawned)
            {
                Room room = Pawn.GetRoom();
                if (room != null) _infectionChanceFactorFromTendRoom = room.GetStat(RoomStatDefOf.InfectionChanceFactor);
            }
        }

        /// <summary>
        /// Resets the infection timer if stopped.
        /// </summary>
        public void ResetIfStopped()
        {
            if(_ticksUntilInfect <= 0)
                ResetTimer();
        }

        /// <summary>
        ///     Resets the timer for mutagenic infection
        /// </summary>
        public void ResetTimer()
        {
            if (!MutagenDefOf.defaultMutagen.CanInfect(parent.pawn)) _ticksUntilInfect = -2;

            if (parent.IsPermanent())
            {
                _ticksUntilInfect = -2;
                return;
            }

            if (parent.Part.def.IsSolid(parent.Part, Pawn.health.hediffSet.hediffs))
            {
                _ticksUntilInfect = -2;
                return;
            }

            if (Pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(parent.Part))
            {
                _ticksUntilInfect = -2;
                return;
            }

            float num = Props.infectionChance;
            if (Pawn.RaceProps.Animal) num *= 0.1f;
            if (Rand.Value <= num)
                _ticksUntilInfect = HealthTuning.InfectionDelayRange.RandomInRange;
            else
                _ticksUntilInfect = -2;
        }

        private void CheckMakeInfection()
        {
            if (Pawn.health.immunity.DiseaseContractChanceFactor(HediffDefOf.WoundInfection, parent.Part) <= 0.001f)
            {
                _ticksUntilInfect = -3;
                return;
            }

            var num = 1f;


            num *= InfectionChanceFactorFromSeverityCurve.Evaluate(parent.Severity);
            if (Pawn.Faction == Faction.OfPlayer) num *= Find.Storyteller.difficultyValues.playerPawnInfectionChanceFactor;
            if (Rand.Value < num)
            {
                _ticksUntilInfect = -4;

                bool enabled = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableMutagenDiseases;
                if (!enabled) return;


                if (!MutagenDefOf.defaultMutagen.CanInfect(parent.pawn)) return;

                Pawn.health.AddHediff(Hediffs.MorphTransformationDefOf.PM_MutagenicInfection, parent.Part);
            }
            else
            {
                _ticksUntilInfect = -3;
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <seealso cref="Verse.HediffCompProperties_Infecter" />
    public class CompProps_MutagenicInfecter : HediffCompProperties_Infecter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CompProps_MutagenicInfecter" /> class.
        /// </summary>
        public CompProps_MutagenicInfecter()
        {
            compClass = typeof(Comp_MutagenicInfecter);
        }
    }
}