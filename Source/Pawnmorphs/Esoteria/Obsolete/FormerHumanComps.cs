using System;
using System.Collections.Generic;
using Pawnmorph.ThingComps;
using Verse;

namespace Pawnmorph
{
    // Obsoleted because they were merged together and moving to the appropriate namespace

    [Obsolete("Use " + nameof(Comp_CanBeFormerHuman) + " instead.")]
    public class CompFormerHumanChance : Comp_CanBeFormerHuman
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Log.Error("Pawnmorph.CompAlwaysFormerHuman is obsolete!" +
                " Use " + nameof(Comp_CanBeFormerHuman) + " instead." +
                " This comp may be removed in the future!");
        }
    }

    [Obsolete("Use " + nameof(Comp_CanBeFormerHuman) + " instead")]
    public class CompAlwaysFormerHuman : Comp_CanBeFormerHuman
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Log.Error("Pawnmorph.CompAlwaysFormerHuman is obsolete!" +
                " Use " + nameof(Comp_CanBeFormerHuman) + " instead." +
                " This comp may be removed in the future!");
        }
    }


    [Obsolete("Use " + nameof(CompProperties_CanBeFormerHuman) + " instead.")]
    public class CompProperties_FormerHumanChance : CompProperties_CanBeFormerHuman
    {
        public override bool Always => false;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var err in base.ConfigErrors(parentDef))
                yield return err;
            yield return "Pawnmorph.CompProperties_FormerHumanChance is obsolete!" +
                " Use " + nameof(CompProperties_CanBeFormerHuman) + " instead." +
                " This comp may be removed in the future!";
        }
    }

    [Obsolete("Use " + nameof(CompProperties_CanBeFormerHuman) + " instead.")]
    public class CompProperties_AlwaysFormerHuman : CompProperties_CanBeFormerHuman
    {
        public override bool Always => true;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var err in base.ConfigErrors(parentDef))
                yield return err;
            yield return "Pawnmorph.CompProperties_AlwaysFormerHuman is obsolete!" +
                " Use " + nameof(CompProperties_CanBeFormerHuman) + " instead." +
                " This comp may be removed in the future!";
        }
    }
}