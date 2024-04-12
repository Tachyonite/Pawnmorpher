// MorphGraphicsUtils.cs created by Iron Wolf for Pawnmorph on 09/13/2019 9:50 AM
// last updated 09/13/2019  9:51 AM

using System;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using RimWorld;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace Pawnmorph.GraphicSys
{
	/// <summary>
	///     collection of useful graphics related utility functions on morphs
	/// </summary>
	public static class MorphGraphicsUtils
	{

		/// <summary>
		/// Sets the color of the skin.
		/// </summary>
		/// <param name="alienComp">The alien comp.</param>
		/// <param name="first">The first.</param>
		/// <param name="second">The second.</param>
		/// <exception cref="ArgumentNullException">alienComp</exception>
		public static void SetSkinColor([NotNull] this AlienPartGenerator.AlienComp alienComp, Color first, Color? second = null)
		{
			if (alienComp == null) throw new ArgumentNullException(nameof(alienComp));
			alienComp.ColorChannels["skin"].first = first;
			if (second != null) alienComp.ColorChannels["skin"].second = second.Value;

		}

		/// <summary>
		/// Gets the color of the hair.
		/// </summary>
		/// <param name="alienComp">The alien comp.</param>
		/// <param name="first">if set to <c>true</c> [first].</param>
		/// <exception cref="ArgumentNullException">alienComp</exception>
		public static Color? GetHairColor([NotNull] this AlienPartGenerator.AlienComp alienComp, bool first = true)
		{
			if (alienComp == null) throw new ArgumentNullException(nameof(alienComp));
			AlienPartGenerator.ExposableValueTuple<Color, Color> tuple = alienComp.ColorChannels.TryGetValue("hair");
			return first ? tuple?.first : tuple?.second;

		}

		/// <summary>
		/// Gets the color of the skin.
		/// </summary>
		/// <param name="alienComp">The alien comp.</param>
		/// <param name="first">if set to <c>true</c> [first].</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">alienComp</exception>
		public static Color? GetSkinColor([NotNull] this AlienPartGenerator.AlienComp alienComp, bool first = true)
		{
			if (alienComp == null) throw new ArgumentNullException(nameof(alienComp));
			AlienPartGenerator.ExposableValueTuple<Color, Color> tuple = alienComp.ColorChannels.TryGetValue("skin");
			return first ? tuple?.first : tuple?.second;
		}

		/// <summary>
		/// Sets the color of the hair.
		/// </summary>
		/// <param name="alienComp">The alien comp.</param>
		/// <param name="first">The first.</param>
		/// <param name="second">The second.</param>
		/// <exception cref="ArgumentNullException">alienComp</exception>
		public static void SetHairColor([NotNull] this AlienPartGenerator.AlienComp alienComp, Color first, Color? second = null)
		{
			if (alienComp == null) throw new ArgumentNullException(nameof(alienComp));
			var tuple = alienComp.ColorChannels["hair"];
			tuple.first = first;
			if (second != null) tuple.second = second.Value;

		}

		/// <summary>
		/// Generates the random color.
		/// </summary>
		/// <param name="generator">The generator.</param>
		/// <param name="channelName">Name of the channel.</param>
		/// <param name="first">if set to <c>true</c> [first].</param>
		/// <param name="seed">The seed.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">generator</exception>
		public static Color? GenerateRandomColor([NotNull] this AlienPartGenerator generator, string channelName, bool first = true, int? seed = null)
		{
			if (generator == null)
				throw new ArgumentNullException(nameof(generator));

			var channel = generator.colorChannels?.FirstOrDefault(c => c.name == channelName);
			if (channel == null)
				return null;

			var channelCategory = channel.entries.RandomElementByWeight((ColorChannelGeneratorCategory ccgc) => ccgc.weight);
			var cGen = first ? channelCategory?.first : channelCategory?.second;
			if (cGen == null)
				return null;


			if (seed != null)
			{
				try
				{
					Rand.PushState(seed.Value);
					return cGen.NewRandomizedColor();
				}
				finally
				{
					Rand.PopState();
				}
			}

			return cGen.NewRandomizedColor();

		}

		/// <summary>
		/// Gets the part generator.
		/// </summary>
		/// <param name="alienRace">The alien race.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">alienRace</exception>
		[CanBeNull]
		public static AlienPartGenerator GetPartGenerator([NotNull] this ThingDef_AlienRace alienRace)
		{
			if (alienRace == null) throw new ArgumentNullException(nameof(alienRace));
			return alienRace.alienRace?.generalSettings?.alienPartGenerator;
		}

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

			HybridRaceSettings.GraphicsSettings gSettings = def.raceSettings?.graphicsSettings;

			if (def.ExplicitHybridRace == null)
			{
				Gender? gender = pawn?.gender;

				if (gender == Gender.Female && gSettings?.femaleHairColorOverride != null)
					return gSettings.femaleHairColorOverride;

				return gSettings?.hairColorOverride ?? GetSkinColorOverride(def, pawn);
			}

			var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;
			var pGen = hRace.GetPartGenerator();

			Color? color = null;

			if (pGen != null)
			{
				color = pGen.GenerateRandomColor("hair", true, pawn?.thingIDNumber);
			}

			color = color ?? GetSkinColorOverride(def, pawn);


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
				HybridRaceSettings.GraphicsSettings gSettings = def.raceSettings?.graphicsSettings;
				if (pawn?.gender == Gender.Female && gSettings?.femaleHairColorOverrideSecond != null)
					return gSettings.femaleHairColorOverrideSecond;

				return gSettings?.hairColorOverrideSecond ?? GetSkinColorSecondOverride(def, pawn);
			}

			var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;
			var gen = hRace?.GetPartGenerator();
			Color? color = gen?.GenerateRandomColor("hair", false, pawn?.thingIDNumber);





			color = color ?? GetSkinColorOverride(def, pawn);


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
				HybridRaceSettings.GraphicsSettings raceGSettings = def.raceSettings?.graphicsSettings;
				Gender? gender = pawn?.gender;
				if (gender == Gender.Female && raceGSettings?.femaleSkinColorOverride != null)
					return raceGSettings.femaleSkinColorOverride;

				return raceGSettings?.skinColorOverride;
			}

			var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;
			var pGen = hRace?.GetPartGenerator();


			return pGen?.GenerateRandomColor("skin", true, pawn?.thingIDNumber);
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
				HybridRaceSettings.GraphicsSettings gSettings = def.raceSettings?.graphicsSettings;

				if (pawn?.gender == Gender.Female && gSettings?.femaleSkinColorOverrideSecond != null)
					return gSettings.femaleSkinColorOverrideSecond;

				return gSettings?.skinColorOverrideSecond;
			}

			var hRace = def.ExplicitHybridRace as ThingDef_AlienRace;

			AlienPartGenerator generalSettingsAlienPartGenerator = hRace?.alienRace?.generalSettings?.alienPartGenerator;

			return generalSettingsAlienPartGenerator?.GenerateRandomColor("skin", false, pawn?.thingIDNumber);
		}

		/// <summary>
		///     refresh the graphics associated with this pawn, including the portraits if it's a colonist
		/// </summary>
		/// <param name="pawn"></param>
		public static void RefreshGraphics([NotNull] this Pawn pawn)
		{
			if (Current.ProgramState != ProgramState.Playing)
				return; //make sure we don't refresh the graphics while the game is loading

			pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
	}
}