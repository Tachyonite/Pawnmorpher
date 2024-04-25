// ChaomorphUtilities.cs created by Iron Wolf for Pawnmorph on 09/26/2020 5:48 PM
// last updated 09/26/2020  5:48 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// static container for general chaomorph utilities 
	/// </summary>
	[StaticConstructorOnStartup]
	public static class ChaomorphUtilities
	{
		[NotNull]
		private static readonly List<ThingDef>[] _chaomorphArr;

		[NotNull]
		private static readonly List<ThingDef>[] _randomChaomorphArrCache;

		[NotNull]
		private static Dictionary<ThingDef, ChaomorphExtension> _cachedExtensions =
			new Dictionary<ThingDef, ChaomorphExtension>();

		[NotNull]
		private static readonly
			Dictionary<ThingDef, PawnKindDef> _pawnKindLookup = new Dictionary<ThingDef, PawnKindDef>();

		[NotNull]
		private static readonly float[] _totals;


		/// <summary>
		/// Gets the chaomorphs of the given type 
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		[NotNull]
		public static IReadOnlyList<ThingDef> GetChaomorphs(ChaomorphType type)
		{
			return _chaomorphArr[(int)type];
		}


		/// <summary>
		/// Gets a random chaomorph.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		[CanBeNull]
		public static ThingDef GetRandomChaomorph(ChaomorphType type)
		{
			float rN = Rand.Range(0, _totals[(int)type]);
			ThingDef selectedChaomorph = null;
			foreach (ThingDef chaomorph in _randomChaomorphArrCache[(int)type])
			{
				ChaomorphExtension ext = _cachedExtensions[chaomorph];
				if (rN <= ext.selectionWeight)
				{
					selectedChaomorph = chaomorph;
					break;
				}

				rN -= ext.selectionWeight;
			}

			return selectedChaomorph;

		}

		/// <summary>
		/// Gets a random chaomorph pawnkind def.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>the pawnkind def, null if none is available</returns>
		[CanBeNull]
		public static PawnKindDef GetRandomChaomorphPK(ChaomorphType type)
		{
			ThingDef thingDef = GetRandomChaomorph(type);
			if (thingDef == null) return null;
			foreach (PawnKindDef pkDef in DefDatabase<PawnKindDef>.AllDefs)
				if (pkDef.race == thingDef)
					return pkDef; //handle multiple pawnkinds per chaomorph? 

			return null;
		}

		/// <summary>
		/// Determines whether this instance is a chaomorph.
		/// </summary>
		/// <param name="race">The race.</param>
		/// <returns>
		///   <c>true</c> if the specified race is chaomorph; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsChaomorph([NotNull] this ThingDef race)
		{
			return _cachedExtensions.ContainsKey(race);
		}

		static ChaomorphUtilities()
		{
			var chaoTypes = Enum.GetValues(typeof(ChaomorphType)).OfType<ChaomorphType>().ToArray();
			_chaomorphArr = new List<ThingDef>[chaoTypes.Length];
			_cachedExtensions = new Dictionary<ThingDef, ChaomorphExtension>();
			_randomChaomorphArrCache = new List<ThingDef>[_chaomorphArr.Length];
			for (int i = 0; i < chaoTypes.Length; i++)
			{
				_chaomorphArr[i] = new List<ThingDef>();
				_randomChaomorphArrCache[i] = new List<ThingDef>();
			}


			foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
			{
				var ext = thingDef.GetModExtension<ChaomorphExtension>();
				if (ext == null) continue;


				PawnKindDef pk;
				if (ext.pawnKindDef == null)
				{
					pk = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(p => p.race == thingDef);
					if (pk == null)
					{
						Log.Warning($"unable to find pawnkind def for {thingDef.defName}");
						continue;
					}

					ext.pawnKindDef = pk;
				}
				else
				{
					pk = ext.pawnKindDef;
				}

				_pawnKindLookup[thingDef] = pk;

				if (thingDef.race == null)
				{
					Log.Error($"trying to add invalid chaomorph {thingDef.defName}! chaomorphs must have defined race properties!");
				}
				var lst = _chaomorphArr[(int)ext.chaoType];
				lst.Add(thingDef);
				_cachedExtensions[thingDef] = ext;
			}

			_totals = new float[_chaomorphArr.Length];

			for (int i = 0; i < _chaomorphArr.Length; i++)
			{
				var morphs = _chaomorphArr[i];
				float total = 0;
				foreach (ThingDef thingDef in morphs)
				{
					if (!_cachedExtensions.TryGetValue(thingDef, out var ext)) continue;
					if (ext.selectionWeight <= 0) continue;
					total += ext.selectionWeight;
					_randomChaomorphArrCache[i].Add(thingDef);
				}

				_totals[i] = total;
			}

		}

	}
}