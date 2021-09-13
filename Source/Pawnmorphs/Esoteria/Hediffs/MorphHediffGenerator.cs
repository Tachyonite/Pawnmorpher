// MorphHediffGenerator.cs created by Iron Wolf for Pawnmorph on 09/13/2021 7:25 AM
// last updated 09/13/2021  7:25 AM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     static class responsible for generating implicit morph injector hediffs
    /// </summary>
    public static class MorphHediffGenerator
    {
        [NotNull] private static readonly List<HediffDef> _allGeneratedHediffs = new List<HediffDef>();

        static string MorphHediffName(string morphName)
        {
            return "Pawnmorph" + morphName + "TF"; 
        }

        static string MorphHediffNamePartial(string morphName)
        {
            return MorphHediffName(morphName) + "Partial";
        }
        static HediffDef GenerateFullHediffFor([NotNull] MorphDef morphDef)
        {
            HediffDef newDef = new HediffDef()
            {
                defName = MorphHediffName(morphDef.defName),
                label = morphDef.label,
                hediffClass = typeof(Hediff_MutagenicBase)
            };

            throw new NotImplementedException();

        }
    }
}