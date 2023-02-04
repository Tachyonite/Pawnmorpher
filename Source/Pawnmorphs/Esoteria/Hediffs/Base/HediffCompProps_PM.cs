#nullable enable

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
    public abstract class HediffCompProps_PM : HediffCompProperties { }
    
    /// <summary>
    /// Properties for <see cref="HediffComp_PM{TComp,TProps}"/>.  This is the same as HediffCompProperties but additionally
    /// is able to perform compile-time verification to ensure that it's only used with the correct HediffComp_PM.
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
        /// Creates a new instance of this class
        /// </summary>
        protected HediffCompProps_PM()
        {
            compClass = typeof(TComp);
        }
    }
}