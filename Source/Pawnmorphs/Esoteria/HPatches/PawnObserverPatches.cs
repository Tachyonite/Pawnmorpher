// PawnObserverPatches.cs created by Iron Wolf for Pawnmorph on 07/05/2021 12:43 PM
// last updated 07/05/2021  12:43 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	/// <summary>
	/// static class for pawn observer patching 
	/// </summary>
	public static class PawnObserverPatches
	{
		[NotNull]
		private readonly static ThingDef Chaothrumbo;

		static PawnObserverPatches()
		{
			Chaothrumbo = DefDatabase<ThingDef>.GetNamed("PM_Chaothrumbo");
		}

		[NotNull]
		private static readonly Type patchType = typeof(PawnObserver);
		/// <summary>
		/// Preforms the patches.
		/// </summary>
		/// <param name="harInstance">The har instance.</param>
		public static void PreformPatches([NotNull] Harmony harInstance)
		{
			try
			{
				var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
				var pfMethod = typeof(PawnObserverPatches).GetMethod(nameof(ObserveSurroundingPostfix), flags);

				var method = patchType.GetMethods(flags)
									  .Where(m => m.HasAttribute<CompilerGeneratedAttribute>())
									  .First(m => m.ReturnType == typeof(bool) && m.HasSignature(typeof(Region)));

				harInstance.Patch(method, postfix: new HarmonyMethod(pfMethod));

			}
			catch (InvalidOperationException e)
			{
				Log.Error($"unable to patch PawnObserver\nerror: {e}");
			}
		}

		private static bool PossibleToObserve(Pawn pawn, Thing thing)
		{
			if (thing.Position.InHorDistOf(pawn.Position, 5f))
			{
				return GenSight.LineOfSight(thing.Position, pawn.Position, pawn.Map, skipFirstCell: true);
			}
			return false;
		}
		static void ObserveSurroundingPostfix(Region reg, PawnObserver __instance, List<Thought_MemoryObservationTerror> ___terrorThoughts, Pawn ___pawn)
		{
			//TODO figure out a generic fix that doesn't hard code chaothrumbos 

			foreach (Thing thing in reg.ListerThings.ThingsOfDef(Chaothrumbo))
			{
				if (!PossibleToObserve(___pawn, thing)) continue;
				TryCreateObservedThought(thing);
				TryCreateObservedHistoryEvent(thing);
			}


			void TryCreateObservedHistoryEvent(Thing thing)
			{
				IObservedThoughtGiver observedThoughtGiver;
				if ((observedThoughtGiver = thing as IObservedThoughtGiver) != null)
				{
					HistoryEventDef historyEventDef = observedThoughtGiver.GiveObservedHistoryEvent(___pawn);
					if (historyEventDef != null)
					{
						HistoryEvent historyEvent = new HistoryEvent(historyEventDef, ___pawn.Named(HistoryEventArgsNames.Doer), thing.Named(HistoryEventArgsNames.Subject));
						Find.HistoryEventsManager.RecordEvent(historyEvent);
					}
				}
			}
			void TryCreateObservedThought(Thing thing)
			{
				if (TerrorUtility.TryCreateTerrorThought(thing, out var thought))
				{
					___terrorThoughts.Add(thought);
				}
				IObservedThoughtGiver observedThoughtGiver2;
				if ((observedThoughtGiver2 = thing as IObservedThoughtGiver) != null)
				{
					Thought_Memory thought_Memory = observedThoughtGiver2.GiveObservedThought(___pawn);
					if (thought_Memory != null)
					{
						___pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
					}
				}
			}


		}

	}
}