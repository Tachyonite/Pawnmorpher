// SelfTookMemoryThought_RequiresMeme.cs created by Iron Wolf for Pawnmorph on 07/22/2021 5:00 PM
// last updated 07/22/2021  5:00 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;

namespace Pawnmorph.PreceptComps
{
	/// <summary>
	///     self took thought that provides overrides for given memes
	/// </summary>
	/// <seealso cref="RimWorld.PreceptComp_SelfTookMemoryThought" />
	public class SelfTookMemoryThought_MemeOverride : PreceptComp_SelfTookMemoryThought
	{
		/// <summary>
		///     The entries
		/// </summary>
		[UsedImplicitly] protected List<MemeThoughtEntry> entries = new List<MemeThoughtEntry>();

		[Unsaved] private List<ValueTuple<MemeDef, ThoughtDef>> _cachedList;

		[NotNull]
		private IEnumerable<ValueTuple<MemeDef, ThoughtDef>> CachedList
		{
			get
			{
				if (_cachedList == null) _cachedList = entries.Select(p => (ValueTuple<MemeDef, ThoughtDef>)p).ToList();

				return _cachedList;
			}
		}

		/// <summary>
		///     gets all configuration errors.
		/// </summary>
		/// <param name="parent">The parent.</param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(PreceptDef parent)
		{
			foreach (string configError in base.ConfigErrors(parent)) yield return configError;

			if (entries == null) yield return "null entry list";
			else
				foreach (MemeThoughtEntry memeThoughtEntry in entries)
				{
					if (memeThoughtEntry == null)
					{
						yield return "null entry in entry list";
						continue;
					}

					foreach (string configError in memeThoughtEntry.ConfigErrors(parent)) yield return configError;
				}
		}

		/// <summary>
		///     called when a member takes a specific action
		/// </summary>
		/// <param name="ev">The ev.</param>
		/// <param name="precept">The precept.</param>
		/// <param name="canApplySelfTookThoughts">if set to <c>true</c> [can apply self took thoughts].</param>
		public override void Notify_MemberTookAction(HistoryEvent ev, Precept precept, bool canApplySelfTookThoughts)
		{
			if (ev.def != eventDef || !canApplySelfTookThoughts)
				return;
			var p = ev.args.GetArg<Pawn>(HistoryEventArgsNames.Doer);
			if (p.needs == null
			 || p.needs.mood == null
			 || onlyForNonSlaves && p.IsSlave
			 || thought.minExpectationForNegativeThought != null
			 && ExpectationsUtility.CurrentExpectationFor(p).order < thought.minExpectationForNegativeThought.order)
				return;
			Thought_Memory newThought = ThoughtMaker.MakeThought(GetBestThoughtFor(p), precept);
			Pawn animal;
			if (newThought is Thought_KilledInnocentAnimal killedInnocentAnimal
			 && ev.args.TryGetArg(HistoryEventArgsNames.Victim, out animal))
				killedInnocentAnimal.SetAnimal(animal);
			Corpse corpse;
			if (newThought is Thought_MemoryObservation memoryObservation
			 && ev.args.TryGetArg(HistoryEventArgsNames.Subject, out corpse))
				memoryObservation.Target = corpse;
			p.needs.mood.thoughts.memories.TryGainMemory(newThought);
		}

		/// <summary>
		///     Determines whether this instance can give the given thought to the pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="tDef">The t definition.</param>
		/// <returns>
		///     <c>true</c> if this instance can give the given thought to the pawn  otherwise, <c>false</c>.
		/// </returns>
		protected virtual bool CanGiveThought([NotNull] Pawn pawn, [NotNull] ThoughtDef tDef)
		{
			return tDef.IsValidFor(pawn);
		}

		/// <summary>
		/// Gets the best thought for.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected ThoughtDef GetBestThoughtFor(Pawn pawn)
		{
			if (!CachedList.TryGetMemeVariant(pawn, out ThoughtDef tDef, CanGiveThought)) return thought;

			return tDef;
		}

		/// <summary>
		///     meme thought entry class
		/// </summary>
		protected class MemeThoughtEntry
		{
			/// <summary>
			///     The meme
			/// </summary>
			public MemeDef meme;

			/// <summary>
			///     The thought
			/// </summary>
			public ThoughtDef thought;

			/// <summary>
			///     gets all configuration errors.
			/// </summary>
			/// <param name="parent">The parent.</param>
			/// <returns></returns>
			[NotNull]
			public IEnumerable<string> ConfigErrors(PreceptDef parent)
			{
				if (meme == null) yield return "no meme def given";
				if (thought == null) yield return "no thought given";
			}

			/// <summary>
			///     Performs an explicit conversion from <see cref="MemeThoughtEntry" /> to
			///     <see cref="ValueTuple{MemeDef, ThoughtDef}" />.
			/// </summary>
			/// <param name="entry">The entry.</param>
			/// <returns>
			///     The result of the conversion.
			/// </returns>
			public static explicit operator ValueTuple<MemeDef, ThoughtDef>(MemeThoughtEntry entry)
			{
				return (entry?.meme, entry?.thought);
			}
		}
	}
}