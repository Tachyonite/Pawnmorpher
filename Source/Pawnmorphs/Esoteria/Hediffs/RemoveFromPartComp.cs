// RemoveFromPartComp.cs modified by Iron Wolf for Pawnmorph on 09/25/2019 5:42 PM
// last updated 09/25/2019  5:42 PM

using System;
using System.Linq;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
    public class RemoveFromPartComp : HediffCompBase<RemoveFromPartCompProperties>
    {
        private int _addedTick = -1 ;
        public override void CompExposeData()
        {
            Scribe_Values.Look(ref _addedTick, "addedTick", -1, true);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && _addedTick == -1)
            {
                _addedTick = Find.TickManager.TicksAbs;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            bool FilterFunc(Hediff hediff)
            {
                if (hediff == parent) return false; //don't remove the parent 
                if (hediff.Part != parent.Part) return false; //remove only parts on the same part 
                if (!(Props.hediffTypeFilter.PassesFilter(hediff.GetType()))) return false; // get parts that match the filter 

                var oComp = hediff.TryGetComp<RemoveFromPartComp>();
                if (oComp == null) return false; //the hediffs must have this comp to 

                if (oComp._addedTick <= _addedTick) return false; //the part to be removed must be older or the same age as this comp 
                return (oComp.Props.layers & Props.layers) != 0;  // the hediffs must be on the same layer(s) 


            }


            var allHediffs = Pawn.health.hediffSet.hediffs.Where(FilterFunc);
            foreach (Hediff allHediff in allHediffs)
            {
                Pawn.health.RemoveHediff(allHediff); 
            }
            
        }
    }

    public class RemoveFromPartCompProperties : HediffCompPropertiesBase<RemoveFromPartComp>
    {
        public MutationLayers layers;
        public Filter<Type> hediffTypeFilter = new Filter<Type>();  
    }

    /// <summary>
    /// flags representing the 'layer' a mutation resides on 
    /// </summary>
    [Flags]
    public enum MutationLayers
    {
        None = 0,
        Core = 1 ,
        Skin = 1 << 1
    }
}