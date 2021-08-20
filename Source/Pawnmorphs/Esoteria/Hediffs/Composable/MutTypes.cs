using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines which mutations to add
    /// </summary>
    public abstract class MutTypes
    {
        /// <summary>
        /// Gets the list of available mutations.
        /// </summary>
        /// <returns>The mutations.</returns>
        /// <param name="hediff">Hediff.</param>
        public abstract IEnumerable<MutationEntry> GetMutations(Hediff_MutagenicBase hediff);
    }

    /// <summary>
    /// A simple MutTypes that returns ALL THE MUTATIONS _O/
    /// Good for chaotic mutations.
    /// </summary>
    public class MutTypes_All : MutTypes
    {
        // Low chance by default for any one mutation
        [UsedImplicitly] float chance = 0.1f;

        public override IEnumerable<MutationEntry> GetMutations(Hediff_MutagenicBase hediff)
        {
            return DefDatabase<MutationDef>.AllDefs
                    .Select(m => MutationEntry.FromMutation(m, chance));
        }
    }

    /// <summary>
    /// A simple MutTypes that accepts a list of mutations directly from the XML
    /// </summary>
    public class MutTypes_List : MutTypes
    {
        [UsedImplicitly] List<MutationDef> mutations;
        [UsedImplicitly] float chance = 1f;

        public override IEnumerable<MutationEntry> GetMutations(Hediff_MutagenicBase hediff)
        {
            return mutations.Select(m => MutationEntry.FromMutation(m, chance));
        }
    }

    /// <summary>
    /// A simple MutTypes that selects all mutations from a morph def
    /// </summary>
    public class MutTypes_Morph : MutTypes
    {
        [UsedImplicitly] MorphDef morphDef;
        [UsedImplicitly] float chance = 1f;

        public override IEnumerable<MutationEntry> GetMutations(Hediff_MutagenicBase hediff)
        {
            return morphDef.AllAssociatedMutations
                    .Select(m => MutationEntry.FromMutation(m, chance));
        }
    }

    /// <summary>
    /// A simple MutTypes that selects all mutations from a class (including child classes)
    /// </summary>
    public class MutTypes_Class : MutTypes
    {
        [UsedImplicitly] AnimalClassDef classDef;
        [UsedImplicitly] float chance = 1f;

        public override IEnumerable<MutationEntry> GetMutations(Hediff_MutagenicBase hediff)
        {
            return classDef.GetAllMutationIn()
                    .Select(m => MutationEntry.FromMutation(m, chance));
        }
    }

    /// <summary>
    /// A MutTypes that selects mutations defined in HediffComp_MutagenicTypes
    /// 
    /// Most "dynamic" hediffs that want to share mutation data across stages will
    /// want to use this MutTypes, as MutTypes are stateless.
    /// </summary>
    public class MutTypes_FromComp : MutTypes
    {
        [UsedImplicitly] float chance = 1f;

        public override IEnumerable<MutationEntry> GetMutations(Hediff_MutagenicBase hediff)
        {
            return hediff.TryGetComp<HediffComp_MutationType>()
                    .GetMutations()
                    .Select(m => MutationEntry.FromMutation(m, chance));
        }
    }
}
