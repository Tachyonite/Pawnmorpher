// Def_AspectThought.cs modified by Iron Wolf for Pawnmorph on 09/28/2019 7:39 AM
// last updated 09/28/2019  7:39 AM

using System.Collections.Generic;
using RimWorld;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// thought def that has an aspect attached 
	/// </summary>
	public class Def_AspectThought : ThoughtDef
	{
		/// <summary>
		/// The aspect to check for 
		/// </summary>
		public AspectDef aspect;

		/// <summary>
		/// Get all Configuration Errors with this instance
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors())
			{
				yield return configError;
			}

			if (aspect == null)
			{
				yield return "no aspect def assigned";
			}
		}
	}
}