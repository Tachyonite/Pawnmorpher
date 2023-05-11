// AspectRestrictionExtension.cs modified by Iron Wolf for Pawnmorph on 12/04/2019 8:28 PM
// last updated 12/04/2019  8:28 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	///     restriction def extension that restricts a def to pawns with/without specific aspects
	/// </summary>
	/// <seealso cref="Pawnmorph.DefExtensions.RestrictionExtension" />
	public class AspectRestriction : RestrictionExtension
	{


		/// <summary>
		///     The aspect entries
		/// </summary>
		[NotNull] public List<Entry> aspectEntries = new List<Entry>();


		[Unsaved] private Dictionary<AspectDef, List<int>> _lookupDict = null;

		/// <summary>
		///     gets all errors with this instance
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors()) yield return configError;

			var tmpDict = new Dictionary<AspectDef, List<int>>();
			foreach (Entry aspectEntry in aspectEntries)
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (aspectEntry.aspectDef == null)
				{
					yield return "aspect def is null";
				}
				else
				{
					List<int> lst;
					if (!tmpDict.TryGetValue(aspectEntry.aspectDef, out lst))
					{
						lst = new List<int>();
						tmpDict[aspectEntry.aspectDef] = lst;
					}

					lst.Add(aspectEntry.stageIndex);
				}

			foreach (KeyValuePair<AspectDef, List<int>> keyValuePair in tmpDict)
			{
				List<int> lst = keyValuePair.Value;
				if (lst.Any(i => i < 0) && lst.Any(i => i >= 0)
				) //check if any entries have stage indices less then zero and greater then zero 
					yield return
						$"entry for {keyValuePair.Key.defName} has indices that are both less then zero and greater then or equal to zero!";
			}
		}

		/// <summary>
		///     checks if the given pawn passes the restriction.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     if the def can be used with the given pawn
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		protected override bool PassesRestrictionImpl(Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			bool hasAspect;
			AspectTracker tracker = pawn.GetAspectTracker();
			if (tracker == null)
			{
				hasAspect = false; //no tracker, then they cannot have an aspect 
			}
			else
			{
				hasAspect = false;
				foreach (Aspect aspect in tracker)
				{
					List<int> indexLst = GetStagesForAspect(aspect.def);
					if (indexLst == null) continue;

					if (indexLst.Any(i => i < 0))
					{
						hasAspect = true;
						break;
					}

					if (indexLst.Contains(aspect.StageIndex))
					{
						hasAspect = true;
						break;
					}
				}
			}
			return hasAspect;
		}

		[CanBeNull]
		private List<int> GetStagesForAspect([NotNull] AspectDef aspectDef)
		{
			if (_lookupDict == null)
			{
				_lookupDict = new Dictionary<AspectDef, List<int>>();
				foreach (Entry aspectEntry in aspectEntries)
				{
					if (aspectEntry.aspectDef == null) continue;

					List<int> lst;
					if (!_lookupDict.TryGetValue(aspectEntry.aspectDef, out lst))
					{
						lst = new List<int>();
						_lookupDict[aspectEntry.aspectDef] = lst;
					}

					lst.Add(aspectEntry.stageIndex);
				}
			}

			return _lookupDict.TryGetValue(aspectDef);
		}

		/// <summary>
		///     simple class for storing entries about an aspect and stage
		/// </summary>
		public class Entry
		{
			/// <summary>
			///     The aspect definition to look for
			/// </summary>
			public AspectDef aspectDef;

			/// <summary>
			///     The stage index to look for
			/// </summary>
			/// if less then 0 then any stage will do
			public int stageIndex = -1;
		}
	}
}