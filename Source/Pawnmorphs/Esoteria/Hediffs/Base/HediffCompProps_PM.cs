#nullable enable

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Properties that define a <see cref="HediffComp_PM{TComp,TProps}"/>.  Identical to <see cref="HediffCompProperties"/>
    /// except only meant to be used Comps that subclass <see cref="HediffComp_PM"/>.
    /// <br/>
    /// Note that the <see cref="HediffCompProps_PM{TComp,TProps}"/> subclass knows its own Comp type and is able to perform
    /// compile-time checking to ensure it is only used with the correct Comp.  You should prefer subclassing that class instead
    /// of this one directly.  This class exists primarily to serve as a non-generic base class for all Pawnmorpher hediff comp
    /// properties. 
    /// </summary>
    /// <seealso cref="HediffComp_PM{TComp,TProps}"/>
    public abstract class HediffCompProps_PM : HediffCompProperties
    {
    }

    /// <summary>
    /// Properties for <see cref="HediffComp_PM{TComp,TProps}"/>.  This is the same as HediffCompProperties but additionally
    /// is able to perform verification to ensure that it's only used with the correct HediffComp_PM.
    /// <br/>
    /// <see cref="HediffCompProperties.compClass"/> is automatically initialized to the correct type as well.
    /// </summary>
    /// <example>
    /// To use, define your classes like so:
    /// <code>
    /// class MyComp : HediffComp_PM{MyComp, MyCompProps}
    /// class MyCompProps : HediffCompProps_PM{MyComp, MyCompProps}
    /// </code>
    /// </example>
    /// <seealso cref="HediffCompProps_PM"/>
    /// <seealso cref="HediffComp_PM{TComp,TProps}"/>
    /// <typeparam name="TComp">The concrete type of the HediffComp_PM</typeparam>
    /// <typeparam name="TProps">The concrete type of the HediffCompProps_PM</typeparam>
    public abstract class HediffCompProps_PM<TComp, TProps> : HediffCompProps_PM
        where TComp : HediffComp_PM<TComp, TProps>
        where TProps : HediffCompProps_PM<TComp, TProps>
    {
        /// <summary>
        /// Called once during loading, while Defs are being finalized.
        /// Any special initialization behavior for a Def should be done here.
        /// </summary>
        public override void ResolveReferences(HediffDef parentDef)
        {
            base.ResolveReferences(parentDef);
            compClass ??= typeof(TComp); // Only assign comp class here if the def didn't manually specify one
        }

        /// <summary>
        /// Called once during loading, to check Defs for any configuration errors.
        /// Any error checking of Defs should be done here.
        /// </summary>
        /// <param name="parentDef">The parent hediff def this comp is attached to</param>
        /// <returns>An enumerable of any configuration errors detected during loading</returns>
        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef)!)
                yield return error;

            if (compClass != null && !compClass.IsAssignableFrom(typeof(TComp)))
                yield return $"PMComp {GetType().Name} attached to {parentDef.defName} had an invalid compClass."
                           + $"Was {compClass.Name} but should have been a {typeof(TComp).Name} or subclass.";
        }
    }
}