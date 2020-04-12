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

            if (pawn != null)
            {
                Aspects.Coloration colorationAspect = pawn.GetAspectTracker()?.GetAspect(typeof(Aspects.Coloration)) as Aspects.Coloration;
                if(colorationAspect != null) 
                {
                    Color? aspectColor = colorationAspect.TryGetColorationAspectColor(Aspects.Coloration.PawnColorSlot.HairFirst);
                    if (aspectColor.HasValue)
                        return aspectColor;
                }
            }

            HybridRaceSettings.GraphicsSettings gSettings = def.raceSettings?.graphicsSettings;

            if (def.ExplicitHybridRace == null)
            {
                Gender? gender = pawn?.gender;

                if (gender == Gender.Female && gSettings?.femaleHairColorOverride != null)
                    return gSettings.femaleHairColorOverride;

                return gSettings?.hairColorOverride ?? GetSkinColorOverride(def, pawn);
            }

            var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;
            ColorGenerator colorGenerator = hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienhaircolorgen;
            Color? color;
            if (colorGenerator != null)
                try
                {
                    if (pawn != null)
                        Rand.PushState(pawn.thingIDNumber);
                    color = colorGenerator.NewRandomizedColor();
                }
                finally
                {
                    if (pawn != null) Rand.PopState();
                }
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

            if (pawn != null)
            {
                Aspects.Coloration colorationAspect = pawn.GetAspectTracker()?.GetAspect(typeof(Aspects.Coloration)) as Aspects.Coloration;
                if (colorationAspect != null)
                {
                    Color? aspectColor = colorationAspect.TryGetColorationAspectColor(Aspects.Coloration.PawnColorSlot.HairSecond);
                    if (aspectColor.HasValue)
                        return aspectColor;
                }
            }

            if (def.ExplicitHybridRace == null)
            {
                HybridRaceSettings.GraphicsSettings gSettings = def.raceSettings?.graphicsSettings;
                if (pawn?.gender == Gender.Female && gSettings?.femaleHairColorOverrideSecond != null)
                    return gSettings.femaleHairColorOverrideSecond;

                return gSettings?.hairColorOverrideSecond ?? GetSkinColorSecondOverride(def, pawn);
            }

            var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;
            ColorGenerator colorGenerator = hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienhairsecondcolorgen;
            Color? color;
            if (colorGenerator != null)
                try
                {
                    if (pawn != null) Rand.PushState(pawn.thingIDNumber);

                    color = colorGenerator.NewRandomizedColor();
                }
                finally
                {
                    if (pawn != null) Rand.PopState();
                }
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

            if(pawn != null)
            {
                Aspects.Coloration colorationAspect = pawn.GetAspectTracker()?.GetAspect(typeof(Aspects.Coloration)) as Aspects.Coloration;
                if (colorationAspect != null)
                {
                    Color? aspectColor = colorationAspect.TryGetColorationAspectColor(Aspects.Coloration.PawnColorSlot.SkinFirst);
                    if (aspectColor.HasValue)
                        return aspectColor;
                }
            }

            if (def.ExplicitHybridRace == null)
            {
                HybridRaceSettings.GraphicsSettings raceGSettings = def.raceSettings?.graphicsSettings;
                Gender? gender = pawn?.gender;
                if (gender == Gender.Female && raceGSettings?.femaleSkinColorOverride != null)
                    return raceGSettings.femaleSkinColorOverride;

                return raceGSettings?.skinColorOverride;
            }

            var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;
            ColorGenerator colorGenerator = hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienskincolorgen;
            Color? color;
            if (colorGenerator != null)
                try
                {
                    if (pawn != null) Rand.PushState(pawn.thingIDNumber);
                    color = colorGenerator.NewRandomizedColor();
                }
                finally
                {
                    if (pawn != null) Rand.PopState();
                }
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

            if (pawn != null)
            {
                Aspects.Coloration colorationAspect = pawn.GetAspectTracker()?.GetAspect(typeof(Aspects.Coloration)) as Aspects.Coloration;
                if (colorationAspect != null)
                {
                    Color? aspectColor = colorationAspect.TryGetColorationAspectColor(Aspects.Coloration.PawnColorSlot.SkinSecond);
                    if (aspectColor.HasValue)
                        return aspectColor;
                }
            }

            if (def.ExplicitHybridRace == null)
            {
                HybridRaceSettings.GraphicsSettings gSettings = def.raceSettings?.graphicsSettings;

                if (pawn?.gender == Gender.Female && gSettings?.femaleSkinColorOverrideSecond != null)
                    return gSettings.femaleSkinColorOverrideSecond;

                return gSettings?.skinColorOverrideSecond;
            }

            var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;
            ColorGenerator colorGenerator = hRace?.alienRace?.generalSettings?.alienPartGenerator?.alienskinsecondcolorgen;

            Color? color;
            if (colorGenerator != null)
                try
                {
                    if (pawn != null) Rand.PushState(pawn.thingIDNumber);
                    color = colorGenerator.NewRandomizedColor();
                }
                finally
                {
                    if (pawn != null) Rand.PopState();
                }
            else color = null;

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
    }
}