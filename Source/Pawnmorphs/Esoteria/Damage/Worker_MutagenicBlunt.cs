// Worker_MutagenicBlunt.cs created by Iron Wolf for Pawnmorph on 11/06/2019 1:55 PM
// last updated 11/06/2019  1:55 PM

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Pawnmorph.Damage
{
	/// <summary>
	///     damage worker for mutagenic blunt damage
	/// </summary>
	/// <seealso cref="Verse.DamageWorker_Blunt" />
	public class Worker_MutagenicBlunt : Worker_MutagenicInjury
	{
		/// <summary>
		///     Applies the special effects to part.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="totalDamage">The total damage.</param>
		/// <param name="dinfo">The dinfo.</param>
		/// <param name="result">The result.</param>
		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
		{
			bool flag = Rand.Chance(def.bluntInnerHitChance);
			float num = !flag ? 0f : def.bluntInnerHitDamageFractionToConvert.RandomInRange;
			float num2 = totalDamage * (1f - num);
			DamageInfo lastInfo = dinfo;
			while (true)
			{
				num2 -= FinalizeAndAddInjury(pawn, num2, lastInfo, result);
				if (!pawn.health.hediffSet.PartIsMissing(lastInfo.HitPart)) break;
				if (num2 <= 1f) break;
				BodyPartRecord parent = lastInfo.HitPart.parent;
				if (parent == null) break;
				lastInfo.SetHitPart(parent);
			}

			if (flag
			 && !lastInfo.HitPart.def.IsSolid(lastInfo.HitPart, pawn.health.hediffSet.hediffs)
			 && lastInfo.HitPart.depth == BodyPartDepth.Outside)
			{
				IEnumerable<BodyPartRecord> source = from x in pawn.health.hediffSet.GetNotMissingParts()
													 where x.parent == lastInfo.HitPart
														&& x.def.IsSolid(x, pawn.health.hediffSet.hediffs)
														&& x.depth == BodyPartDepth.Inside
													 select x;
				BodyPartRecord hitPart;
				if (source.TryRandomElementByWeight(x => x.coverageAbs, out hitPart))
				{
					DamageInfo lastInfo2 = lastInfo;
					lastInfo2.SetHitPart(hitPart);
					float totalDamage2 = totalDamage * num + totalDamage * def.bluntInnerHitDamageFractionToAdd.RandomInRange;
					FinalizeAndAddInjury(pawn, totalDamage2, lastInfo2, result);
				}
			}

			if (!pawn.Dead)
			{
				SimpleCurve simpleCurve = null;
				if (lastInfo.HitPart.parent == null)
					simpleCurve = def.bluntStunChancePerDamagePctOfCorePartToBodyCurve;
				else
					foreach (BodyPartRecord current in pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource))
						if (InSameBranch(current, lastInfo.HitPart))
						{
							simpleCurve = def.bluntStunChancePerDamagePctOfCorePartToHeadCurve;
							break;
						}

				if (simpleCurve != null)
				{
					float x2 = totalDamage / pawn.def.race.body.corePart.def.GetMaxHealth(pawn);
					if (Rand.Chance(simpleCurve.Evaluate(x2)))
					{
						DamageInfo dinfo2 = dinfo;
						dinfo2.Def = DamageDefOf.Stun;
						dinfo2.SetAmount(def.bluntStunDuration.SecondsToTicks() / 30f);
						pawn.TakeDamage(dinfo2);
					}
				}
			}
		}

		/// <summary>
		///     Chooses the hit part.
		/// </summary>
		/// <param name="dinfo">The dinfo.</param>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
		}

		private bool InSameBranch(BodyPartRecord lhs, BodyPartRecord rhs)
		{
			while (lhs.parent != null && lhs.parent.parent != null) lhs = lhs.parent;
			while (rhs.parent != null && rhs.parent.parent != null) rhs = rhs.parent;
			return lhs == rhs;
		}
	}
}