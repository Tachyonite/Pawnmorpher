using System;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines what the chance of a full transformation is
    /// </summary>
    public abstract class TFChance
    {
        /// <summary>
        /// Whether or not to transform the pawn.  Checked only upon entering a stage.
        /// </summary>
        /// <param name="hediff">The hediff doing the transformation.</param>
        public abstract bool ShouldTransform(Hediff_MutagenicBase hediff);
    }

    /// <summary>
    /// A simple TFChance class that just always transforms the pawn
    /// </summary>
    public class TFChance_Always : TFChance
    {
        /// <summary>
        /// Whether or not to transform the pawn.  Checked only upon entering a stage.
        /// </summary>
        /// <param name="hediff">The hediff doing the transformation.</param>
        public override bool ShouldTransform(Hediff_MutagenicBase hediff)
        {
            return true;
        }
    }

    /// <summary>
    /// A TFChance class that transforms the pawn with a random chance specified in the XML
    /// Also affected by the TransformationSensitivity stat, unless disabled
    /// </summary>
    public class TFChance_Random : TFChance
    {
        /// <summary>
        /// The chance of a transformation.
        /// </summary>
        [UsedImplicitly] public float chance;

        /// <summary>
        /// Whether or not transformation sensitivity is respected.
        /// If true, the chance will be multiplied by the sensitivity stat
        /// </summary>
        [UsedImplicitly] public bool respectSensitivity = true;

        /// <summary>
        /// Whether or not to transform the pawn.  Checked only upon entering a stage.
        /// </summary>
        /// <param name="hediff">The hediff doing the transformation.</param>
        public override bool ShouldTransform(Hediff_MutagenicBase hediff)
        {
            float tfChance = chance;

            if (respectSensitivity)
                tfChance *= hediff.pawn.GetStatValue(PMStatDefOf.TransformationSensitivity) / 100f;

            tfChance = Mathf.Clamp(tfChance, 0f, 1f);
            return Rand.Value < tfChance;
        }
    }

    /// <summary>
    /// A TFChance class that transforms the pawn with a random chance based on the full-TF setting
    /// Also affected by the TransformationSensitivity stat, unless disabled
    /// </summary>
    public class TFChance_BySetting : TFChance
    {
        /// <summary>
        /// Whether or not transformation sensitivity is respected.
        /// If true, the chance will be multiplied by the sensitivity stat
        /// </summary>
        [UsedImplicitly] public bool respectSensitivity = true;

        /// <summary>
        /// Whether or not to transform the pawn.  Checked only upon entering a stage.
        /// </summary>
        /// <param name="hediff">The hediff doing the transformation.</param>
        public override bool ShouldTransform(Hediff_MutagenicBase hediff)
        {
            float tfChance = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance;

            if (respectSensitivity)
                tfChance *= hediff.pawn.GetStatValue(PMStatDefOf.TransformationSensitivity) / 100;

            tfChance = Mathf.Clamp(tfChance, 0f, 1f);
            return Rand.Value < tfChance;
        }
    }
}
