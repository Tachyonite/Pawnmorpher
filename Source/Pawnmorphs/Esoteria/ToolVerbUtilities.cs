// ToolUtilities.cs created by Iron Wolf for Pawnmorph on 08/25/2021 4:46 PM
// last updated 08/25/2021  4:46 PM

using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// static class containing tool and verb related utilities 
	/// </summary>
	public static class ToolVerbUtilities
	{
		/// <summary>
		/// Determines whether this tool is a natural weapon, like a claw 
		/// </summary>
		/// <param name="tool">The tool.</param>
		/// <returns>
		///   <c>true</c> if this tool is a natural weapon, like a claw ; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNaturalWeapon([NotNull] this Tool tool)
		{
			if (tool.hediff != null) return tool.hediff is MutationDef; //cache this somehow if this is a performance issue 

			return tool.linkedBodyPartsGroup != null;

		}
	}
}