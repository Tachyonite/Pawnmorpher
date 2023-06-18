// BodyUtilities.cs created by Iron Wolf for Pawnmorph on 03/16/2020 7:15 PM
// last updated 03/16/2020  7:15 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// a collection of various body related utilities 
	/// </summary>
	public static class BodyUtilities
	{
		[NotNull]
		private static readonly Dictionary<BodyPartRecord, PartAddress> _internDict =
			new Dictionary<BodyPartRecord, PartAddress>();

		/// <summary>
		///     Gets the part address for this body part record.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">record</exception>
		[NotNull]
		public static PartAddress GetAddress([NotNull] this BodyPartRecord record)
		{
			if (record == null) throw new ArgumentNullException(nameof(record));
			//save this so we don't have to do unneeded calculations, part records should never change
			if (_internDict.TryGetValue(record, out PartAddress addr)) return addr;


			var lst = new List<string>();
			BodyPartRecord node = record;
			while (node != null)
			{
				lst.Add(node.Label);
				node = node.parent;
			}

			lst.Reverse();
			return new PartAddress(lst);
		}


		/// <summary>
		///     Gets the body part record at the given part address
		/// </summary>
		/// <param name="bodyDef">The body definition.</param>
		/// <param name="address">The address.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		///     bodyDef
		///     or
		///     address
		/// </exception>
		[CanBeNull]
		public static BodyPartRecord GetRecordAt([NotNull] this BodyDef bodyDef, [NotNull] PartAddress address)
		{
			if (bodyDef == null) throw new ArgumentNullException(nameof(bodyDef));
			if (address == null) throw new ArgumentNullException(nameof(address));

			if (address.Count == 0) return null;
			BodyPartRecord curRecord = bodyDef.corePart;
			if (curRecord.Label != address[0]) return null;

			for (var i = 1; i < address.Count; i++)
			{
				curRecord = curRecord.parts.MakeSafe().FirstOrDefault(p => p.Label == address[i]);
				if (curRecord == null) break;
			}

			return curRecord;
		}


		/// <summary>
		/// Gets a body part that is equivalent to partRecord from the given bodyDef, if one exists.
		/// </summary>
		/// <param name="bodyDef">The body def to check</param>
		/// <param name="partRecord">The body part to search for</param>
		/// <returns>The matching <see cref="BodyPartRecord"/> from bodyDef, if one exists, or null otherwise</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[CanBeNull]
		public static BodyPartRecord GetRecord([NotNull] this BodyDef bodyDef, [NotNull] BodyPartRecord partRecord)
		{
			if (bodyDef == null) throw new ArgumentNullException(nameof(bodyDef));
			if (partRecord == null) throw new ArgumentNullException(nameof(partRecord));


			return bodyDef.GetPartsWithDef(partRecord.def).FirstOrDefault(x => x.Label == partRecord.Label);
		}

		/// <summary>
		/// Gets all non missing parts of the given part defs 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="partDefs">The part defs.</param>
		/// <returns></returns>
		[NotNull]
		public static IEnumerable<BodyPartRecord> GetAllNonMissingParts([NotNull] this Pawn pawn,
																		[NotNull] IEnumerable<BodyPartDef> partDefs)
		{
			var lookup = partDefs.ToList();
			foreach (BodyPartRecord record in pawn.health.hediffSet.GetAllNonMissingWithoutProsthetics())
			{
				if (lookup.Contains(record.def))
				{
					yield return record;
				}
			}
		}

		/// <summary>
		/// Gets the part health multiplier that is applied to negative things on this part.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="record">The record.</param>
		/// <returns></returns>
		public static float GetPartHealthMultiplier([NotNull] Pawn p, [NotNull] BodyPartRecord record)
		{
			const float e0 = 0.4f;
			const float e1 = 1.5f;
			float nHealth = GetPartNormalizedHealth(record, p);
			nHealth = 1 - Mathf.Clamp01((nHealth - e0) / (e1 - e0));
			return MathUtilities.SmoothStep(0, 1, nHealth);
		}


		/// <summary>
		/// Gets the normalized part health.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <param name="p">The p.</param>
		/// <param name="trueNormal">if set to <c>true</c> take mutations into account with 1 being completely healed, otherwise mutations that add health can push this value beyond 1.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">record
		/// or
		/// p</exception>
		/// this is usually a value between 0-1, where 1 is full health, some mutations can push this value beyond 1
		public static float GetPartNormalizedHealth([NotNull] BodyPartRecord record, [NotNull] Pawn p, bool trueNormal = false)
		{
			if (record == null) throw new ArgumentNullException(nameof(record));
			if (p == null) throw new ArgumentNullException(nameof(p));

			var mHealth = trueNormal ? GetPartMaxHealth(record, p) : record.def.GetMaxHealth(p);
			var curHealth = p.health?.hediffSet?.GetPartHealth(record) ?? 0;
			return curHealth / mHealth;
		}


		/// <summary>
		/// Gets the maximum health of the given record for the given pawn 
		/// </summary>
		/// Note: this is used by a transpiler, do not re order arguments without fixing HediffSetPatches.GetPartHealthTranspiler as well
		/// <param name="p">The p.</param>
		/// <param name="record">The record.</param>
		/// <returns></returns>
		public static float GetPartMaxHealth([NotNull] BodyPartRecord record, [NotNull] Pawn p)
		{
			if (p == null) throw new ArgumentNullException(nameof(p));
			if (record == null) throw new ArgumentNullException(nameof(record));

			float maxPartHealth = record.def.GetMaxHealth(p);

			if (p.def.race.Animal || p.def.race.IsMechanoid)
				return maxPartHealth;

			if (p.def.TryGetRaceMutationSettings()?.immuneToAll == true)
				return maxPartHealth;

			MutationTracker mTracker = p.GetMutationTracker(); //use mTracker so we only check mutations, a bit faster 
			if (mTracker == null)
				return maxPartHealth;

			float offset = 0;
			float multiplier = 0;
			foreach (Hediff_AddedMutation mutation in mTracker.AllMutations)
			{
				MutationStage mStage = mutation.CurStage as MutationStage;

				if (mStage == null)
					continue;

				if (mStage.globalHealthMultiplier != 0)
					multiplier += mStage.globalHealthMultiplier;

				if (mutation.Part != record)
					continue;

				offset += mStage.healthOffset;
			}

			return Mathf.Ceil((offset * p.HealthScale
			  + maxPartHealth) * (multiplier > 0 ? multiplier : 1)); //multiplying out health scale like this in case any mods patch BodyPartDef.GetMaxHealth
		}


		/// <summary>
		/// Gets all non missing parts on this pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		[NotNull]
		public static IEnumerable<BodyPartRecord> GetAllNonMissingParts([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			return (pawn.health?.hediffSet?.GetAllNonMissingWithoutProsthetics()).MakeSafe();
		}

		/// <summary>
		/// Gets all non missing parts of the given part defs 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="partDefs">The part defs.</param>
		/// <returns></returns>
		[NotNull]
		public static IEnumerable<BodyPartRecord> GetAllNonMissingParts([NotNull] this Pawn pawn,
																		[NotNull] IReadOnlyList<BodyPartDef> partDefs)
		{

			foreach (BodyPartRecord record in pawn.health.hediffSet.GetAllNonMissingWithoutProsthetics())
			{
				if (partDefs.Contains(record.def))
				{
					yield return record;
				}
			}
		}
	}
}