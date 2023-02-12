using System.Collections.Generic;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// A backstory def that can explicitly allow a list of multiple work types 
	/// </summary>
	public class ListedBackstoryDef : AlienRace.AlienBackstoryDef
	{

		/// <summary>
		/// The list of allowed WorkTags
		/// </summary>
		public List<WorkTags> workAllowsList = new List<WorkTags>();


		/// <inheritdoc />
		public override void ResolveReferences()
		{
			if (workAllowsList.Count > 0)
			{
				WorkTags allowed = WorkTags.None;

				foreach (WorkTags workAllowEntry in workAllowsList)
					allowed |= workAllowEntry;

				workAllows = allowed;
			}

			base.ResolveReferences();

			workDisables ^= WorkTags.AllWork;
		}

	}
}
