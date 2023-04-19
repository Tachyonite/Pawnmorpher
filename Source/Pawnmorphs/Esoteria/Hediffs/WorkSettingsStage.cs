// WorkSettingsStage.cs created by Iron Wolf for Pawnmorph on 04/26/2020 9:07 AM
// last updated 04/26/2020  9:07 AM

using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	///     hediff stage that affects what work the pawn is capable of doing
	/// </summary>
	/// <seealso cref="Verse.HediffStage" />
	/// <seealso cref="Pawnmorph.IWorkModifier" />
	public class WorkSettingsStage : HediffStage, IWorkModifier
	{
		/// <summary>
		///     The allowed work tags
		/// </summary>
		public WorkTags allowedWork;

		/// <summary>
		/// The disallowed work tags
		/// </summary>
		public WorkTags disallowedWork;



		/// <summary>
		///     a filter for work type defs
		/// </summary>
		public Filter<WorkTypeDef> workFilter;

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkSettingsStage"/> class.
		/// </summary>
		public WorkSettingsStage()
		{
			allowedWork = (WorkTags)~0; //everything
		}

		/// <summary>
		///     Gets the allowed work tags.
		/// </summary>
		/// <value>
		///     The allowed work tags.
		/// </value>
		WorkTags IWorkModifier.AllowedWorkTags => (allowedWork & (~disallowedWork));

		/// <summary>
		///     Gets the work type filter.
		/// </summary>
		/// <value>
		///     The work type filter.
		/// </value>
		Filter<WorkTypeDef> IWorkModifier.WorkTypeFilter => workFilter;
	}
}