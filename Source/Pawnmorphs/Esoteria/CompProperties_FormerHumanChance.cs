using Verse;

namespace Pawnmorph
{
    public class CompProperties_FormerHumanChance : CompProperties
    {
        public float Chance
        {
            get
            {
                return LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().formerChance;
            }
        }

        public CompProperties_FormerHumanChance()
        {
            compClass = typeof(CompFormerHumanChance);
        }
    }
}
