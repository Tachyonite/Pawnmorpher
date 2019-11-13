// RemoveFromPartComp.cs modified by Iron Wolf for Pawnmorph on 09/25/2019 5:42 PM
// last updated 09/25/2019  5:42 PM

using System;
using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// comp that removes other mutations when it's parent is added 
    /// </summary>
    /// <seealso>
    ///     <cref>Pawnmorph.Utilities.HediffCompBase{Pawnmorph.Hediffs.RemoveFromPartCompProperties}</cref>
    /// </seealso>
    public class RemoveFromPartComp : HediffCompBase<RemoveFromPartCompProperties>
    {
        private int _addedTick = -1 ;

        /// <summary>
        /// Gets the tick (time) this comp was added.
        /// </summary>
        /// <value>
        /// The added tick.
        /// </value>
        public int AddedTick => _addedTick;
        /// <summary>
        /// Gets the layer.
        /// </summary>
        /// <value>
        /// The layer.
        /// </value>
        public MutationLayers Layer => Props.layers;

        /// <summary>
        /// exposes all data for this comp.
        /// </summary>
        public override void CompExposeData()
        {
            Scribe_Values.Look(ref _addedTick, "addedTick", -1, true);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && _addedTick == -1)
            {
                _addedTick = Find.TickManager.TicksAbs;
            }
        }
        /// <summary>
        /// called after this comp is created .
        /// </summary>
        public override void CompPostMake()
        {
            base.CompPostMake();

            _addedTick = Current.ProgramState != ProgramState.Playing ? 0 : Find.TickManager.TicksAbs;
        }

        private List<Hediff> _rmCache;

        /// <summary>
        /// called after this instance was added to the pawn.
        /// </summary>
        /// <param name="dinfo">The damage info.</param>
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            bool FilterFunc(Hediff hediff)
            {
                if (hediff == parent) return false; //don't remove the parent 
                if (hediff.Part != parent.Part) return false; //remove only parts on the same part 
                if (!Props.hediffTypeFilter.PassesFilter(hediff.GetType())) return false; // get parts that match the filter 

                var oComp = hediff.TryGetComp<RemoveFromPartComp>();
                if (oComp == null)
                {
                    //Log.Message($"$$$$$$$$$$$$$$ {hediff.Label} does not have a RemoveFromPartComp");

                    return false; //the hediffs must have this comp to 
                }

                if (oComp._addedTick > _addedTick)
                    return false; //the part to be removed must be older or the same age as this comp 
                return (oComp.Props.layers & Props.layers) != 0; // the hediffs must be on the same layer(s) 


            }


           _rmCache = Pawn.health.hediffSet.hediffs.Where(FilterFunc).ToList();
           
        }

        /// <summary>
        /// called every tick after the parent is updated.
        /// </summary>
        /// <param name="severityAdjustment">The severity adjustment.</param>
        public override void CompPostTick(ref float severityAdjustment)
        {
            if ((_rmCache?.Count ?? 0) == 0) return; //remove hediffs one at a time to not trigger exceptions about invalidating hediff set's internal enumerator 
            var hm = _rmCache[0];
            _rmCache.RemoveAt(0);
            Pawn.health.RemoveHediff(hm); 
        }
    }

    /// <summary>
    /// properties for the comp that removes other mutations when it's parent is added 
    /// </summary>
    /// <seealso>
    ///     <cref>Pawnmorph.Utilities.HediffCompPropertiesBase{Pawnmorph.Hediffs.RemoveFromPartComp}</cref>
    /// </seealso>
    public class RemoveFromPartCompProperties : HediffCompPropertiesBase<RemoveFromPartComp>
    {
        /// <summary>
        /// The layer to check for mutations 
        /// </summary>
        public MutationLayers layers;
        /// <summary>
        /// The optional hediff type filter
        /// </summary>
        public Filter<Type> hediffTypeFilter = new Filter<Type>();  
    }

    /// <summary>
    /// flags representing the 'layer' a mutation resides on 
    /// </summary>
    [Flags]
    public enum MutationLayers
    {
        /// <summary>
        /// is not a mutation 
        /// </summary>
        None = 0,
        /// <summary>
        /// the mutation affects the core of the part 
        /// </summary>
        Core = 1,
        /// <summary>
        /// mutation affects the surface of a part 
        /// </summary>
        Skin = 2
    }
}