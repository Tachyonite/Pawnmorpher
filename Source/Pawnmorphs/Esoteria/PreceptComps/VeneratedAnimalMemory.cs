// VeneratedAnimalMutationThought.cs created by Iron Wolf for Pawnmorph on 07/22/2021 7:05 AM
// last updated 07/22/2021  7:05 AM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Thoughts.Precept;
using RimWorld;
using Verse;

namespace Pawnmorph.PreceptComps
{
	/// <summary>
	///     precept comp for giving a thought based on a venerated animal mutation
	/// </summary>
	/// <seealso cref="RimWorld.PreceptComp" />
	public abstract class VeneratedAnimalMemory : PreceptComp
	{

		/// <summary>
		/// The history event to look for
		/// </summary>
		public HistoryEventDef historyEvent;

		/// <summary>
		///     The thought definition to give
		/// </summary>
		public ThoughtDef thoughtDef;

		/// <summary>
		/// gets all configuration errors with this instance.
		/// </summary>
		/// <param name="parent">The parent.</param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(PreceptDef parent)
		{
			foreach (string configError in base.ConfigErrors(parent)) yield return configError;

			if (thoughtDef == null) yield return "no thought def set";
			if (historyEvent == null) yield return "no historyEvent set";
		}



		/// <summary>
		/// called when a pawn with an ideo with the given precept takes an action or has an action done to them 
		/// </summary>
		/// <param name="ev">The ev.</param>
		/// <param name="precept">The precept.</param>
		/// <param name="canApplySelfTookThoughts">if set to <c>true</c> [can apply self took thoughts].</param>
		public override void Notify_MemberTookAction(HistoryEvent ev, Precept precept, bool canApplySelfTookThoughts)
		{
			base.Notify_MemberTookAction(ev, precept, canApplySelfTookThoughts);
			if (!canApplySelfTookThoughts) return;

			if (ev.def != historyEvent) return;


			Pawn dooer = ev.GetDoer();

			Ideo ideo = dooer.Ideo;
			if (ideo == null) return;
			ThingDef animal = GetAnimal(ev, ideo);
			if (animal == null) return;

			MutationMemory_VeneratedAnimal thought = PMThoughtUtilities.CreateVeneratedAnimalMemory(thoughtDef, animal, precept);
			dooer.TryGainMemory(thought);
		}


		/// <summary>
		/// Gets the animal from the given history event .
		/// </summary>
		/// <param name="historyEvent">The history event.</param>
		/// <param name="ideo">The ideo.</param>
		/// <returns>
		/// the animal from the event. if null the thought will not be given
		/// </returns>
		[CanBeNull]
		protected abstract ThingDef GetAnimal(in HistoryEvent historyEvent, [NotNull] Ideo ideo);
	}
}