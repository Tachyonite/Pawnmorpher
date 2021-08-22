using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// A HediffComp_MutType that picks a random morph def from a class and then
    /// returns mutations and TFs from that def.
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.HediffComp_MutTypeBase"/>
    /// <seealso cref="Pawnmorph.Hediffs.HediffComp_MutTypeBase_Dynamic"/>
    public class HediffComp_MutType_RandomClassMorph : HediffComp_MutTypeBase_Dynamic
    {
        public class Properties : HediffCompPropertiesBase<HediffComp_MutType_RandomAnyMorph>
        {
            [UsedImplicitly] public AnimalClassDef animalClassDef;
            [UsedImplicitly] public bool allowRestricted;

            public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
            {
                foreach (var error in base.ConfigErrors(parentDef))
                    yield return error;
                if (animalClassDef == null)
                    yield return "HediffComp_MutType_RandomClassMorph animalClassDef is null!";
            }
        }

        public Properties Props => (Properties)props;

        /// <summary>
        /// Gets the morph def.
        /// </summary>
        /// <returns>The morph def.</returns>
        protected override MorphDef GetMorphDef()
        {
            return Props.animalClassDef.GetAllMorphsInClass()
                    .Where(d => Props.allowRestricted || !d.Restricted)
                    .RandomElement();
        }
    }
}
