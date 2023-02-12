// HasMorphStat.cs created by Iron Wolf for Pawnmorph on 09/01/2021 5:21 PM
// last updated 09/01/2021  5:21 PM

using RimWorld;
using Verse;

namespace Pawnmorph.StatWorkers
{
	/// <summary>
	/// stat worker for the utility stat HasMorph 
	/// </summary>
	/// <seealso cref="RimWorld.StatWorker" />
	public class HasMorphStat : StatWorker
	{
		/// <summary>
		/// Determines whether this stat is shown for the given request.
		/// </summary>
		/// <param name="req">The stat request.</param>
		/// <returns>
		///   <c>true</c> if this stat shoudl be shown for the given request; otherwise, <c>false</c>.
		/// </returns>
		public override bool ShouldShowFor(StatRequest req)
		{
			return GetThingDef(req)?.race?.Animal == true;
		}

		/// <summary>
		/// Determines whether this stat is disabled for the given thing.
		/// </summary>
		/// <param name="thing">The thing.</param>
		/// <returns>
		///   <c>true</c> if this stat is disabled for the given thing; otherwise, <c>false</c>.
		/// </returns>
		public override bool IsDisabledFor(Thing thing)
		{
			return thing?.def?.race?.Animal != true;
		}

		private const string NO_MORPH_LABEL = "PmMorphInfo_NoMorph";
		private const string MORPH_LABEL = "PmMorphInfo_MorphDisplay";
		private const string MORPH_TAG = "MORPH";

		/// <summary>
		/// Gets the stat draw entry label.
		/// </summary>
		/// <param name="stat">The stat.</param>
		/// <param name="value">The value.</param>
		/// <param name="numberSense">The number sense.</param>
		/// <param name="optionalReq">The optional req.</param>
		/// <param name="finalized">if set to <c>true</c> [finalized].</param>
		/// <returns></returns>
		public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq,
													 bool finalized = true)
		{
			var morph = GetThingDef(optionalReq)?.TryGetBestMorphOfAnimal();
			if (morph == null)
			{
				return NO_MORPH_LABEL.Translate();
			}

			return MORPH_LABEL.Translate(morph.LabelCap.Named(MORPH_TAG));
		}

		/// <summary>
		/// Gets the unfinalized stat explanation.
		/// </summary>
		/// <returns>The explanation unfinalized.</returns>
		/// <param name="req">Req.</param>
		/// <param name="numberSense">Number sense.</param>
		public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense) => "";

		/// <summary>
		/// Gets the explanation finalize part.
		/// </summary>
		/// <returns>The explanation finalize part.</returns>
		/// <param name="req">Req.</param>
		/// <param name="numberSense">Number sense.</param>
		/// <param name="finalVal">Final value.</param>
		public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal) => "";

		/// <summary>
		/// Gets the thing def from a request.
		/// </summary>
		/// <returns>The thing def.</returns>
		/// <param name="req">Req.</param>
		private ThingDef GetThingDef(StatRequest req)
		{
			return (req.Def as ThingDef) ?? req.Pawn?.def;
		}
	}
}