// Worker_MutagenicInjury.cs modified by Iron Wolf for Pawnmorph on 11/03/2019 11:05 AM
// last updated 11/03/2019  11:05 AM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Damage
{
	/// <summary>
	///     damage worker that adds mutagenic buildup hediff in addition to regular injuries
	/// </summary>
	/// <seealso cref="Verse.DamageWorker_AddInjury" />
	public class Worker_MutagenicInjury : DamageWorker_AddInjury
	{
		/// <summary>
		///     values below this should be considered 0
		/// </summary>
		protected const float EPSILON = 0.001f;


		private const float REDUCE_VALUE = 1 / 3f;

		/// <summary>
		///     Applies the specified dinfo.
		/// </summary>
		/// <param name="dinfo">The dinfo.</param>
		/// <param name="thing">The thing.</param>
		/// <returns></returns>
		public override DamageResult Apply(DamageInfo dinfo, Thing thing)
		{
			if (thing is Pawn pawn)
			{
				MutagenDef mutagen = dinfo.Weapon.GetModExtension<MutagenExtension>()?.mutagen
								  ?? dinfo.Def.GetModExtension<MutagenicDamageExtension>()?.mutagen
								  ?? MutagenDefOf.defaultMutagen;

				if (mutagen.CanInfect(pawn))
					return ApplyToPawn(dinfo, pawn, mutagen);
			}

			return base.Apply(dinfo, thing);
		}


		/// <summary>
		///     Adds some extra buildup. taking into account toxic resistance and immunities
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="dInfo">The d information.</param>
		protected void AddExtraBuildup(Pawn pawn, DamageInfo dInfo)
		{
			if (!MutagenDefOf.defaultMutagen.CanInfect(pawn)) return;
			float extraSeverity = dInfo.Amount * 0.02f * dInfo.GetSeverityPerDamage();
			extraSeverity *= 1f - pawn.GetStatValue(StatDefOf.ToxicResistance);

			if (Mathf.Abs(extraSeverity) < EPSILON) return;

			HediffDef hDef = dInfo.Def.GetModExtension<MutagenicDamageExtension>()?.mutagenicBuildup
						  ?? MorphTransformationDefOf.MutagenicBuildup_Weapon;
			if (pawn != null)
				MutagenicBuildupUtilities.AdjustMutagenicBuildup(dInfo.Def, pawn, extraSeverity);

		}

		/// <summary>
		///     Adds the mutation on.
		/// </summary>
		/// <param name="forceHitPart">The force hit part.</param>
		/// <param name="pawn">The pawn.</param>
		protected void AddMutationOn([NotNull] BodyPartRecord forceHitPart, [NotNull] Pawn pawn)
		{
			if (!MutagenDefOf.defaultMutagen.CanInfect(pawn)) return;
			while (forceHitPart != null)
			{
				var mutation = MutationUtilities.GetMutationsByPart(forceHitPart.def).RandomElementWithFallback();
				if (mutation == null)
				{
					forceHitPart = forceHitPart.parent; //go upward until we hit a mutable part 
					continue;
				}

				if (!MutationUtilities.AddMutation(pawn, mutation, forceHitPart))
				{
					forceHitPart = forceHitPart.parent; //go upward until we hit a mutable part  
					continue;
				}

				break; //if we apply a mutation break the loop 
			}
		}
		/// <summary>
		/// does explosive damage to a thing 
		/// </summary>
		/// <param name="explosion">The explosion.</param>
		/// <param name="t">The t.</param>
		/// <param name="damagedThings">The damaged things.</param>
		/// <param name="ignoredThings">The ignored things.</param>
		/// <param name="cell">The cell.</param>
		protected override void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell)
		{
			if (t.def.category == ThingCategory.Mote || t.def.category == ThingCategory.Ethereal) return;
			if (damagedThings.Contains(t)) return;
			damagedThings.Add(t);
			//it puts out fires 
			if (def == DamageDefOf.Bomb && t.def == ThingDefOf.Fire && !t.Destroyed)
			{
				t.Destroy();
				return;
			}
			//only affects pawns 

			if (!(t is Pawn pawn))
			{
				ignoredThings?.Add(t);
				return;
			}

			if (!MutagenDefOf.defaultMutagen.CanInfect(pawn))
			{

				ignoredThings?.Add(pawn);
				return; //only affects susceptible pawns 
			}


			float num;
			if (t.Position == explosion.Position)
				num = Rand.RangeInclusive(0, 359);
			else
				num = (t.Position - explosion.Position).AngleFlat;
			DamageDef damageDef = def;
			var amount = (float)explosion.GetDamageAmountAt(cell);
			float armorPenetrationAt = explosion.GetArmorPenetrationAt(cell);
			float angle = num;
			Thing instigator = explosion.instigator;
			ThingDef weapon = explosion.weapon;
			var dinfo = new DamageInfo(damageDef, amount, armorPenetrationAt, angle, instigator, null, weapon,
									   DamageInfo.SourceCategory.ThingOrUnknown, explosion.intendedTarget);
			float severityPerDamage = dinfo.GetSeverityPerDamage();
			MutagenicDamageUtilities.ApplyPureMutagenicDamage(dinfo, pawn,
															  severityPerDamage: severityPerDamage);


			BattleLogEntry_ExplosionImpact battleLogEntry_ExplosionImpact = null;
			battleLogEntry_ExplosionImpact =
				new BattleLogEntry_ExplosionImpact(explosion.instigator, t, explosion.weapon, explosion.projectile, def);
			Find.BattleLog.Add(battleLogEntry_ExplosionImpact);

			pawn.stances?.stagger?.StaggerFor(95);
		}




		/// <summary>Reduces the damage.</summary>
		/// <param name="dInfo">The d information.</param>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected DamageInfo ReduceDamage(DamageInfo dInfo, Pawn pawn)
		{
			float r;
			if (MutagenDefOf.defaultMutagen.CanInfect(pawn))
				r = dInfo.Def.GetModExtension<MutagenicDamageExtension>()?.reduceValue ?? REDUCE_VALUE;
			else
				r = 1;
			return MutagenicDamageUtilities.ReduceDamage(dInfo, r);
		}

		private DamageResult ApplyToPawn(DamageInfo dinfo, Pawn pawn, MutagenDef mutagen)
		{
			//reduce the amount to make it less likely to kill the pawn 
			float originalDamage = dinfo.Amount;

			dinfo = ReduceDamage(dinfo, pawn);

			DamageResult res = base.Apply(dinfo, pawn);
			float severityPerDamage = dinfo.GetSeverityPerDamage();

			if (!mutagen.CanInfect(pawn)) return res;
			MutagenicDamageUtilities.ApplyMutagenicDamage(originalDamage, dinfo, pawn, res, severityPerDamage: severityPerDamage,
														  mutagen: mutagen);

			return res;
		}
	}
}