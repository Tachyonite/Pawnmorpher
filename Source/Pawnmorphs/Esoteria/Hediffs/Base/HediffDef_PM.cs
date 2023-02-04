#nullable enable

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// The base Def class for <see cref="Hediff_PM"/> hediffs.  This is mostly identical to <see cref="Verse.HediffDef"/> but
    /// features an additional field for <see cref="HediffComp_PM"/> comps.  These custom comps are not ticked every tick but
    /// rather support ticks every 60, 2500, or 60000 ticks for performance reasons.
    /// <br/>
    /// Note that the <see cref="HediffDef_PM{THediff,TDef}"/> subclass knows its own Hediff type and automatically ensures it is
    /// instantiated as the correct type.  You should prefer subclassing that class instead of this one directly.  This class
    /// exists primarily to serve as a non-generic base class for all Pawnmorpher hediff defs. 
    /// </summary>
    /// <seealso cref="HediffDef_PM{THediff,TDef}"/>
    public abstract class HediffDef_PM : HediffDef
    {
        /// <summary>
        /// The Pawnmorpher comps attached to this hediff. <see cref="HediffComp_PM{TComp,TProps}"/> comps are only ticked once a
        /// second for performance reasons.
        /// </summary>
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public List<HediffCompProps_PM>? pmComps;

        /// <inheritdoc />
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (pmComps == null)
                return;
            foreach (HediffCompProps_PM compProps in pmComps)
                compProps.ResolveReferences(this);
        }

        /// <inheritdoc />
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string str in base.ConfigErrors()!)
                yield return str;

            if (pmComps != null)
                foreach (HediffCompProps_PM t in pmComps)
                    foreach (string configError in t.ConfigErrors(this)!)
                        yield return t + ": " + configError;
        }
    }

    /// <summary>
    /// A subclass of <see cref="HediffDef_PM"/> that knows its own type and its hediff's type and can automatically initialize
    /// its hediff to the correct type.
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
    /// <seealso cref="HediffDef_PM"/>
    /// <typeparam name="THediff">The concrete type of the Hediff_PM</typeparam>
    /// <typeparam name="TDef">The concrete type of the HediffDef_PM</typeparam>
    public abstract class HediffDef_PM<THediff, TDef> : HediffDef_PM
        where THediff : Hediff_PM<THediff, TDef>
        where TDef : HediffDef_PM<THediff, TDef>
    {
        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        protected HediffDef_PM()
        {
            hediffClass = typeof(THediff);
        }
    }
}