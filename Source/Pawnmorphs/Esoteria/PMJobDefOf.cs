// PMJobDefOf.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 9:04 AM
// last updated 09/22/2019  9:04 AM

using JetBrains.Annotations;
using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph
{
	/// <summary> Static container for commonly referenced job defs. </summary>
	[DefOf]
	public static class PMJobDefOf
	{
		static PMJobDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMJobDefOf));
		}

		public static JobDef PMLayEgg;
		public static JobDef PMMilkSelf;
		public static JobDef PMDrainChemcyst;
		public static JobDef PMShaveSelf;
		public static JobDef PMResurrect;
		public static JobDef PM_OperateSequencer;

		public static JobDef PM_MutagenicSow;
		public static JobDef PM_PlantMutagenicPlant;

		public static JobDef PM_CarrySpecialToMutagenChamber;

		[NotNull] public static JobDef RecruitSapientFormerHuman;
		[NotNull]
		public static JobDef PM_UseMutationGenome;

		public static JobDef EnterMutagenChamber;
		[NotNull] public static JobDef PM_TransformPrisoner;
	}
}