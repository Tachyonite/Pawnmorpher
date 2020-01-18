// MorphGraphicsUtils.cs created by Iron Wolf for Pawnmorph on 09/13/2019 9:50 AM
// last updated 09/13/2019  9:51 AM

using System;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.GraphicSys
{
    /// <summary>
    ///     collection of useful graphics related utility functions on morphs
    /// </summary>
    public static class MorphGraphicsUtils
    {
        /// <summary>
        ///     Gets the hair color override.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">def</exception>
        public static Color? GetHairColorOverride([NotNull] this MorphDef def, Pawn pawn = null)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            HybridRaceSettings.GraphicsSettings gSettings = def.raceSettings?.graphicsSettings;

            if (def.explicitHybridRace == null)
            {
                Gender? gender = pawn?.gender;

                if (gender == Gender.Female && gSettings?.femaleHairColorOverride != null)
                    return gSettings.femaleHairColorOverride;

                return gSettings?.hairColorOverride ?? GetSkinColorOverride(def, pawn);
            }

            var hRace = def.explicitHybridRace as ThingDef_AlienRace;
            return hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienhaircolorgen?.NewRandomizedColor()
                ?? GetSkinColorOverride(def, pawn);
        }

        /// <summary>
        /// Gets the hair color override second.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">def</exception>
        public static Color? GetHairColorOverrideSecond([NotNull] this MorphDef def, Pawn pawn=null)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (def.explicitHybridRace == null)
            {
                HybridRaceSettings.GraphicsSettings gSettings = def.raceSettings?.graphicsSettings;
                if (pawn?.gender == Gender.Female && gSettings?.femaleHairColorOverrideSecond != null)
                {
                    return gSettings.femaleHairColorOverrideSecond;
                }

                return gSettings?.hairColorOverrideSecond ?? GetSkinColorSecondOverride(def, pawn);
            }
            var hRace = def.explicitHybridRace as ThingDef_AlienRace;
            return hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienhairsecondcolorgen?.NewRandomizedColor()
                ?? GetSkinColorSecondOverride(def, pawn);
        }

        /// <summary>
        ///     Gets the skin color override.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">def</exception>
        public static Color? GetSkinColorOverride([NotNull] this MorphDef def, Pawn pawn = null)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (def.explicitHybridRace == null)
            {
                HybridRaceSettings.GraphicsSettings raceGSettings = def.raceSettings?.graphicsSettings;
                Gender? gender = pawn?.gender;
                if (gender == Gender.Female && raceGSettings?.femaleSkinColorOverride != null)
                    return raceGSettings.femaleSkinColorOverride;

                return raceGSettings?.skinColorOverride;
            }

            var hRace = def.explicitHybridRace as ThingDef_AlienRace;
            return hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienskincolorgen?.NewRandomizedColor();
        }

        /// <summary>
        /// Gets the skin color second override.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">def</exception>
        public static Color? GetSkinColorSecondOverride([NotNull] this MorphDef def, Pawn pawn=null)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (def.explicitHybridRace == null)
            {
                HybridRaceSettings.GraphicsSettings gSettings = def.raceSettings?.graphicsSettings;

                if (pawn?.gender == Gender.Female && gSettings?.femaleSkinColorOverrideSecond != null)
                {
                    return gSettings.femaleSkinColorOverrideSecond;
                }

                return gSettings?.skinColorOverrideSecond;
            }
            var hRace = def.explicitHybridRace as ThingDef_AlienRace;
            return hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienskinsecondcolorgen?.NewRandomizedColor();
        }

        /// <summary>
        ///     refresh the graphics associated with this pawn, including the portraits if it's a colonist
        /// </summary>
        /// <param name="pawn"></param>
        public static void RefreshGraphics([NotNull] this Pawn pawn)
        {
            if (Current.ProgramState != ProgramState.Playing)
                return; //make sure we don't refresh the graphics while the game is loading
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            if (pawn.IsColonist) PortraitsCache.SetDirty(pawn);
        }
    }
}