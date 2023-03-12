// TfHediffDefOf.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 7:40 PM
// last updated 08/13/2019  7:41 PM

using JetBrains.Annotations;
using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph
{
	/// <summary> Collection of misc tf related HediffDefs. </summary>
	[DefOf]
	public static class TfHediffDefOf
	{
		// ReSharper disable once NotNullMemberIsNotInitialized
		static TfHediffDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TfHediffDefOf));
		}


		[NotNull, UsedImplicitly(ImplicitUseKindFlags.Assign)]
		public static HediffDef PermanentlyFeral;

		public static HediffDef TransformationParalysis;

		[NotNull] public static HediffDef FeralPillSapienceDrop;

		[NotNull]
		public static HediffDef SapienceLimiterHediff;

		public static HediffDef LuciferiumHigh;

		public static HediffDef Bullrush;
	}
}