// RandomGiver.cs modified by Iron Wolf for Pawnmorph on 12/16/2019 12:36 PM
// last updated 12/16/2019  12:36 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Aspects
{
	/// <summary>
	///     a aspect giver that gives random aspects
	/// </summary>
	/// <seealso cref="Pawnmorph.AspectGiver" />
	public class RandomGiver : AspectGiver
	{
		/// <summary>
		///     The entries
		/// </summary>
		[NotNull] public List<Entry> entries = new List<Entry>();

		/// <summary>
		///     Gets the aspects available to be given to pawns.
		/// </summary>
		/// <value>
		///     The available aspects.
		/// </value>
		public override IEnumerable<AspectDef> AvailableAspects => entries.Select(e => e.aspect);

		/// <summary>
		///     get all configuration errors with this instance
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (Entry entry in entries)
				if (entry.aspect == null)
					yield return "aspectDef is null";
		}


		/// <summary>
		///     Tries to give aspects to the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="outList">if not null, all given aspects will be placed into the list</param>
		/// <returns>if any aspects were successfully given to the pawn</returns>
		public override bool TryGiveAspects(Pawn pawn, List<Aspect> outList = null)
		{
			var anyApplied = false;
			foreach (Entry entry in entries)
				if (Rand.Value < entry.chance)
					anyApplied |= ApplyAspect(pawn, entry.aspect, entry.aspectStage, outList);

			return anyApplied;
		}

		/// <summary>
		///     Tries to give a single aspect to the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>the aspect if any was successfully given to the pawn</returns>
		public Aspect GiveOneAspect(Pawn pawn)
		{
			float totalChance = entries.Sum(e => e.chance);
			float chanceMult = 1f / totalChance;
			float randValue = Rand.Value;

			float chanceAccum = 0;
			List<Aspect> outList = new List<Aspect>();
			foreach (Entry entry in entries)
			{
				if (randValue < (chanceAccum + (entry.chance * chanceMult)))
				{
					ApplyAspect(pawn, entry.aspect, entry.aspectStage, outList);
					break;
				}
				else
					chanceAccum += (entry.chance * chanceMult);
			}

			return outList.FirstOrDefault();
		}

		/// <summary>
		///     simple class for storing individual entries in the giver
		/// </summary>
		public class Entry
		{
			/// <summary>
			///     The aspect to give
			/// </summary>
			public AspectDef aspect;

			/// <summary>
			///     The percent chance to give the aspect
			/// </summary>
			/// note 1 / percent is the expected number of mutations needed before the aspect is added
			public float chance;

			/// <summary>
			///     The aspect stage
			/// </summary>
			public int aspectStage;

			//TODO message or letter information? 
		}
	}
}