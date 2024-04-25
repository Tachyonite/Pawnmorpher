// ManhunterTfSettings.cs created by Iron Wolf for Pawnmorph on 06/23/2020 6:47 AM
// last updated 06/23/2020  6:47 AM

namespace Pawnmorph.TfSys
{
	/// <summary>
	///     struct for storing
	/// </summary>
	public struct ManhunterTfSettings
	{
		/// <summary>
		///     The multiplier for manhunter chance
		/// </summary>
		/// this is multiplied against the mod's manhunter chance, ex. setting it to 2 will make double the chance for it to happen
		public float mult;

		/// <summary>
		///     offset for the
		/// </summary>
		public float offset;

		/// <summary>
		///     Gets the default settings.
		/// </summary>
		/// <value>
		///     The default.
		/// </value>
		public static ManhunterTfSettings Default { get; } = new ManhunterTfSettings { mult = 1, offset = 0 };


		/// <summary>
		///     settings that means the pawn will never manhunter when tf'd
		/// </summary>
		/// <value>
		///     The never.
		/// </value>
		public static ManhunterTfSettings Never { get; } = new ManhunterTfSettings();

		/// <summary>
		///     settings that will always cause the pawn to manhunter when transformed
		/// </summary>
		/// <value>
		///     The always.
		/// </value>
		public static ManhunterTfSettings Always { get; } = new ManhunterTfSettings { mult = 1, offset = 1 };

		/// <summary>
		///     gets the overall manhunter chance
		/// </summary>
		/// <param name="friendly">if set to <c>true</c> this is the chance for a friendly pawn.</param>
		/// <returns></returns>
		public float ManhunterChance(bool friendly)
		{
			float baseChance = friendly
								   ? FormerHumanUtilities.BaseFriendlyManhunterTfChance
								   : FormerHumanUtilities.BaseManhunterTfChance;
			baseChance += offset;
			return baseChance * mult;
		}
	}
}