// RemoveFromPartComp.cs modified by Iron Wolf for Pawnmorph on 09/25/2019 5:42 PM
// last updated 09/25/2019  5:42 PM

using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Utils;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     comp that removes other mutations when it's parent is added
    /// </summary>
    /// <seealso>
    ///     <cref>Pawnmorph.Utilities.HediffCompBase{Pawnmorph.Hediffs.RemoveFromPartCompProperties}</cref>
    /// </seealso>
    public class RemoveFromPartComp : HediffCompBase<RemoveFromPartCompProperties>
    {
        [NotNull] private readonly List<Hediff> _rmCache = new List<Hediff>();

        private int _addedTick = -1;

        /// <summary>
        ///     Gets the tick (time) this comp was added.
        /// </summary>
        /// <value>
        ///     The added tick.
        /// </value>
        public int AddedTick => _addedTick;

        /// <summary>
        ///     Gets the layer.
        /// </summary>
        /// <value>
        ///     The layer.
        /// </value>
        public MutationLayer Layer => Props.layer;

        /// <summary>
        ///     exposes all data for this comp.
        /// </summary>
        public override void CompExposeData()
        {
            Scribe_Values.Look(ref _addedTick, "addedTick", -1, true);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && _addedTick == -1) _addedTick = Find.TickManager.TicksAbs;
        }

        /// <summary>
        ///     called after this comp is created .
        /// </summary>
        public override void CompPostMake()
        {
            base.CompPostMake();

            _addedTick = Current.ProgramState != ProgramState.Playing ? 0 : Find.TickManager.TicksAbs;
        }

        /// <summary>
        ///     called after this instance was added to the pawn.
        /// </summary>
        /// <param name="dinfo">The damage info.</param>
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            RemoveOtherMutations();
        }

        /// <summary>
        ///     called every tick after the parent is updated.
        /// </summary>
        /// <param name="severityAdjustment">The severity adjustment.</param>
        public override void CompPostTick(ref float severityAdjustment)
        {
           
            if (_rmCache.Count == 0)
                return; //remove hediffs one at a time to not trigger exceptions about invalidating hediff set's internal enumerator 
            Hediff hm = _rmCache[0];
            _rmCache.RemoveAt(0);
            if (Pawn.health.hediffSet.hediffs.Contains(hm))
            {
                Pawn.health.RemoveHediff(hm);
            }
        }


        private void RemoveOtherMutations()
        {
            foreach (Hediff_AddedMutation otherHediff in Pawn
                                                        .health.hediffSet.hediffs.OfType<Hediff_AddedMutation>()
                                                        .Where(m => m != parent && m.Part == parent.Part))
            {
                if (_rmCache.Contains(otherHediff)) continue;
                var oComp = otherHediff.TryGetComp<RemoveFromPartComp>();
                if (oComp == null) continue; //the hediffs must have this comp to 

                if (oComp._addedTick > _addedTick)
                    continue; //the part to be removed must be older or the same age as this comp 
                if (oComp.Props.layer == Props.layer) 
                    _rmCache.Add(otherHediff);
            }
        }
    }

    /// <summary>
    ///     properties for the comp that removes other mutations when it's parent is added
    /// </summary>
    /// <seealso>
    ///     <cref>Pawnmorph.Utilities.HediffCompPropertiesBase{Pawnmorph.Hediffs.RemoveFromPartComp}</cref>
    /// </seealso>
    public class RemoveFromPartCompProperties : HediffCompPropertiesBase<RemoveFromPartComp>
    {
        /// <summary>
        ///     The layer to check for mutations
        /// </summary>
        public MutationLayer layer;
    }

    /// <summary>
    ///     flags representing the 'layer' a mutation resides on
    /// </summary>
    public enum MutationLayer
    {

        /// <summary>
        ///     the mutation affects the core of the part
        /// </summary>
        Core = 1,

        /// <summary>
        ///     mutation affects the surface of a part
        /// </summary>
        Skin = 2
    }
}