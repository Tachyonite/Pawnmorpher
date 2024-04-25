using System.Collections.Generic;
using System.Linq;
using Pawnmorph;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace EtherGun
{
	/// <summary>
	/// death worker that causes a mutagenic explosion upon death 
	/// </summary>
	/// <seealso cref="Verse.DeathActionWorker" />
	public class DeathActionWorker_MutagenicExplosion : DeathActionWorker
	{
		/// <summary>
		/// Gets the death rules.
		/// </summary>
		/// <value>
		/// The death rules.
		/// </value>
		public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

		/// <summary>
		/// Gets a value indicating whether this instance is dangerous in melee.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is dangerous in melee; otherwise, <c>false</c>.
		/// </value>
		public override bool DangerousInMelee => true;

		/// <summary>
		/// called when the attached pawn dies.
		/// </summary>
		/// <param name="corpse">The corpse.</param>
		public override void PawnDied(Corpse corpse, Lord _)
		{
			GenExplosion.DoExplosion(radius: (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 0) ? 2.9f : ((corpse.InnerPawn.ageTracker.CurLifeStageIndex != 1) ? 5.9f : 3.9f), center: corpse.Position, map: corpse.Map, damType: DamageDefOf.Flame, instigator: corpse.InnerPawn);
			List<Thing> thingList = GenRadial.RadialDistinctThingsAround(corpse.PositionHeld, corpse.Map, (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 0) ? 2.9f : ((corpse.InnerPawn.ageTracker.CurLifeStageIndex != 1) ? 5.9f : 3.9f), true).ToList();
			List<Pawn> pawnsAffected = new List<Pawn>();
			HediffDef hediff = MorphTransformationDefOf.FullRandomTF;
			float chance = 0.7f;

			foreach (Pawn pawn in thingList.OfType<Pawn>())
			{

				if (!pawnsAffected.Contains(pawn) && MutagenDefOf.defaultMutagen.CanInfect(pawn))
				{
					pawnsAffected.Add(pawn);
				}
			}

			TransformPawn.ApplyHediff(pawnsAffected, corpse.InnerPawn.Map, hediff, chance);

		}
	}
}
