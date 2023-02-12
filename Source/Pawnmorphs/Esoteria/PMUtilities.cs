// PMUtilities.cs created by Iron Wolf for Pawnmorph on 09/15/2019 7:38 PM
// last updated 09/15/2019  7:38 PM

using System;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// a collection of general Pawnmorpher related utilities 
	/// </summary>
	public static class PMUtilities
	{

		/// <summary>
		/// Gets a value indicating whether mutagenic diseases are enabled.
		/// </summary>
		/// <value>
		///   <c>true</c> if [mutagenic diseases enabled]; otherwise, <c>false</c>.
		/// </value>
		public static bool MutagenicDiseasesEnabled => GetSettings()?.enableMutagenDiseases == true;

		/// <summary>
		/// Gets a value indicating whether hazardous chaobulb is enabled or not.
		/// </summary>
		/// <value>
		///   <c>true</c> if hazardous chaobulb; otherwise, <c>false</c>.
		/// </value>
		public static bool HazardousChaobulb => GetSettings()?.hazardousChaobulbs == true;

		/// <summary>Gets the mod settings.</summary>
		/// <returns></returns>
		[NotNull]
		public static PawnmorpherSettings GetSettings()
		{
			return LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>();
		}


		/// <summary>
		/// Determines whether this pawn is loading or spawning.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if this pawn is loading or spawning ; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsLoadingOrSpawning([NotNull] this Pawn pawn)
		{
			if (PawnGenerator.IsBeingGenerated(pawn)) return true;
			if (pawn.health == null || pawn.mindState?.inspirationHandler == null || pawn.needs == null) return true;
			return false;
		}

		/// <summary>
		/// Gets the relation of this pawn to the given faction
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="faction">The faction.</param>
		/// <returns></returns>
		public static ColonyRelation GetRelation([NotNull] this Pawn pawn, [CanBeNull] Faction faction = null)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			faction = faction ?? Faction.OfPlayer;
			if (pawn.Faction == faction) return ColonyRelation.Colonist;
			if (pawn.IsPrisoner && pawn.guest?.HostFaction == faction)
			{

				return pawn.guilt?.IsGuilty == true ? ColonyRelation.PrisonerGuilty : ColonyRelation.Prisoner;
			}
			if (pawn.IsSlave && pawn.guest?.HostFaction == faction) return ColonyRelation.Slave;
			if (pawn.Faction != null && faction.AllyOrNeutralTo(pawn.Faction))
			{
				return ColonyRelation.Ally;
			}

			return ColonyRelation.Wild;
		}

		/// <summary>
		/// checks if this pawn can witness things about the other pawn.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="victim">The victim.</param>
		/// <returns></returns>
		public static bool Witnessed([NotNull] this Pawn p, [NotNull] Pawn victim)
		{
			if (!p.Awake() || !p.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
			{
				return false;
			}
			if (victim.IsCaravanMember())
			{
				return victim.GetCaravan() == p.GetCaravan();
			}
			if (!victim.Spawned || !p.Spawned)
			{
				return false;
			}
			if (!p.Position.InHorDistOf(victim.Position, 12f))
			{
				return false;
			}
			if (!GenSight.LineOfSight(victim.Position, p.Position, victim.Map))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// checks if this pawn can witness things about the other pawn.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="victimLocation">The victim location.</param>
		/// <returns></returns>
		public static bool Witnessed([NotNull] this Pawn p, IntVec3 victimLocation)
		{
			if (!p.Awake() || !p.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
			{
				return false;
			}

			if (!p.Position.InHorDistOf(victimLocation, 12f))
			{
				return false;
			}
			if (!GenSight.LineOfSight(victimLocation, p.Position, p.Map))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Gets the rival status of the other pawn relative to this pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="otherPawn">The other pawn.</param>
		/// <returns></returns>
		public static RivalStatus GetRivalStatus([NotNull] this Pawn pawn, [NotNull] Pawn otherPawn)
		{
			if (pawn.relations == null || otherPawn.relations == null) return RivalStatus.None;
			var lv = pawn.relations.OpinionOf(otherPawn);
			if (lv > 20) return RivalStatus.Friend;
			if (lv < -20) return RivalStatus.Rival;
			return RivalStatus.None;
		}
	}

	/// <summary>
	/// enum representing the rival status of a pawn
	/// </summary>
	public enum RivalStatus
	{
		/// <summary>
		/// The none
		/// </summary>
		None,
		/// <summary>
		/// The rival
		/// </summary>
		Rival,
		/// <summary>
		/// The friend
		/// </summary>
		Friend
	}
}