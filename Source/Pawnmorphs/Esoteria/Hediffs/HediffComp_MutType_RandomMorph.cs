using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// A HediffComp_MutationType that picks a random morph def from a list, and
    /// then returns mutations and TFs from that def
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.Composable.MutTypes_FromComp"/>
    /// <seealso cref="Pawnmorph.Hediffs.Composable.TFTypes_FromComp"/>
    public class HediffComp_MutType_RandomMorph : HediffComp_MutTypeBase_Dynamic
    {
        public class Properties : HediffCompPropertiesBase<HediffComp_MutType_RandomMorph>
        {
            [UsedImplicitly] public List<MorphDef> morphDefs;

            public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
            {
                foreach (var error in base.ConfigErrors(parentDef))
                    yield return error;
                if (morphDefs == null)
                    yield return "HediffComp_MutationType_RandomMorph morphDefs is null!";
            }
        }

        public Properties Props => (Properties)props;

        protected override MorphDef GetMorphDef()
        {
            return Props.morphDefs.RandomElement();
        }
    }
}
