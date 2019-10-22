using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// properties for the former human chance prop 
    /// </summary>
    /// <seealso cref="Verse.CompProperties" />
    public class CompProperties_FormerHumanChance : CompProperties

    {
        /// <summary>
        /// Gets the chance to add the former human hediff.
        /// </summary>
        /// <value>
        /// The chance.
        /// </value>
        public float Chance => LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().formerChance;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompProperties_FormerHumanChance"/> class.
        /// </summary>
        public CompProperties_FormerHumanChance()
        {
            compClass = typeof(CompFormerHumanChance);
        }
    }
}
