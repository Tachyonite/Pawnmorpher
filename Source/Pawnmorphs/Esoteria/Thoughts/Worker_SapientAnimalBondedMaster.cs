// Worker_SapientAnimalBondedMaster.cs modified by Iron Wolf for Pawnmorph on 12/22/2019 8:47 PM
// last updated 12/22/2019  8:47 PM

using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{

	/// <summary>
	/// abstract base class for both the sapient animal bonded master and bonded non master thoughts 
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker" />
	public abstract class Worker_SapientAnimalBondedBase : ThoughtWorker
	{
		/// <summary>
		/// Determines whether [is valid relation] [the specified relation].
		/// </summary>
		/// <param name="relation">The relation.</param>
		/// <returns>
		///   <c>true</c> if [is valid relation] [the specified relation]; otherwise, <c>false</c>.
		/// </returns>
		protected bool IsValidRelation(DirectPawnRelation relation)
		{
			var otherPawn = relation?.otherPawn;
			if (otherPawn == null) return false;
			if (otherPawn.RaceProps?.Animal == true) return false; //ignore bonds to former humans 
			if (otherPawn.Dead) return false;
			return relation.def == PawnRelationDefOf.Bond;
		}


	}

	/// <summary>
	/// 
	/// </summary>
	public class Worker_SapientAnimalBondedMaster : Worker_SapientAnimalBondedBase
	{
		/// <summary>
		/// gets the current state .
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			SapienceLevel? sapienceLevel = p.GetQuantizedSapienceLevel();
			if (sapienceLevel == null || sapienceLevel == SapienceLevel.Feral || sapienceLevel == SapienceLevel.PermanentlyFeral) return false;

			if (p.Faction != Faction.OfPlayer) return false;

			bool isBonded = false;
			bool masterToBondedPawn = false;
			foreach (DirectPawnRelation relationsDirectRelation in p.relations.DirectRelations)
			{
				if (!IsValidRelation(relationsDirectRelation)) continue;
				Pawn otherPawn = relationsDirectRelation.otherPawn;
				isBonded = true;//we can have only 1 bonded relationship 
				masterToBondedPawn = otherPawn == p.playerSettings?.RespectedMaster;
				break;
			}

			if (isBonded && masterToBondedPawn)
			{
				var stage = Mathf.Min(def.stages.Count - 1, (int)sapienceLevel);
				return ThoughtState.ActiveAtStage(stage);
			}

			return false;

		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class Worker_SapientAnimalBondedNonMaster : Worker_SapientAnimalBondedBase
	{

		/// <summary>
		///gets the current state .
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			SapienceLevel? sapienceLevel = p.GetQuantizedSapienceLevel();
			if (sapienceLevel == null || sapienceLevel == SapienceLevel.Feral || sapienceLevel == SapienceLevel.PermanentlyFeral) return false;
			if (p.Faction != Faction.OfPlayer) return false;

			bool isBonded = false;
			bool masterToBondedPawn = false;
			foreach (DirectPawnRelation relationsDirectRelation in p.relations.DirectRelations)
			{
				if (!IsValidRelation(relationsDirectRelation)) continue;
				isBonded = true;//we can have only 1 bonded relationship 
				masterToBondedPawn = relationsDirectRelation.otherPawn == p.playerSettings?.RespectedMaster;
				break;
			}

			if (isBonded && !masterToBondedPawn)
			{
				var stage = Mathf.Min(def.stages.Count - 1, (int)sapienceLevel);
				return ThoughtState.ActiveAtStage(stage);
			}

			return false;

		}
	}
}