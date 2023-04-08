#nullable enable

using System;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// A base class for Pawnmorpher hediff comps.  This class is very similar to <see cref="Verse.HediffComp"/> but notably
    /// does not have an unsealed Tick() method.  Instead, a number of TickX() methods are supplied to run logic once every 60,
    /// 2500, or 60000 ticks.
    /// <br/>
    /// This is a performance optimization.  Pawnmorpher adds a large number of hediffs, and to keep performance high the amount
    /// of code executed every tick should be minimized.  Most hediff comps should be implementable with a tick that runs at most
    /// once a second, but if you need to implement logic that runs every game tick anyway you should use a regular HediffComp
    /// instead.
    /// <br/>
    /// Note that the <see cref="HediffComp_PM{TComp,TProps}"/> subclass knows its own CompProperties type and is able to perform
    /// compile-time checking to ensure the correct CompProperties is used.  You should prefer subclassing that class instead of
    /// this one directly.  This class exists primarily to serve as a non-generic base class for all Pawnmorpher hediff comps. 
    /// </summary>
    /// <seealso cref="HediffComp_PM{TComp,TProps}"/>
    public abstract class HediffComp_PM : HediffComp
    {
        [Unsaved] private Hediff_PM? _parent;

        /// <summary>
        /// The hediff this comp belongs to.
        /// This version is cast to Hediff_PM automatically.
        /// </summary>
        /// <exception cref="InvalidCastException">If the parent hediff is not a subclass of Hediff_PM</exception>
        public Hediff_PM Parent => _parent ??= parent as Hediff_PM
                                            ?? throw new
                                                   InvalidCastException($"{Def?.defName} was not attached to a Pawnmorpher hediff!"
                                                                      + $" Hediff {parent?.def?.defName} should have been a subclass of"
                                                                      + $" Hediff_PM but was instead a {parent?.GetType().Name}");

        /// <summary>
        /// Called once a game tick, but only if this PMComp is attached as a regular comp. All this method does is replicate the
        /// behavior <see cref="Hediff_PM{THediff,TDef}.PostTick()"/> when this is used as a regular comp.
        /// <br />
        /// This method is sealed because it does not normally get called at all.  It exists only for backwards compatibility if a
        /// PMComp is used as a regular comp instead of a PM comp.  If you need to do something every tick, you should create a
        /// normal <see cref="Verse.HediffComp"/> instead.
        /// </summary>
        /// <param name="severityAdjustment"></param>
        public sealed override void CompPostTick(ref float severityAdjustment)
        {
            Log.WarningOnce($"[Pawnmorpher] Comp {GetType().Name} is a PMComp but is still being used as a regular Comp on"
                          + $" Hediff {parent?.def?.defName}. It should be set as a PMComp for improved performance.",
                            Gen.HashCombineInt(4444, parent?.loadID ?? 0));

            int hashOffsetTick = Find.TickManager!.TicksGame + Pawn?.HashOffset() ?? 0;

            if (hashOffsetTick % 60 == 0) // Every real-life second
                TickSecond(ref severityAdjustment);

            if (hashOffsetTick % 2500 == 0) // Once an in-game hour
                TickHour(ref severityAdjustment);

            if (hashOffsetTick % 60000 == 0) // Once an in-game day
                TickDay(ref severityAdjustment);
        }

        /// <summary>
        /// Called once every real-time second (every 60 game ticks). This happens 1000 times per Rimworld day.
        /// Each call is exactly 1 second apart, but the exact moment it is called is offset by the pawn hash and so is different
        /// for every pawn.
        /// </summary>
        /// <param name="severityAdjustment">The severity will be adjusted by this amount after all comps have ticked</param>
        public virtual void TickSecond(ref float severityAdjustment)
        {
        }

        /// <summary>
        /// Called once every Rimworld hour (every 2500 game ticks). This happens 24 times per Rimworld day.
        /// Each call is exactly 1 hour apart, but the exact moment it is called is offset by the pawn hash and so is different
        /// for every pawn. 
        /// </summary>
        /// <param name="severityAdjustment">The severity will be adjusted by this amount after all comps have ticked</param>
        public virtual void TickHour(ref float severityAdjustment)
        {
        }

        /// <summary>
        /// Called once every Rimworld day (every 60000 game ticks).
        /// Each call is exactly 1 day apart, but the exact moment it is called is offset by the pawn hash and so is different
        /// for every pawn.
        /// </summary>
        /// <param name="severityAdjustment">The severity will be adjusted by this amount after all comps have ticked</param>
        public virtual void TickDay(ref float severityAdjustment)
        {
        }
    }

    /// <summary>
    /// A subclass of <see cref="HediffComp_PM"/> that knows its own type and its expected properties type and can
    /// perform compile-time verification to ensure the correct property object is being used.
    /// <br/>
    /// See https://en.wikipedia.org/wiki/Bounded_quantification#F-bounded_quantification if your want to understand what the type
    /// bounds are doing doing.
    /// </summary>
    /// <example>
    /// Define your classes like so:
    /// <code>
    /// class MyComp : HediffComp_PM{MyComp, MyCompProps}
    /// class MyCompProps : HediffCompProps_PM{MyComp, MyCompProps}
    /// </code>
    /// </example>
    /// <typeparam name="TComp">The concrete type of the HediffComp_PM</typeparam>
    /// <typeparam name="TProps">The concrete type of the HediffCompProps_PM</typeparam>
    public abstract class HediffComp_PM<TComp, TProps> : HediffComp_PM
        where TComp : HediffComp_PM<TComp, TProps>
        where TProps : HediffCompProps_PM<TComp, TProps>
    {
        [Unsaved] private TProps? _props;

        /// <summary>
        /// The <see cref="HediffCompProps_PM{TComp, TProps}"/> of this hediff comp, cast to the correct type
        /// </summary>
        /// <exception cref="InvalidCastException">If the comp properties object were not of the correct type</exception>
        public TProps Props => _props ??= props as TProps
                                       ?? throw new InvalidCastException($"HediffCompProps for {GetType().Name} had"
                                                                       + $" incorrect type! Should have been {typeof(TProps).Name}"
                                                                       + $" but was instead {props?.GetType().Name}");
    }
}