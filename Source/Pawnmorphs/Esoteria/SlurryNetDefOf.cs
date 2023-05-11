using PipeSystem;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph
{
	/// <summary>
	/// Defs related to pawnmorpher's use of VFE slurry net
	/// </summary>
	[DefOf]
	public static class SlurryNetDefOf
	{
		/// <summary>
		/// The pawnmorpher slurry net def.
		/// </summary>
		public static PipeNetDef PM_SlurryNet;

		static SlurryNetDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(SlurryNetDefOf));
		}
	}
}
