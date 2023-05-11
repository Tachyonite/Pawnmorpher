// ThoughtGroupDefExtension.cs modified by Iron Wolf for Pawnmorph on 12/02/2019 7:36 AM
// last updated 12/02/2019  7:36 AM

using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// def extension that represents a group a thoughts that should be tried under certain circumstances, the specifics are determined based on what the extension is attached to 
	/// </summary>
	/// if attached to a thoughtDef, when trying to add the thoughtDef, the other thoughts will be tried first before adding the given thought 
	/// <seealso cref="Verse.DefModExtension" />
	public class ThoughtGroupDefExtension : DefModExtension
	{
		/// <summary>
		/// list of thoughts that should be tried 
		/// </summary>
		[NotNull]
		public List<ThoughtDef> thoughts = new List<ThoughtDef>();


	}
}