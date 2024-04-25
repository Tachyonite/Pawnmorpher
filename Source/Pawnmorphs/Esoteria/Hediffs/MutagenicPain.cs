// MutagenicPain.cs created by Iron Wolf for Pawnmorph on 09/02/2021 7:01 AM
// last updated 09/02/2021  7:01 AM

using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// simple hediff class that uses the PM_MutagenPainSensitivity stat to control how much pain it's giving 
	/// </summary>
	/// <seealso cref="Verse.HediffWithComps" />
	public class MutagenicPain : HediffWithComps
	{
		[Unsaved, NotNull] private readonly Cached<float> _painStat;

		/// <summary>
		/// Initializes a new instance of the <see cref="MutagenicPain"/> class.
		/// </summary>
		public MutagenicPain()
		{
			_painStat = new Cached<float>(() => pawn.GetStatValue(PMStatDefOf.PM_MutagenPainSensitivity), 1);
		}

		private const int REFRESH_PERIOD_SPAWNED = 60;
		private const int REFRESH_PERIOD_WORLD_PAWN = REFRESH_PERIOD_SPAWNED * 3;

		private int RefreshPeriod => pawn.SpawnedOrAnyParentSpawned ? REFRESH_PERIOD_SPAWNED : REFRESH_PERIOD_WORLD_PAWN;

		/// <summary>
		/// Ticks this instance.
		/// </summary>
		public override void Tick()
		{
			base.Tick();

			if (pawn.IsHashIntervalTick(RefreshPeriod))
				_painStat.Recalculate();
		}

		/// <summary>
		/// Gets the pain offset.
		/// </summary>
		/// <value>
		/// The pain offset.
		/// </value>
		public override float PainOffset => base.PainOffset * _painStat.Value;
	}
}