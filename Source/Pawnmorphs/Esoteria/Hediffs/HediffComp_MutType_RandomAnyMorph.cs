using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// A HediffComp_MutationType that picks a completely random morph def and
    /// then returns mutations and TFs from that def.
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.HediffComp_MutTypeBase"/>
    /// <seealso cref="Pawnmorph.Hediffs.HediffComp_MutTypeBase_Dynamic"/>
    public class HediffComp_MutType_RandomAnyMorph : HediffComp_MutTypeBase_Dynamic
    {
        public class Properties : HediffCompPropertiesBase<HediffComp_MutType_RandomAnyMorph>
        {
            [UsedImplicitly] public bool allowRestricted = false;
        }

        public Properties Props => (Properties)props;

        /// <summary>
        /// Gets the morph def.
        /// </summary>
        /// <returns>The morph def.</returns>
        protected override MorphDef GetMorphDef()
        {
            return DefDatabase<MorphDef>.AllDefs
                    .Where(d => Props.allowRestricted || !d.Restricted)
                    .RandomElement();
        }
    }
}
