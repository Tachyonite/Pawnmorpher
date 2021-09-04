using System;
using System.Collections.Generic;
using Pawnmorph.ThingComps;
using Verse;

namespace Pawnmorph
{
    // Obsoleted because of a name change and moving to the appropriate namespace

    [Obsolete("Use " + nameof(Comp_FormerHumanChance) + " instead.")]
    public class CompFormerHumanChance : Comp_FormerHumanChance { }

    [Obsolete("Use " + nameof(Comp_AlwaysFormerHuman) + " instead")]
    public class CompAlwaysFormerHuman : Comp_AlwaysFormerHuman { }


    [Obsolete("Use " + nameof(ThingComps.CompProperties_FormerHumanChance) + " instead.")]
    public class CompProperties_FormerHumanChance : ThingComps.CompProperties_FormerHumanChance
    {
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var err in base.ConfigErrors(parentDef))
                yield return err;
            yield return "Pawnmorph.CompProperties_FormerHumanChance is obsolete!" +
            	" Use Pawnmorph.ThingComps.CompProperties_FormerHumanChance instead." +
            	" This comp may be removed in the future!";
        }
    }

    [Obsolete("Use " + nameof(ThingComps.CompProperties_AlwaysFormerHuman) + " instead.")]
    public class CompProperties_AlwaysFormerHuman : ThingComps.CompProperties_AlwaysFormerHuman
    {
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var err in base.ConfigErrors(parentDef))
                yield return err;
            yield return "Pawnmorph.CompProperties_AlwaysFormerHuman is obsolete!" +
            	" Use Pawnmorph.ThingComps.CompProperties_AlwaysFormerHuman instead." +
            	" This comp may be removed in the future!";
        }
    }
}