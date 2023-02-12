// Worker_MutagenicCut.cs created by Iron Wolf for Pawnmorph on 11/11/2019 2:42 PM
// last updated 11/11/2019  2:42 PM

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph.Damage
{
	/// <summary>
	/// damage worker for mutagenic cut 
	/// </summary>
	public class Worker_MutagenicCut : Worker_MutagenicInjury
	{
		/// <summary>Chooses the hit part.</summary>
		/// <param name="dinfo">The dinfo.</param>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside, null);
		}

		/// <summary>Applies the special effects to part.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="totalDamage">The total damage.</param>
		/// <param name="dinfo">The dinfo.</param>
		/// <param name="result">The result.</param>
		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
		{
			BodyPartRecord hitPart = dinfo.HitPart;
			if (hitPart.depth == BodyPartDepth.Inside)
			{
				List<BodyPartRecord> list = new List<BodyPartRecord>();
				for (BodyPartRecord bodyPartRecord = hitPart; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
				{
					list.Add(bodyPartRecord);
					if (bodyPartRecord.depth == BodyPartDepth.Outside)
					{
						break;
					}
				}
				float num = (float)(list.Count - 1) + 0.5f;
				for (int i = 0; i < list.Count; i++)
				{
					DamageInfo dinfo2 = dinfo;
					dinfo2.SetHitPart(list[i]);
					base.FinalizeAndAddInjury(pawn, totalDamage / num * ((i != 0) ? 1f : 0.5f), dinfo2, result);
				}

				if (Rand.Range(0, 1f) < 0.5f)
					AddMutationOn(hitPart, pawn);
				//add extra mutagenic buildup severity 
				AddExtraBuildup(pawn, dinfo);
			}
			else
			{

				float l = hitPart.def.IsSolid(hitPart, pawn.health.hediffSet.hediffs) ? 0.5f : 0.3f;

				if (Rand.Range(0, 1f) < l)
					AddMutationOn(hitPart, pawn);

				int num2 = (this.def.cutExtraTargetsCurve == null) ? 0 : GenMath.RoundRandom(this.def.cutExtraTargetsCurve.Evaluate(Rand.Value));
				List<BodyPartRecord> list2;
				if (num2 != 0)
				{
					IEnumerable<BodyPartRecord> enumerable = hitPart.GetDirectChildParts();
					if (hitPart.parent != null)
					{
						enumerable = enumerable.Concat(hitPart.parent);
						if (hitPart.parent.parent != null)
						{
							enumerable = enumerable.Concat(hitPart.parent.GetDirectChildParts());
						}
					}
					list2 = (from x in enumerable.Except(hitPart).InRandomOrder(null).Take(num2)
							 where !x.def.conceptual
							 select x).ToList<BodyPartRecord>();
				}
				else
				{
					list2 = new List<BodyPartRecord>();
				}
				list2.Add(hitPart);
				float num3 = totalDamage * (1f + this.def.cutCleaveBonus) / ((float)list2.Count + this.def.cutCleaveBonus);
				if (num2 == 0)
				{
					num3 = base.ReduceDamageToPreserveOutsideParts(num3, dinfo, pawn);
				}
				for (int j = 0; j < list2.Count; j++)
				{
					DamageInfo dinfo3 = dinfo;
					dinfo3.SetHitPart(list2[j]);
					base.FinalizeAndAddInjury(pawn, num3, dinfo3, result);
				}
			}
		}
	}
}