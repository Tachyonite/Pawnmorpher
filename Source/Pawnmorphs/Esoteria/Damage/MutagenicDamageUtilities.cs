// MutagenicDamageUtilities.cs created by Iron Wolf for Pawnmorph on 11/06/2019 7:47 AM
// last updated 11/06/2019  7:47 AM

using System;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Interfaces;
using UnityEngine;
using Verse;

namespace Pawnmorph.Damage
{
	/// <summary>
	///     static class containing utilities related to mutagenic damage
	/// </summary>
	public static class MutagenicDamageUtilities
	{
		/// <summary>the amount of hediff severity to add per point of damage </summary>
		public const float SEVERITY_PER_DAMAGE = 0.04f;

		/// <summary>the fraction by which the dInfo damage will be reduced by</summary>
		public const float REDUCE_VALUE = 1 / 3f;

		/// <summary>Applies the mutagenic damage.</summary>
		/// <param name="originalDamage">The original damage.</param>
		/// <param name="damageInfo">The damage information.</param>
		/// <param name="pawn">The pawn.</param>
		/// <param name="result">The result.</param>
		/// <param name="mutagenicDef">The definition of the mutagenic damage hediff to add.</param>
		/// <param name="severityPerDamage">The severity per damage.</param>
		/// <param name="mutagen">The mutagen.</param>
		/// <exception cref="ArgumentNullException">
		///     pawn
		///     or
		///     result
		/// </exception>
		public static void ApplyMutagenicDamage(float originalDamage, DamageInfo damageInfo, [NotNull] Pawn pawn,
												[NotNull] DamageWorker.DamageResult result,
												HediffDef mutagenicDef = null, float severityPerDamage = SEVERITY_PER_DAMAGE,
												MutagenDef mutagen = null)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			if (result == null) throw new ArgumentNullException(nameof(result));
			MutagenicDamageExtension ext = damageInfo.Weapon?.GetModExtension<MutagenicDamageExtension>()
										?? damageInfo.Def?.GetModExtension<MutagenicDamageExtension>();
			mutagen = mutagen ?? MutagenDefOf.defaultMutagen;
			mutagenicDef = mutagenicDef
						?? ext?.mutagenicBuildup
						?? MorphTransformationDefOf.MutagenicBuildup_Weapon;
			//first check if we're given a specific hediff to use
			//then use what's ever attached to the damage def
			//then use the default 

			if (!mutagen.CanInfect(pawn)) return;

			ApplyMutagen(originalDamage, damageInfo, pawn, mutagenicDef, mutagen, severityPerDamage, result);
		}

		/// <summary>
		///     Applies the pure mutagenic damage.
		/// </summary>
		/// this does not actually damage the pawn
		/// <param name="dInfo">The damage info.</param>
		/// <param name="pawn">The pawn.</param>
		/// <param name="mutationHediffDef">The mutation hediff definition.</param>
		/// <param name="severityPerDamage">The severity per damage.</param>
		/// <param name="mutagen">The mutagen.</param>
		public static void ApplyPureMutagenicDamage(DamageInfo dInfo, [NotNull] Pawn pawn,
													HediffDef mutationHediffDef = null,
													float severityPerDamage = SEVERITY_PER_DAMAGE, MutagenDef mutagen = null)
		{
			mutagen = mutagen ?? MutagenDefOf.defaultMutagen;
			MutagenicDamageExtension ext = dInfo.Weapon?.GetModExtension<MutagenicDamageExtension>()
										?? dInfo.Def?.GetModExtension<MutagenicDamageExtension>();
			mutationHediffDef = mutationHediffDef
							 ?? ext?.mutagenicBuildup
							 ?? MorphTransformationDefOf.MutagenicBuildup_Weapon;
			//first check if we're given a specific hediff to use
			//then use what's ever attached to the damage def
			//then use the default 

			float severityToAdd = Mathf.Clamp(dInfo.Amount * severityPerDamage, 0, mutationHediffDef.maxSeverity);

			Hediff hediff = HediffMaker.MakeHediff(mutationHediffDef, pawn);
			hediff.Severity = severityToAdd;

			if (hediff is ICaused caused)
			{
				if (dInfo.Weapon != null)
					caused.Causes.Add(MutationCauses.WEAPON_PREFIX, dInfo.Weapon);

				if (mutagen != null)
					caused.Causes.Add(MutationCauses.MUTAGEN_PREFIX, mutagen);

				if (dInfo.Def != null)
					caused.Causes.Add(string.Empty, dInfo.Def);
			}

			pawn.health.AddHediff(hediff);
		}


		/// <summary>Gets the severity per damage.</summary>
		/// <param name="dInfo">The damage information.</param>
		/// <returns></returns>
		public static float GetSeverityPerDamage(this DamageInfo dInfo)
		{
			MutagenicDamageExtension ext = dInfo.Weapon?.GetModExtension<MutagenicDamageExtension>()
										?? dInfo.Def?.GetModExtension<MutagenicDamageExtension>();
			float severityPerDamage = ext?.severityPerDamage
								   ?? SEVERITY_PER_DAMAGE;

			return severityPerDamage;
		}

		/// <summary>Reduces the damage by some amount.</summary>
		/// <param name="dInfo">The d information.</param>
		/// <param name="reduceAmount">The reduce amount.</param>
		/// <returns>a new damage info struct with the modified damage</returns>
		public static DamageInfo
			ReduceDamage(DamageInfo dInfo,
						 float reduceAmount =
							 REDUCE_VALUE) //we want to reduce the damage so weapons look more damage then they actually are 
		{
			float reducedDamage = dInfo.Amount * reduceAmount;

			dInfo = new DamageInfo(dInfo.Def, reducedDamage, dInfo.ArmorPenetrationInt, dInfo.Angle, dInfo.Instigator,
								   dInfo.HitPart, dInfo.Weapon, dInfo.Category, dInfo.intendedTargetInt);
			return dInfo;
		}

		private static void ApplyMutagen(float damage, DamageInfo info, Pawn pawn, HediffDef mutagen, MutagenDef mutagenDef, float severityPerDamage,
										 DamageWorker.DamageResult result)
		{
			Hediff hediff = HediffMaker.MakeHediff(mutagen, pawn);
			float severityToAdd = damage * severityPerDamage;

			//apply  toxin resistance 
			severityToAdd *= pawn.GetMutagenicBuildupMultiplier(); //[0, 1?] 
																   //0 is immune 
																   //Add our own mutagenic sensitivity stat to? 
			hediff.Severity = severityToAdd;

			if (hediff is ICaused caused)
			{
				if (info.Weapon != null)
					caused.Causes.Add(MutationCauses.WEAPON_PREFIX, info.Weapon);
				
				if (mutagen != null)
					caused.Causes.Add(MutationCauses.MUTAGEN_PREFIX, mutagenDef);

				if (info.Def != null)
					caused.Causes.Add(string.Empty, info.Def);
			}

			Log.Message($"original damage:{damage}, reducedDamage{info.Amount}, severity:{severityToAdd}");

			pawn.health.AddHediff(hediff, null, info, result);
		}
	}
}