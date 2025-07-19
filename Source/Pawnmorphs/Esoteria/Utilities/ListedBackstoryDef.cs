using System;
using System.Collections.Generic;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// A backstory def that can explicitly allow a list of multiple work types 
	/// </summary>
	public class ListedBackstoryDef : AlienRace.AlienBackstoryDef
	{

		/// <inheritdoc />
		public override void ResolveReferences()
		{
			base.ResolveReferences();

			workDisables ^= WorkTags.AllWork;
		}

	}
}
