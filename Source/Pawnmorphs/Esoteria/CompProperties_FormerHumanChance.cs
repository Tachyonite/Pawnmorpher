using Verse;

namespace Pawnmorph
{
    public class CompProperties_FormerHumanChance : CompProperties
    {
        public float chance = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().formerChance;

        public CompProperties_FormerHumanChance()
        {
            compClass = typeof(CompFormerHumanChance);
        }
    }
}
