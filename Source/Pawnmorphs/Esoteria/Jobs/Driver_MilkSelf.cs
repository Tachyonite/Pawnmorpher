using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Jobs
{
    /// <summary> Job driver to make humanoid pawns milk themselves using HediffComp_Production. </summary>
    public class Driver_MilkSelf : Driver_ProduceThing
    {
        [NotNull]
        private static readonly Dictionary<HediffDef, bool> _cache = new Dictionary<HediffDef, bool>();

        [NotNull]
        private static readonly List<HediffComp_Production> _cacheList = new List<HediffComp_Production>();

        static void FillList([NotNull] Hediff hDiff)
        {
            if (_cache.TryGetValue(hDiff.def, out bool hasComp))
            {
                if (!hasComp) return;
            }

            if (hDiff is HediffWithComps hComps)
            {
                foreach (HediffComp_Production prodComp in hComps.comps.MakeSafe().OfType<HediffComp_Production>())
                {
                    hasComp = true;

                    if (prodComp.CanProduceNow) _cacheList.Add(prodComp); 

                }

                _cache[hDiff.def] = hasComp; 
            }
            else
            {
                _cache[hDiff.def] = false; 
            }
        }

        /// <summary>
        /// Produce whatever resources this driver is producing.
        /// </summary>
        public override void Produce()
        {



            HediffSet hediffSet = pawn?.health?.hediffSet;
            if (hediffSet == null) return;

            _cacheList.Clear();
            foreach (Hediff hediffSetHediff in hediffSet.hediffs)
            {
                if(hediffSetHediff == null) continue;
                FillList(hediffSetHediff); 
            }

            foreach (HediffComp_Production hediffCompProduction in _cacheList)
            {
                hediffCompProduction.Produce();
            }

        }
    }
}
