// MutationHediffExtension.cs created by Iron Wolf for Pawnmorph on 09/15/2019 8:44 PM
// last updated 09/15/2019  8:44 PM

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// mod def extension for applying to hediffs to manually assign them a part without using a hediff giver 
    /// </summary>
    /// Note, the results will be additive, so if a mutation has this extension and is in one or more hediff givers, the total set
    /// of parts it can be applied to is the union of this extension and the givers 
    public class MutationHediffExtension : DefModExtension
    {
        public List<BodyPartDef> parts = new List<BodyPartDef>();
        public List<MutationCategoryDef> categories = new List<MutationCategoryDef>(); 
    }
}