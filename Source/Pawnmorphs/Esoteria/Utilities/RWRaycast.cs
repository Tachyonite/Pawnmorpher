// RWRaycast.cs created by Iron Wolf for Pawnmorph on 02/23/2021 3:26 PM
// last updated 02/23/2021  3:26 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Utilities
{
	/// <summary>
	///     static class for performing rimworld raycasts
	/// </summary>
	public static class RWRaycast
	{
		internal static bool debug;

		[NotNull] private static readonly StringBuilder debugBuilder = new StringBuilder();

		[NotNull] private static readonly HashSet<Thing> _testSet = new HashSet<Thing>();

		/// <summary>
		///     gets all targets from the given raycast and store them in the given buffer.
		/// </summary>
		/// <param name="map">The map.</param>
		/// <param name="cell">The cell.</param>
		/// <param name="dir">The dir.</param>
		/// <param name="buffer">The buffer.</param>
		/// <param name="targets">The targets.</param>
		/// <returns>the number of hits</returns>
		public static int RaycastAllNoAlloc([NotNull] Map map, IntVec2 cell, Vector2 dir, [NotNull] RWRaycastHit[] buffer,
											RaycastTargets targets)
		{
			if (map == null) throw new ArgumentNullException(nameof(map));
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			float dSqr = dir.sqrMagnitude;

			if (Math.Abs(dSqr) < 0.01f)
			{
				Log.Warning($"trying to raycast with very small dir {dir}");
				return 0;
			}

			if (debug) debugBuilder.Clear();

			Vector2 dHat = dir.normalized;
			Vector2 cellPos = cell.ToVector2();
			var eCell = new IntVec2(cellPos + dir);

			Vector2 curPos = cellPos;
			IntVec2 curCell = cell;
			var hits = 0;

			if (debug) debugBuilder.AppendLine($"curPos:{curCell} dir:{dir} dHat:{dHat} eCell{eCell}");

			_testSet.Clear();

			while (hits < buffer.Length && eCell != curCell && (curCell - cell).ToVector2().sqrMagnitude < dSqr)
			{
				Vector2 lPos = curPos;
				curPos += dHat;
				var tCell = new IntVec2(curPos);

				if (debug) debugBuilder.AppendLine($"testing {tCell} from  {lPos} + {dHat} :  curCell:{curCell}");

				if (tCell != curCell)
				{
					if (!tCell.ToIntVec3.InBounds(map)) break;
					TestCell(map, tCell, targets, buffer, ref hits);
					curCell = tCell;
				}
			}

			if (debug)
			{
				if (hits > 0)
					debugBuilder.AppendLine($"hits:[{hits}:{string.Join(",", buffer.Take(hits))}]");
				else
					debugBuilder.AppendLine("no hits");
				Log.Message(debugBuilder.ToString());
			}

			return hits;
		}


		/// <summary>
		///     gets all targets from the given raycast and store them in the given buffer.
		/// </summary>
		/// <param name="map">The map.</param>
		/// <param name="p0">The p0.</param>
		/// <param name="p1">The p1.</param>
		/// <param name="buffer">The buffer.</param>
		/// <param name="targets">The targets.</param>
		/// <returns>the number of hits</returns>
		public static int RaycastAllNoAlloc([NotNull] Map map, IntVec3 p0, IntVec3 p1, [NotNull] RWRaycastHit[] buffer,
											RaycastTargets targets)
		{
			IntVec2 p0IV2 = p0.ToIntVec2;
			IntVec2 p1IV2 = p1.ToIntVec2;
			Vector2 dir = p1IV2.ToVector2() - p0IV2.ToVector2();
			return RaycastAllNoAlloc(map, p0IV2, dir, buffer, targets);
		}

		private static void TestCell(Map map, IntVec2 cell, RaycastTargets targets, RWRaycastHit[] buffer, ref int hits)
		{
			ThingGrid thingGrid = map.thingGrid;

			List<Thing> things = thingGrid.ThingsListAt(cell.ToIntVec3);


			if (things == null || things.Count == 0) return;

			foreach (Thing thing in things)
			{
				if (_testSet.Contains(thing)) continue;
				_testSet.Add(thing);

				if (debug) debugBuilder.AppendLine($"testing cell:{cell} found {thing.Label}");

				if (targets == RaycastTargets.All)
				{
					buffer[hits] = new RWRaycastHit { hitThing = thing };
					hits++;
				}
				else
				{
					if ((targets & RaycastTargets.Impassible) != 0 && thing.def?.passability == Traversability.Impassable)
					{
						buffer[hits] = new RWRaycastHit { hitThing = thing };
						hits++;
					}
					else if ((targets & RaycastTargets.Walls) != 0 && thing.def == ThingDefOf.Wall)
					{
						buffer[hits] = new RWRaycastHit { hitThing = thing };
						hits++;
					}
					else if ((targets & RaycastTargets.Pawns) != 0 && thing is Pawn p)
					{
						buffer[hits] = new RWRaycastHit { hitThing = thing, hitPawn = p };
						hits++;
					}
				}

				if (hits == buffer.Length) break;
			}
		}
	}


	/// <summary>
	/// </summary>
	public struct RWRaycastHit
	{
		/// <summary>
		///     The thing hit
		/// </summary>
		public Thing hitThing;


		/// the hit pawn
		public Pawn hitPawn;

		/// <summary>
		///     Performs an explicit conversion from <see cref="RWRaycastHit" /> to <see cref="System.Boolean" />.
		/// </summary>
		/// <param name="hit">The hit.</param>
		/// <returns>
		///     The result of the conversion.
		/// </returns>
		public static explicit operator bool(RWRaycastHit hit)
		{
			return hit.hitThing != null;
		}

		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>The fully qualified type name.</returns>
		public override string ToString()
		{
			return $"{hitThing?.Label ?? "NO HIT"}";
		}
	}

	/// <summary>
	/// </summary>
	[Flags]
	public enum RaycastTargets
	{
		/// <summary>
		///     All
		/// </summary>
		All = ~0,

		/// <summary>
		///     none
		/// </summary>
		None = 0,

		/// <summary>
		///     walls
		/// </summary>
		Walls = 1 << 0,

		/// <summary>
		///     pawns
		/// </summary>
		Pawns = 1 << 1,

		/// <summary>
		///     impassible stuff
		/// </summary>
		Impassible = 1 << 2
	}
}