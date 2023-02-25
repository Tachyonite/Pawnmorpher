using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EtherGun
{
	/// <summary>
	/// comp for creating a mutagenic explosion 
	/// </summary>
	public class CompEtherExplosive : CompExplosive
	{
		/// <summary>
		/// the comp properties 
		/// </summary>
		public new CompProperties_EtherExplosive Props
		{
			get
			{
				return (CompProperties_EtherExplosive)props;
			}
		}

		/// <summary>
		/// called every tick after it's parent updates 
		/// </summary>
		public override void CompTick()
		{
			base.CompTick();
			if (wickStarted && wickTicksLeft <= 1)
			{
				TransformArea();
			}
		}


		void TransformArea()
		{
			List<Thing> thingList = GenRadial.RadialDistinctThingsAround(parent.PositionHeld, parent.Map, Props.explosiveRadius, true).ToList();
			List<Pawn> pawnsAffected = new List<Pawn>();
			HediffDef hediff = Props.HediffToAdd;
			float chance = Props.AddHediffChance;

			foreach (Pawn pawn in thingList.OfType<Pawn>())
			{

				if (!pawnsAffected.Contains(pawn) && Props.CanAddHediffToPawn(pawn))
				{
					pawnsAffected.Add(pawn);
				}
			}

			TransformPawn.ApplyHediff(pawnsAffected, parent.Map, hediff, chance); // Does the list need clearing?
		}
	}
}
