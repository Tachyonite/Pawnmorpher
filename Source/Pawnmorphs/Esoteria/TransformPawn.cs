using System.Collections.Generic;
using System.Linq;
using Pawnmorph;
using Verse;

namespace EtherGun
{
	/// <summary>
	/// static class containing tranformation related functions 
	/// </summary>
	public static class TransformPawn
	{
		/// <summary>Applies the hediff.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="map">The map.</param>
		/// <param name="hediff">The hediff.</param>
		/// <param name="chance">The chance.</param>
		public static void ApplyHediff(Pawn pawn, Map map, HediffDef hediff, float chance)
		{
			var rand = Rand.Value;
			if (rand <= chance)
			{
				var etherOnPawn = pawn.health?.hediffSet?.GetFirstHediffOfDef(hediff);
				var randomSeverity = 1f;
				if (etherOnPawn != null)
				{
					etherOnPawn.Severity += randomSeverity;
				}
				else
				{
					Hediff hediffOnPawn = HediffMaker.MakeHediff(hediff, pawn);
					hediffOnPawn.Severity = randomSeverity;
					pawn.health?.AddHediff(hediffOnPawn);
					IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), map);
				}
			}
		}

		/// <summary>Applies the hediff.</summary>
		/// <param name="pawns">The pawns.</param>
		/// <param name="map">The map.</param>
		/// <param name="hediff">The hediff.</param>
		/// <param name="chance">The chance.</param>
		public static void ApplyHediff(List<Pawn> pawns, Map map, HediffDef hediff, float chance)
		{
			for (int i = 0; i < pawns.Count(); i++)
			{
				ApplyHediff(pawns[i], map, hediff, chance);
			}
		}
	}
}
