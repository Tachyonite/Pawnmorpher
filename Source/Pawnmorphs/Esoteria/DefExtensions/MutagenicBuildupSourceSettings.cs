// MutagenBuildupSourceSettings.cs created by Iron Wolf for Pawnmorph on 03/25/2020 7:18 PM
// last updated 03/25/2020  7:18 PM

using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// def extension for settings for mutagen buildup sources 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class MutagenicBuildupSourceSettings : DefModExtension
	{
		/// <summary>
		/// the maximum severity this source should add 
		/// </summary>
		public float maxBuildup = 1;


		/// <summary>
		/// The mutagenic buildup definition to use 
		/// </summary>
		/// if null a default will be used instead 
		[CanBeNull]
		public HediffDef mutagenicBuildupDef;

		/// <summary>
		/// The mutagen definition
		/// </summary>
		[CanBeNull] public MutagenDef mutagenDef;
	}
}