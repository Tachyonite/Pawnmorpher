#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        /// <summary>
        /// A list of all Pawnmorpher-specific HediffComps attached to this hediff.  <see cref="HediffComp_PM"/> hediffs are
        /// ticked less often than ordinary comps for performance reasons.
        /// </summary>
        public IEnumerable<HediffComp_PM> PMComps => _pmComps;

        // TODO
        // public override string LabelBase
        // {
        //     get
        //     {
        //         StringBuilder stringBuilder = new StringBuilder();
        //         int index = 0;
        //         while (true)
        //         {
        //             int num = index;
        //             int? count = this.comps?.Count;
        //             int valueOrDefault = count.GetValueOrDefault();
        //             if (num < valueOrDefault & count.HasValue)
        //             {
        //                 string compLabelPrefix = this.comps[index].CompLabelPrefix;
        //                 if (!compLabelPrefix.NullOrEmpty())
        //                 {
        //                     stringBuilder.Append(compLabelPrefix);
        //                     stringBuilder.Append(" ");
        //                 }
        //
        //                 ++index;
        //             }
        //             else
        //                 break;
        //         }
        //
        //         stringBuilder.Append(base.LabelBase);
        //         return stringBuilder.ToString();
        //     }
        // }

        // TODO
        // public override string LabelInBrackets
        // {
        //     get
        //     {
        //         StringBuilder stringBuilder = new StringBuilder();
        //         stringBuilder.Append(base.LabelInBrackets);
        //         int index = 0;
        //         while (true)
        //         {
        //             int num = index;
        //             int? count = this.comps?.Count;
        //             int valueOrDefault = count.GetValueOrDefault();
        //             if (num < valueOrDefault & count.HasValue)
        //             {
        //                 string labelInBracketsExtra = this.comps[index].CompLabelInBracketsExtra;
        //                 if (!labelInBracketsExtra.NullOrEmpty())
        //                 {
        //                     if (stringBuilder.Length != 0)
        //                         stringBuilder.Append(", ");
        //                     stringBuilder.Append(labelInBracketsExtra);
        //                 }
        //
        //                 ++index;
        //             }
        //             else
        //                 break;
        //         }
        //
        //         return stringBuilder.ToString();
        //     }
        // }

        // TODO - Optimize this
        /// <inheritdoc />
        public override bool ShouldRemove => PMComps.Any(t => t.CompShouldRemove) || base.ShouldRemove;

        // TODO - Optimize this
        /// <inheritdoc />
        public override bool Visible => !PMComps.Any(t => t.CompDisallowVisible()) && base.Visible;

        // TODO
        // public override string TipStringExtra
        // {
        //     get
        //     {
        //         StringBuilder stringBuilder = new StringBuilder();
        //         stringBuilder.Append(base.TipStringExtra);
        //         if (this.comps != null)
        //         {
        //             for (int index = 0; index < this.comps.Count; ++index)
        //             {
        //                 string compTipStringExtra = this.comps[index].CompTipStringExtra;
        //                 if (!compTipStringExtra.NullOrEmpty())
        //                     stringBuilder.AppendLine(compTipStringExtra);
        //             }
        //         }
        //
        //         return stringBuilder.ToString();
        //     }
        // }
        
        // TODO
        // public override string Description
        // {
        //     get
        //     {
        //         StringBuilder stringBuilder = new StringBuilder(base.Description);
        //         int index = 0;
        //         while (true)
        //         {
        //             int num = index;
        //             int? count = this.comps?.Count;
        //             int valueOrDefault = count.GetValueOrDefault();
        //             if (num < valueOrDefault & count.HasValue)
        //             {
        //                 string descriptionExtra = this.comps[index].CompDescriptionExtra;
        //                 if (!descriptionExtra.NullOrEmpty())
        //                 {
        //                     stringBuilder.Append(" ");
        //                     stringBuilder.Append(descriptionExtra);
        //                 }
        //
        //                 ++index;
        //             }
        //             else
        //                 break;
        //         }
        //
        //         return stringBuilder.ToString();
        //     }
        // }

        // TODO - Optimize
        /// <inheritdoc/>
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

        // TODO - optimize
        /// <inheritdoc />
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
        
        /// <inheritdoc />
        public override void PostMake()
        {
            base.PostMake();
            InitializePMComps();
            for (int index = _pmComps.Count - 1; index >= 0; --index)
            {
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
        }

        /// <inheritdoc />
        public override void PostAdd(DamageInfo? dInfo)
        {
            base.PostAdd(dInfo);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.CompPostPostAdd(dInfo);
        }
        
        /// <inheritdoc />
        public override void PostRemoved()
        {
            base.PostRemoved();
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.CompPostPostRemoved();
        }

        //TODO - override Tick() the same way Hediff_StageChanges does
        
        /// <inheritdoc />
        public override void PostTick()
        {
            base.PostTick();
            //TODO - Do the magic stuff here
            // if (this.comps == null)
            //     return;
            // float severityAdjustment = 0.0f;
            // for (int index = 0; index < this.comps.Count; ++index)
            //     this.comps[index].CompPostTick(ref severityAdjustment);
            // if ((double)severityAdjustment == 0.0)
            //     return;
            // this.Severity += severityAdjustment;
        }

        /// <inheritdoc />
        public override bool TryMergeWith(Hediff other)
        {
            if (!base.TryMergeWith(other))
                return false;
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.CompPostMerged(other);
            return true;
        }

        /// <inheritdoc />
        public override void Tended(float quality, float maxQuality, int batchPosition = 0)
        {
            base.Tended(quality, maxQuality, batchPosition);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.CompTended(quality, maxQuality, batchPosition);
        }

        /// <inheritdoc/>
        public override void Notify_PawnDied()
        {
            base.Notify_PawnDied();
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_PawnDied();
        }

        /// <inheritdoc />
        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_PawnKilled();
        }

        /// <inheritdoc />
        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dInfo)
        {
            base.Notify_KilledPawn(victim, dInfo);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_KilledPawn(victim, dInfo);
        }

        /// <inheritdoc />
        public override void Notify_PawnPostApplyDamage(DamageInfo dInfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dInfo, totalDamageDealt);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_PawnPostApplyDamage(dInfo, totalDamageDealt);
        }

        /// <inheritdoc />
        public override void ModifyChemicalEffect(ChemicalDef chem, ref float effect)
        {
            base.ModifyChemicalEffect(chem, ref effect);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.CompModifyChemicalEffect(chem, ref effect);
        }

        /// <inheritdoc />
        public override void Notify_PawnUsedVerb(Verb verb, LocalTargetInfo target)
        {
            base.Notify_PawnUsedVerb(verb, target);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_PawnUsedVerb(verb, target);
        }

        /// <inheritdoc />
        public override void Notify_EntropyGained(float baseAmount, float finalAmount, Thing? src = null)
        {
            base.Notify_EntropyGained(baseAmount, finalAmount, src!);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_EntropyGained(baseAmount, finalAmount, src!);
        }

        /// <inheritdoc />
        public override void Notify_ImplantUsed(
            string violationSourceName,
            float detectionChance,
            int violationSourceLevel = -1)
        {
            base.Notify_ImplantUsed(violationSourceName, detectionChance, violationSourceLevel);
            foreach (HediffComp_PM pmComp in PMComps)
                pmComp.Notify_ImplantUsed(violationSourceName, detectionChance, violationSourceLevel);
        }

        /// <inheritdoc/>
        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                InitializePMComps();
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
        }

        /// <inheritdoc />
        public override string DebugString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.DebugString() ?? string.Empty);
            if (comps != null)
            {
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
    public abstract class Hediff_PM<THediff, TDef> : HediffWithComps where THediff : Hediff_PM<THediff, TDef>
                                                                     where TDef : HediffDef_PM<THediff, TDef>
    {
        private TDef? _def; // Cache this so we're not constantly typechecking it every time Def is called

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