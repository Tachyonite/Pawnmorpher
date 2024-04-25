// Tag.cs created by Iron Wolf for Pawnmorph on 08/13/2020 5:18 PM
// last updated 08/13/2020  5:18 PM

using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Verbs
{
	/// <summary>
	/// custom verb for the shoot tool 
	/// </summary>
	/// <seealso cref="Verse.Verb_Shoot" />
	/// <seealso cref="Pawnmorph.ICustomVerb" />
	public class Tag : Verb_Shoot, ICustomVerb
	{
		private const string TAG_LABEL = "PMTagGizmoLabel";
		private const string TAG_DESCRIPTION = "PMTagGizmoDescription";

		private static string _tagLabel;
		private static string _tagDescription;

		/// <summary>
		/// Tries the cast shot.
		/// </summary>
		/// <returns></returns>
		protected override bool TryCastShot()
		{
			if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
			{
				return false;
			}
			ThingDef projectile = Projectile;
			if (projectile == null)
			{
				return false;
			}
			ShootLine resultingLine;
			bool flag = TryFindShootLineFromTo(caster.Position, currentTarget, out resultingLine);
			if (verbProps.stopBurstWithoutLos && !flag)
			{
				return false;
			}
			if (base.EquipmentSource != null)
			{
				base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
			}
			Thing launcher = caster;
			Thing equipment = base.EquipmentSource;
			CompMannable compMannable = caster.TryGetComp<CompMannable>();
			if (compMannable != null && compMannable.ManningPawn != null)
			{
				launcher = compMannable.ManningPawn;
				equipment = caster;
			}
			Vector3 drawPos = caster.DrawPos;
			Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, resultingLine.Source, caster.Map);
			if (verbProps.ForcedMissRadius > 0.5f)
			{
				float num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.ForcedMissRadius, currentTarget.Cell - caster.Position);
				if (num > 0.5f)
				{
					int max = GenRadial.NumCellsInRadius(num);
					int num2 = Rand.Range(0, max);
					if (num2 > 0)
					{
						IntVec3 c = currentTarget.Cell + GenRadial.RadialPattern[num2];
						ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
						if (Rand.Chance(0.5f))
						{
							projectileHitFlags = ProjectileHitFlags.All;
						}
						if (!canHitNonTargetPawnsNow)
						{
							projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
						}
						projectile2.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, true, equipment);
						return true;
					}
				}
			}
			ShotReport shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
			Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
			ThingDef targetCoverDef = randomCoverToMissInto?.def;
			//never miss 

			ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;

			if (currentTarget.Thing != null)
			{
				projectile2.Launch(launcher, drawPos, currentTarget, currentTarget, projectileHitFlags4, true, equipment, targetCoverDef);
			}
			else
			{
				projectile2.Launch(launcher, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags4, true, equipment, targetCoverDef);
			}
			return true;
		}


		static string TagLabel
		{
			get
			{
				if (string.IsNullOrEmpty(_tagLabel))
				{
					_tagLabel = TAG_LABEL.Translate();
				}

				return _tagLabel;
			}
		}

		static string TagDescription
		{
			get
			{
				if (string.IsNullOrEmpty(_tagDescription))
				{
					_tagDescription = TAG_DESCRIPTION.Translate();
				}

				return _tagDescription;
			}
		}

		/// <summary>
		/// Gets the label.
		/// </summary>
		/// <param name="ownerThing">The owner thing.</param>
		/// <returns></returns>
		public string GetLabel(Thing ownerThing)
		{
			return TagDescription;
		}

		/// <summary>
		/// Gets the description for this verb 
		/// </summary>
		/// <param name="ownerThing">The owner thing.</param>
		/// <returns></returns>
		public string GetDescription(Thing ownerThing)
		{
			return TagDescription;
		}

		/// <summary>
		/// Gets the UI icon for this verb 
		/// </summary>
		/// <param name="ownerThing">The owner thing.</param>
		/// <returns></returns>
		public Texture2D GetUIIcon(Thing ownerThing)
		{
			return PMTextures.TagrifleIcon;
		}
	}
}