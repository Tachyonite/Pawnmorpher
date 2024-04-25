// MorphDefs.cs modified by Iron Wolf for Pawnmorph on 07/31/2019 1:27 PM
// last updated 07/31/2019  1:27 PM

using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph.Hediffs
{
	/// <summary>
	///     static def of class containing morph transformation defs
	/// </summary>
	[DefOf]
	public static class MorphTransformationDefOf
	{
		/// <summary>
		/// random partial mutations 
		/// </summary>
		public static HediffDef FullRandomTF;
		/// <summary>
		/// full chaomorph mutation 
		/// </summary>
		public static HediffDef FullRandomTFAnyOutcome;



		public static HediffDef StabiliserHigh;  //should move this somewhere else 

		//hediff added by the reverter 
		public static HediffDef PM_Reverting;

		//special def 
		public static HediffDef MutagenicBuildup;
		public static HediffDef MutagenicBuildup_Weapon;

		public static HediffDef PM_MutagenicInfection;
		static MorphTransformationDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(MorphTransformationDefOf));

		}
	}
}