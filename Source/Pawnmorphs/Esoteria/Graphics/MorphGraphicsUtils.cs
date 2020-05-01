// MorphGraphicsUtils.cs created by Iron Wolf for Pawnmorph on 09/13/2019 9:50 AM
// last updated 09/13/2019  9:51 AM

using System;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
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

            Color? aspectColor = pawn?.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>()?.ColorSet?.hairColor;
            if (aspectColor.HasValue)
                return aspectColor;

            if (def.ExplicitHybridRace == null) 
            {
                return def.raceSettings?.ColorGenerator?.GetHairColor(pawn)?.First ?? GetSkinColorOverride(def, pawn);
            }

            var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;
            ColorGenerator colorGenerator = hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienhaircolorgen;
            Color? color;
            if (pawn != null && colorGenerator != null)
                color = colorGenerator.NewRandomizedColorUsingSeed(pawn.thingIDNumber);
            else
                color = GetSkinColorOverride(def, pawn);


            return color;
        }

        /// <summary>
        ///     Gets the hair color override second.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">def</exception>
        public static Color? GetHairColorOverrideSecond([NotNull] this MorphDef def, Pawn pawn = null)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));

            Color? aspectColor = pawn?.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>()?.ColorSet?.hairColorTwo;
            if (aspectColor.HasValue)
                return aspectColor;

            if (def.ExplicitHybridRace == null)
            {
                return def.raceSettings?.ColorGenerator?.GetHairColor(pawn)?.Second ?? GetSkinColorOverride(def, pawn);
            }

            var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;
            ColorGenerator colorGenerator = hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienhairsecondcolorgen;
            Color? color;
            if (colorGenerator != null)
                color = colorGenerator.NewRandomizedColorUsingSeed(pawn.thingIDNumber);
            else
                color = GetSkinColorOverride(def, pawn);


            return color;
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

            Color? aspectColor = pawn?.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>()?.ColorSet?.skinColor;
            if (aspectColor.HasValue)
                return aspectColor;

            if (def.ExplicitHybridRace == null)
            {
                return def.raceSettings?.ColorGenerator?.GetSkinColor(pawn)?.First;
            }

            var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;
            ColorGenerator colorGenerator = hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienskincolorgen;
            Color? color;
            if (colorGenerator != null)
                color = colorGenerator.NewRandomizedColorUsingSeed(pawn.thingIDNumber);
            else
                color = null;

            return color;
        }

        /// <summary>
        ///     Gets the skin color second override.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">def</exception>
        public static Color? GetSkinColorSecondOverride([NotNull] this MorphDef def, Pawn pawn = null)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));

            Color? aspectColor = pawn?.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>()?.ColorSet?.skinColorTwo;
            if (aspectColor.HasValue)
                return aspectColor;

            if (def.ExplicitHybridRace == null)
            {
                return def.raceSettings?.ColorGenerator?.GetSkinColor(pawn)?.Second;
            }

            var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;
            ColorGenerator colorGenerator = hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienskinsecondcolorgen;

            Color? color;
            if (colorGenerator != null)
                color = colorGenerator.NewRandomizedColorUsingSeed(pawn.thingIDNumber);
            else
                color = null;

            return color;
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


        /// <summary>
        /// Gets the color of the skin.
        /// </summary>
        /// <param name="gGenerator">The g controller.</param>
        /// <param name="pawn"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">gController</exception>
        public static ColorChannel? GetSkinColor([NotNull] this IMorphColorGenerator gGenerator, [NotNull] Pawn pawn)
        {
            if (gGenerator == null) throw new ArgumentNullException(nameof(gGenerator));
            return gGenerator.GetChannel(pawn,"skin"); 
        }

        /// <summary>
        /// Gets the color of the hair.
        /// </summary>
        /// <param name="gGenerator">The g controller.</param>
        /// <param name="pawn"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">gController</exception>
        public static ColorChannel? GetHairColor([NotNull] this IMorphColorGenerator gGenerator, [NotNull] Pawn pawn)
        {
            if (gGenerator == null) throw new ArgumentNullException(nameof(gGenerator));
            return gGenerator.GetChannel(pawn,"hair"); 
        }

    }
}