// MorphDefOfs.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 2:46 PM
// last updated 08/02/2019  2:46 PM

using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
	[DefOf]
	public static class MorphDefOfs
	{
		public static MorphDef WolfMorph;
		public static MorphDef WargMorph;
		public static MorphDef FoxMorph;
		public static MorphDef HuskyMorph;
		public static MorphDef CowMorph;
		public static MorphDef BoomalopeMorph;
		public static MorphDef SheepMorph;
		public static MorphDef PM_HellhoundMorph;

		public static MorphDef ChaocowMorph;
		public static MorphDef ChaoboomMorph;
		public static MorphDef ChaofoxMorph;
		public static MorphDef ChaodinoMorph;


		static MorphDefOfs()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MorphDefOfs));
		}
	}
}