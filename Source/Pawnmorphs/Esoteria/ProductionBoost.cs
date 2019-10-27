using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
    public class ProductionBoost
    {
        public Filter<HediffDef> hediffFilter = new Filter<HediffDef>();
        public float productionBoost; //is a increase/decrease in production Hediff's severity

        public float GetBoost(HediffDef hediff)
        {
            return hediffFilter.PassesFilter(hediff) ? productionBoost : 0;
        }
    }
}
