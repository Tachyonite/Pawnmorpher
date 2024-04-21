// PMDamageDefOf.cs created by Iron Wolf for Pawnmorph on 03/28/2020 9:47 AM
// last updated 03/28/2020  9:47 AM

using JetBrains.Annotations;
using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph
{
	[DefOf]
	public static class PMDamageDefOf
	{

		/// <summary>
		/// The mutagen cloud (used with small explosions and spills like pipes) 
		/// </summary>
		[NotNull] public static DamageDef MutagenCloud_Tiny;

		/// <summary>
		/// The mutagen cloud (used with regular explosions like grenades) 
		/// </summary>
		[NotNull] public static DamageDef MutagenCloud;

		/// <summary>
		/// The mutagen cloud large (used with artillery) 
		/// </summary>
		[NotNull] public static DamageDef MutagenCloud_Large;
	}
}