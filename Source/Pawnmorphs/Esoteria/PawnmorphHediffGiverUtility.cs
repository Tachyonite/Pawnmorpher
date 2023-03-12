using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// collection of utilities around hediff givers
	/// </summary>
	public static class PawnmorphHediffGiverUtility
	{
		private static List<BodyPartRecord> _scratchList = new List<BodyPartRecord>();

		/// <summary>Tries to apply the given hediff to the given pawn</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="hediff">The hediff.</param>
		/// <param name="partsToAffect">The parts to affect.</param>
		/// <param name="canAffectAnyLivePart">if set to <c>true</c> [can affect any live part].</param>
		/// <param name="countToAffect">The count to affect.</param>
		/// <param name="outAddedHediffs">The out added hediffs.</param>
		/// <returns></returns>
		public static bool TryApply(Pawn pawn, HediffDef hediff, List<BodyPartDef> partsToAffect = null, bool canAffectAnyLivePart = false, int countToAffect = 1, List<Hediff> outAddedHediffs = null)
		{
			try
			{
				if (canAffectAnyLivePart || partsToAffect != null)
				{
					bool result = false;
					for (int i = 0; i < countToAffect; i++)
					{
						IEnumerable<BodyPartRecord> source = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null);

						if (partsToAffect != null)
						{
							source = from p in source
									 where partsToAffect.Contains(p.def)
									 select p;
						}

						if (canAffectAnyLivePart)
						{
							source = from p in source
									 where p.def.alive
									 select p;
						}

						source = from p in source
								 where !pawn.health.hediffSet.HasHediff(hediff, p, false)
									&& !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(p)
								 select p;
						_scratchList.Clear();
						_scratchList.AddRange(source); //use a scratch list here, can't enumerate over IEnumerable more then once in all cases 

						if (_scratchList.Count == 0)
						{
							break;
						}

						BodyPartRecord partRecord = _scratchList[0];
						Hediff hediff2 = HediffMaker.MakeHediff(hediff, pawn, partRecord);
						pawn.health.AddHediff(hediff2, null, null, null);
						if (outAddedHediffs != null)
						{
							outAddedHediffs.Add(hediff2);
						}

						result = true;
					}

					return result;
				}

				if (!pawn.health.hediffSet.HasHediff(hediff, false))
				{
					Hediff hediff3 = HediffMaker.MakeHediff(hediff, pawn, null);
					pawn.health.AddHediff(hediff3, null, null, null);
					if (outAddedHediffs != null)
					{
						outAddedHediffs.Add(hediff3);
					}

					return true;
				}

				return false;
			}
			catch (Exception exception)
			{
				Log.Warning($"exception {exception.GetType().Name} caught while giving {hediff.defName} to {pawn.Name}, message follows \n{exception}");

				return false;
			}
		}
	}
}
