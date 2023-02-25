// Memory_FactionObservation.cs created by Iron Wolf for Pawnmorph on 06/26/2021 9:51 AM
// last updated 06/26/2021  9:51 AM

using System;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	///     memory of an observed thing that depends on the relative factions of the observer and observed
	/// </summary>
	/// <seealso cref="RimWorld.Thought_MemoryObservation" />
	public class Memory_FactionObservation : Thought_MemoryObservation
	{
		private Thing _observedThing;

		/// <summary>
		///     Gets or sets the observed thing.
		/// </summary>
		/// <value>
		///     The observed thing.
		/// </value>
		[CanBeNull]
		public Thing ObservedThing
		{
			get => _observedThing;
			set
			{
				_observedThing = value;
				if (_observedThing == null) return;

				Target = value;
			}
		}

		/// <summary>
		/// Gets the index of the current stage.
		/// </summary>
		/// <value>
		/// The index of the current stage.
		/// </value>
		public override int CurStageIndex
		{
			get
			{
				var relation = Relation.Neutral;
				Thing thing = ObservedThing;
				if (thing != null && pawn != null)
				{
					Faction pawnFaction = pawn.Faction;
					if (pawnFaction != null)
					{
						Faction thingFaction = thing.Faction;
						if (thingFaction == pawnFaction)
							relation = Relation.Ally;
						else if (thingFaction != null)
						{
							var rel = pawnFaction.RelationKindWith(thingFaction);
							switch (rel)
							{
								case FactionRelationKind.Hostile:
									relation = Relation.Hostile;
									break;
								case FactionRelationKind.Neutral:
									relation = Relation.Neutral;
									break;
								case FactionRelationKind.Ally:
									relation = Relation.Ally;
									break;
								default:
									throw new ArgumentOutOfRangeException();
							}
						}
						else if (thing.HostileTo(pawn)) relation = Relation.Hostile;
					}
				}

				return Mathf.Min((int)relation, def.stages.Count - 1);
			}
		}


		/// <summary>
		///     Exposes the data.
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref _observedThing, nameof(ObservedThing));
		}

		private enum Relation
		{
			Neutral = 0,
			Ally,
			Hostile
		}
	}
}