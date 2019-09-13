// MorphGraphicsUtils.cs created by Iron Wolf for Pawnmorph on 09/13/2019 9:50 AM
// last updated 09/13/2019  9:51 AM

using System;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.GraphicSys
{
    /// <summary>
    /// collection of useful graphics related utility functions on morphs 
    /// </summary>
    public static class MorphGraphicsUtils
    {

        /// <summary>
        /// refresh the graphics associated with this pawn, including the portraits if it's a colonist 
        /// </summary>
        /// <param name="pawn"></param>
        public static void RefreshGraphics([NotNull] this Pawn pawn)
        {
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            if (pawn.IsColonist)
            {
                PortraitsCache.SetDirty(pawn);

            }
        }

        public static Color? GetSkinColorOverride([NotNull] this MorphDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            return def.raceSettings?.graphicsSettings?.skinColorOverride;
        }

        public static Color? GetHairColorOverride([NotNull] this MorphDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            return def.raceSettings?.graphicsSettings?.hairColorOverride; 
        }

        public static Color? GetHairColorOverrideSecond([NotNull] this MorphDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            return def.raceSettings?.graphicsSettings?.hairColorOverrideSecond; 
        }

        public static Color? GetSkinColorSecondOverride([NotNull] this MorphDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            return def.raceSettings?.graphicsSettings?.skinColorOverrideSecond; 
        }


    }
}