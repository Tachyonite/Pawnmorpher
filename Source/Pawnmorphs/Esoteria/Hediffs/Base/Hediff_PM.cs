#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// A base class for all Pawnmorpher hediffs.  This class doesn't have any special functionality of its own, but it supports
    /// special Pawnmorpher HediffComps and other performance optimizations that allow it to perform better than vanilla hediffs.
    /// This is necessary because Pawnmorpher adds significantly more hediffs to pawns than vanilla or most mods, and this is a
    /// major performance bottleneck.
    /// <br/>
    /// Note that the <see cref="Hediff_PM{THediff,TDef}"/> subclass knows its own Def type and can perform compile-time
    /// verification that the correct Hediff is being used for the def.  You should prefer subclassing that class instead of this
    /// one directly.  This class exists primarily to serve as a non-generic base class for all Pawnmorpher hediffs. 
    /// </summary>
    /// <seealso cref="Hediff_PM{THediff,TDef}"/>
    public abstract class Hediff_PM : HediffWithComps
    {
        private readonly List<HediffComp_PM> _pmComps = new();

        // Used to skip ticking comps or PMComps entirely if none exist
        [Unsaved] private bool _hasComps = false;
        [Unsaved] private bool _hasPMComps = false;

        /// <summary>
        /// A list of all Pawnmorpher-specific HediffComps attached to this hediff.  <see cref="HediffComp_PM"/> hediffs are
        /// ticked less often than ordinary comps for performance reasons.
        /// </summary>
        public IEnumerable<HediffComp_PM> PMComps => _pmComps;

        /// <summary>
        /// The hash offset, used for deciding when to run the long ticks 
        /// </summary>
        [field: Unsaved]
        protected int HashOffset { get; private set; }

        // TODO - Optimize, see if we can/should incorporate Hediff_Descriptive's changes
        /// <summary>
        /// The base portion of the label shown in the health screen.  Is the "Fur" part of "Fur (growing)"
        /// </summary>
        public override string LabelBase
        {
            get
            {
                var stringBuilder = new StringBuilder();
                foreach (HediffComp_PM pmComp in PMComps)
                {
                    string? prefix = pmComp.CompLabelPrefix;
                    if (!prefix!.NullOrEmpty())
                    {
                        stringBuilder.Append(prefix!);
                        stringBuilder.Append(" ");
                    }
                }

                stringBuilder.Append(base.LabelBase!);
                return stringBuilder.ToString();
            }
        }

        // TODO - Optimize, see if we can/should incorporate Hediff_Descriptive's changes
        /// <summary>
        /// The portion of the label in parentheses shown in the health screen.  Is the "growing" part of "Fur (growing)"
        /// </summary>
        public override string LabelInBrackets
        {
            get
            {
                var stringBuilder = new StringBuilder(base.LabelInBrackets!);

                foreach (HediffComp_PM pmComp in PMComps)
                {
                    string? labelExtra = pmComp.CompLabelInBracketsExtra;
                    if (!labelExtra!.NullOrEmpty())
                    {
                        if (stringBuilder.Length != 0)
                            stringBuilder.Append(", ");
                        stringBuilder.Append(labelExtra!);
                    }
                }

                return stringBuilder.ToString();
            }
        }

        // TODO - Optimize, see if we can incorporate Hediff_Descriptive's changes
        /// <summary>
        /// A description of the hediff, shown in the tooltip when hovering over it.
        /// </summary>
        public override string Description
        {
            get
            {
                var stringBuilder = new StringBuilder(base.Description!);
                foreach (HediffComp_PM pmComp in PMComps)
                {
                    string? descriptionExtra = pmComp.CompDescriptionExtra;
                    if (!descriptionExtra!.NullOrEmpty())
                    {
                        stringBuilder.Append(" ");
                        stringBuilder.Append(descriptionExtra!);
                    }
                }

                return stringBuilder.ToString();
            }
        }

        // TODO - Optimize, see if we can/should incorporate Hediff_Descriptive's changes
        /// <summary>
        /// Extra information shown at the bottom of the tooltip, after the description
        /// </summary>
        public override string TipStringExtra
        {
            get
            {
                var stringBuilder = new StringBuilder(base.TipStringExtra!);

                foreach (HediffComp_PM pmComp in PMComps)
                {
                    string? tipStringExtra = pmComp.CompTipStringExtra;
                    if (!tipStringExtra!.NullOrEmpty())
                        stringBuilder.AppendLine(tipStringExtra!);
                }

                return stringBuilder.ToString();
            }
        }

        // TODO - Optimize
        /// <summary>
        /// An icon shown in the health screen next to the hediff, such as vanilla's bleeding icons.
        /// </summary>
        public override TextureAndColor StateIcon
        {
            get
            {
                foreach (HediffComp_PM pmComp in PMComps)
                {
                    TextureAndColor compStateIcon = pmComp.CompStateIcon;
                    if (compStateIcon.HasValue)
                        return compStateIcon;
                }

                return base.StateIcon;
            }
        }

        // TODO - Optimize this
        /// <summary>
        /// The hediff will be removed from the pawn if this ever returns true during the health tick
        /// </summary>
        public override bool ShouldRemove => PMComps.Any(t => t.CompShouldRemove) || base.ShouldRemove;

        // TODO - Optimize this
        /// <summary>
        /// If the hediff is currently invisible, it will turn visible the first time this returns true.
        /// Note that setting it to false again will not make the hediff invisible again.
        /// </summary>
        public override bool Visible => !PMComps.Any(t => t.CompDisallowVisible()) && base.Visible;


        /// <summary>
        /// Returns the list of Gizmos (toolbar buttons) that this hediff adds to the pawn, if any
        /// </summary>
        /// <returns>The gizmos this hediff adds</returns>
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerable<Gizmo>? baseGizmos = base.GetGizmos();
            if (baseGizmos != null)
                foreach (Gizmo gizmo in baseGizmos.Where(g => g != null))
                    yield return gizmo;

            foreach (HediffComp_PM pmComp in PMComps)
            {
                IEnumerable<Gizmo>? gizmos = pmComp.CompGetGizmos();
                if (gizmos != null)
                    foreach (Gizmo gizmo in gizmos)
                        yield return gizmo;
            }
        }

        /// <summary>
        /// Called after the hediff is initialized but before it is added to the pawn.
        /// Any object construction should happen here. 
        /// </summary>
        public override void PostMake()
        {
            base.PostMake();

            InitializePMComps();
            for (int index = _pmComps.Count - 1; index >= 0; --index)
                try
                {
                    _pmComps[index]!.CompPostMake();
                }
                catch (Exception ex)
                {
                    Log.Error("Error in HediffComp_PM.CompPostMake(): " + ex);
                    _pmComps.RemoveAt(index);
                }
        }

        /// <summary>
        /// Called after the hediff is first added to the pawn.
        /// Any initialization that requires the pawn to be set should be done here.
        /// <br />
        /// Note that this is not called during loading, only when the hediff is initially added.
        /// </summary>
        public override void PostAdd(DamageInfo? dInfo)
        {
            base.PostAdd(dInfo);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.CompPostPostAdd(dInfo);
            PostAddOrLoad();
        }

        /// <summary>
        /// Called after the hediff is first added to the pawn, and any time the game is loaded (during the
        /// <see cref="LoadSaveMode.PostLoadInit" /> phase).
        /// <br />
        /// Any initialization that needs to be repeated after every load can be done here.
        /// </summary>
        protected virtual void PostAddOrLoad()
        {
            HashOffset = pawn?.HashOffset() ?? 0;
        }

        /// <summary>
        /// Called after the hediff is removed from the pawn.
        /// </summary>
        public override void PostRemoved()
        {
            base.PostRemoved();
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.CompPostPostRemoved();
        }

        //TODO - override Tick() the same way Hediff_StageChanges does

        /// <summary>
        /// Called after the main Hediff tick.  Comps and PMComps are ticked in this method.
        /// <br />
        /// Note that this method is a MAJOR hot path -- it should be optimized as much as possible because it potentially gets
        /// called tens of thousands of times per second. For this reason, foreach loops should be avoided because the iterator
        /// object creation is a notable source of slowdown.
        /// </summary>
        public override void PostTick()
        {
            // We're not calling base.PostTick() here because we're manually replicating it in a slightly more optimized way
            var severityAdjustment = 0f;

            if (_hasComps && comps != null)
            {
                int compCount = comps.Count; // Not caching this genuinely has a noticeable performance impact for hediffs that
                // have multiple comps.  We can't cache it beyond the tick, however, because
                // exceptions in certain places can cause comps to be removed.
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var index = 0; index < compCount; ++index)
                    comps[index]?.CompPostTick(ref severityAdjustment);
            }

            if (_hasPMComps)
            {
                int compCount = _pmComps.Count; // Same as above
                int hashOffsetTick = Find.TickManager!.TicksGame + HashOffset;

                // Note that PMComps.CompPostTick() is never called.  This is because that method exists only for legacy support.
                // That method cannot be overridden and only does exactly what we're doing here (but slightly less efficiently)

                if (hashOffsetTick % 60 == 0) // Every real-life second
                    for (var index = 0; index < compCount; ++index)
                        _pmComps[index]?.TickSecond(ref severityAdjustment);

                if (hashOffsetTick % 2500 == 0) // Once an in-game hour
                    for (var index = 0; index < compCount; ++index)
                        _pmComps[index]?.TickHour(ref severityAdjustment);

                if (hashOffsetTick % 60000 == 0) // Once an in-game day
                    for (var index = 0; index < compCount; ++index)
                        _pmComps[index]?.TickDay(ref severityAdjustment);
            }

            if (severityAdjustment != 0.0f)
                Severity += severityAdjustment;
        }

        /// <summary>
        /// Called when attempting to merge this hediff with another hediff.
        /// CompPostMerge is called on all comps if the merge succeeds.
        /// </summary>
        public override bool TryMergeWith(Hediff other)
        {
            if (!base.TryMergeWith(other))
                return false;
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.CompPostMerged(other);
            return true;
        }

        /// <summary>
        /// Called when this hediff is tended by a doctor
        /// </summary>
        /// <param name="quality">The base quality of the tend, before randomization</param>
        /// <param name="maxQuality">The cap on tend quality based on the medicine used</param>
        /// <param name="batchPosition">
        /// The position in the batch the hediff was tended, if multiple hediffs were tended at the same time
        /// </param>
        public override void Tended(float quality, float maxQuality, int batchPosition = 0)
        {
            base.Tended(quality, maxQuality, batchPosition);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.CompTended(quality, maxQuality, batchPosition);
        }

        /// <summary>
        /// Called when the pawn is about to die, but is not yet dead.
        /// Any cleanup that requires a living pawn should go here.
        /// </summary>
        public override void Notify_PawnDied()
        {
            base.Notify_PawnDied();
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_PawnDied();
        }

        /// <summary>
        /// Called just after the pawn is killed.
        /// Any post-death effects should go here.
        /// </summary>
        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_PawnKilled();
        }

        /// <summary>
        /// Called when this pawn kills another pawn. This is called after the victim is already dead.
        /// </summary>
        /// <param name="victim">The unfortunate victim.</param>
        /// <param name="dInfo">Info about the attack that killed the victim, if there was one.</param>
        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dInfo)
        {
            base.Notify_KilledPawn(victim, dInfo);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_KilledPawn(victim, dInfo);
        }

        /// <summary>
        /// Called after this pawn takes damage.
        /// </summary>
        /// <param name="dInfo">Info about the attack that caused the damage.</param>
        /// <param name="totalDamageDealt">The total amount of damage applied to the pawn.</param>
        public override void Notify_PawnPostApplyDamage(DamageInfo dInfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dInfo, totalDamageDealt);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_PawnPostApplyDamage(dInfo, totalDamageDealt);
        }

        /// <summary>
        /// Called when a chemical is applying any of its effects to the pawn.  The strength of the effect can be controlled
        /// by modifying the effect parameter.
        /// <br />
        /// "Effect" could be the hediff severity, amount of chemical tolerance, or a need change, and should not be assumed to
        /// have any particular meaning or limits to its value.  Any changes should be made relative to the initial value.
        /// </summary>
        /// <param name="chem">The chemical applying the effect</param>
        /// <param name="effect">A float representing the severity of the effect</param>
        public override void ModifyChemicalEffect(ChemicalDef chem, ref float effect)
        {
            base.ModifyChemicalEffect(chem, ref effect);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.CompModifyChemicalEffect(chem, ref effect);
        }

        /// <summary>
        /// Called when the pawn uses a verb.
        /// </summary>
        /// <param name="verb">The verb that was used.</param>
        /// <param name="target">Information about the target of the verb.</param>
        public override void Notify_PawnUsedVerb(Verb verb, LocalTargetInfo target)
        {
            base.Notify_PawnUsedVerb(verb, target);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_PawnUsedVerb(verb, target);
        }

        /// <summary>
        /// Called when the pawn gains neural heat.
        /// </summary>
        /// <param name="baseAmount">The base amount of entropy that would have been gained.</param>
        /// <param name="finalAmount">The actual amount of entropy gained, after accounting for the pawn's stats.</param>
        /// <param name="src">The source of the heat, if one exists.</param>
        public override void Notify_EntropyGained(float baseAmount, float finalAmount, Thing? src = null)
        {
            base.Notify_EntropyGained(baseAmount, finalAmount, src!);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_EntropyGained(baseAmount, finalAmount, src!);
        }

        /// <summary>
        /// Was presumably related to the old Royalty effect where the empire could get upset if you used implants.
        /// Vanilla does not appear to call it any longer and it should be presumed obsolete.
        /// </summary>
        [Obsolete("This method is no longer called anywhere by vanilla code")]
        public override void Notify_ImplantUsed(
            string violationSourceName,
            float detectionChance,
            int violationSourceLevel = -1)
        {
            base.Notify_ImplantUsed(violationSourceName, detectionChance, violationSourceLevel);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_ImplantUsed(violationSourceName, detectionChance, violationSourceLevel);
        }

        /// <summary>
        /// Called when saving and several times during loading, to read/write the save data from XML.  Saving and loading of
        /// simple values can be done with the <see cref="Scribe_Values" /> and related classes.  If you need to perform more
        /// complex initialization, you can determine the current mode by checking the value of
        /// <see cref="Scribe.mode">Scribe.mode</see>.
        /// <br />
        /// Loading is somewhat more complex than saving, and happens in three phases: <br />
        /// - <see cref="LoadSaveMode.LoadingVars" /> is for initializing the objects to prepare them for loading data.
        /// Any object construction should happen during this mode. References to other objects are not yet valid.<br />
        /// - <see cref="LoadSaveMode.ResolvingCrossRefs" /> happens once all objects have been initialized, and is when object
        /// cross-references are resolved.<br />
        /// - <see cref="LoadSaveMode.PostLoadInit" /> happens last, after all cross references have been resolved.
        /// Any one-time initialization that requires the object to be fully loaded should be done here. <br />
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            switch (Scribe.mode)
            {
                case LoadSaveMode.LoadingVars:
                    InitializePMComps();
                    break;
                case LoadSaveMode.PostLoadInit:
                    PostAddOrLoad();
                    break;
            }

            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.CompExposeData();
        }

        private void InitializePMComps()
        {
            if (def is not HediffDef_PM pmDef)
                return;

            if (pmDef.pmComps == null)
                return;

            foreach (HediffCompProps_PM compProp in pmDef.pmComps)
            {
                HediffComp_PM? pmComp = null;
                try
                {
                    pmComp = (HediffComp_PM)Activator.CreateInstance(compProp.compClass!)!;
                    pmComp.props = compProp;
                    pmComp.parent = this;
                    _pmComps.Add(pmComp);
                }
                catch (Exception ex)
                {
                    Log.Error("Could not instantiate or initialize a HediffComp_PM: " + ex);
                    _pmComps.Remove(pmComp!);
                }
            }

            // Cache whether or not we have comps and PMComps.
            // This is used to skip attempting to iterate over them if we do not, for performance reasons
            _hasComps = comps.IsNonNullAndNonEmpty();
            _hasPMComps = _pmComps.Count > 0;
        }

        /// <summary>
        /// Generates a string for debugging that describes the hediff, including any debug strings from attached comps.
        /// </summary>
        /// <returns>A debugging string describing the Hediff</returns>
        public override string DebugString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.DebugString() ?? string.Empty);
            if (comps != null)
                foreach (HediffComp_PM pmComp in PMComps)
                {
                    var compName = pmComp.ToString();
                    if (compName.Contains('_'))
                        compName = pmComp.ToString().Split('_')[1];
                    stringBuilder.Append("--").AppendLine(compName);
                    string? debug = pmComp.CompDebugString();
                    if (!debug!.NullOrEmpty())
                        stringBuilder.AppendLine(debug!.TrimEnd().Indented()!);
                }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// A subclass of <see cref="Hediff_PM"/> that knows its own type and its def type and can perform compile-time verification
    /// that the hediff is associated with the correct def.
    /// <br/>
    /// See https://en.wikipedia.org/wiki/Bounded_quantification#F-bounded_quantification if your want to understand what the type
    /// bounds are doing doing.
    /// </summary>
    /// <example>
    /// Define your classes like so:
    /// <code>
    /// class MyHediff : Hediff_PM{MyHediff, MyHediffDef}
    /// class MyHediffDef : HediffDef_PM{MyHediff, MyHediffDef}
    /// </code>
    /// </example>
    /// <typeparam name="THediff">The concrete type of the Hediff_PM</typeparam>
    /// <typeparam name="TDef">The concrete type of the HediffDef_PM</typeparam>
    public abstract class Hediff_PM<THediff, TDef> : Hediff_PM
        where THediff : Hediff_PM<THediff, TDef>
        where TDef : HediffDef_PM<THediff, TDef>
    {
        [Unsaved] private TDef? _def; // Cache this so we're not constantly typechecking it every time Def is called

        /// <summary>
        /// The Def of this hediff, cast to the correct type
        /// </summary>
        /// <exception cref="InvalidCastException">If the def was not of the expected type</exception>
        public TDef Def => _def ??= def as TDef
                                 ?? throw new InvalidCastException($"HediffDef {def?.defName} had incorrect type!"
                                                                 + $" Should have been {typeof(TDef).Name} but was instead"
                                                                 + $" {def?.GetType().Name}");
    }
}