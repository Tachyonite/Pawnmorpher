// IWorkModifier.cs created by Iron Wolf for Pawnmorph on 04/26/2020 8:40 AM
// last updated 04/26/2020  8:40 AM

using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// interface for something that modifies the kind of work a pawn can do 
	/// </summary>
	public interface IWorkModifier
	{
		/// <summary>
		/// Gets the allowed work tags.
		/// </summary>
		/// <value>
		/// The allowed work tags.
		/// </value>
		WorkTags AllowedWorkTags { get; }


		/// <summary>
		/// Gets the work type filter.
		/// </summary>
		/// <value>
		/// The work type filter.
		/// </value>
		[CanBeNull]
		Filter<WorkTypeDef> WorkTypeFilter { get; }
	}
}