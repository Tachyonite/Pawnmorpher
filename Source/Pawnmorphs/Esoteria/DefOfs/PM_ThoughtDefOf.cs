using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

#pragma warning disable 1591

namespace Pawnmorph.DefOfs
{
	[DefOf]
	public static class PM_ThoughtDefOf
	{
		static PM_ThoughtDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PM_ThoughtDefOf));
		}

		public static ThoughtDef AteHumanlikeMeatDirect;
		public static ThoughtDef AteHumanlikeMeatAsIngredient;
		public static ThoughtDef ButcheredHumanlikeCorpse;
		public static ThoughtDef KnowButcheredHumanlikeCorpse;
	}
}
