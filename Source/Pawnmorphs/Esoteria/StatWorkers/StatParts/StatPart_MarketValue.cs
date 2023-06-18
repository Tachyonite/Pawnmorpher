using Pawnmorph.Things;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.StatWorkers.StatParts
{
	/// <summary>
	/// Market value stat part. Used to apply market value modifiers from Pawnmorpher concepts.
	/// </summary>
	/// <seealso cref="RimWorld.StatPart" />
	internal class StatPart_MarketValue : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{

			if (req.HasThing)
			{
				if (req.Thing is Genome_Animal genome)
				{
					// Modify value based on contained animal type
					PawnKindDef animalKind = genome.AnimalKind;
					if (animalKind == null)
						return;

					val += animalKind.race.BaseMarketValue;
				}
				else if (req.Thing is Pawn pawn)
				{
					// Market value modifiers added by mutations to pawns.
					if (pawn.RaceProps.Animal == false) //  && MutationUtilities.CanApplyMutations
					{
						MutationTracker tracker = pawn.GetMutationTracker();
						if (tracker != null)
						{
							IList<Hediff_AddedMutation> mutations = tracker.AllMutations;
							if (mutations != null)
							{
								for (int i = mutations.Count - 1; i >= 0; i--)
								{
									val += mutations[i].Def.GetMarketValueFor();
								}
							}
						}
					}
				}
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (req.HasThing)
			{
				if (req.Thing is Genome_Animal genome)
				{
					// Market value modifiers added by selected animal in animal genomes.
					PawnKindDef animalKind = genome.AnimalKind;
					if (animalKind == null)
						return null;

					return $"{animalKind.LabelCap}: +${animalKind.race.BaseMarketValue}";
				}
				else if (req.Thing is Pawn pawn)
				{
					// Market value modifiers added by mutations to pawns.
					if (pawn.RaceProps.Animal == false) //  && MutationUtilities.CanApplyMutations
					{
						MutationTracker tracker = pawn.GetMutationTracker();
						if (tracker != null)
						{
							string text = "";
							IList<Hediff_AddedMutation> mutations = tracker.AllMutations;
							if (mutations != null)
							{
								Hediff_AddedMutation mutation;
								for (int i = mutations.Count - 1; i >= 0; i--)
								{
									mutation = mutations[i];

									if (text.Length > 0)
										text += Environment.NewLine;

									text += $"{mutation.LabelCap}: +${mutation.Def.GetMarketValueFor()}";
								}
							}
						}
					}
				}
			}
			return null;
		}

	}
}
