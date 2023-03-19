// IInitializable.cs modified by Iron Wolf for Pawnmorph on 01/13/2020 5:46 PM
// last updated 01/13/2020  5:46 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// interface for a hediff stage or hediff giver that needs to be initialized 
	/// </summary>
	[Obsolete("will be obsolete with teh new hediff stages ")]
	public interface IInitializable
	{
		/// <summary>
		/// Gets all Configuration errors in this instance.
		/// </summary>
		/// <returns></returns>
		[NotNull] IEnumerable<string> ConfigErrors();
	}

	/// <summary>
	/// interface for a hediff stage that needs to preform some configuration on startup 
	/// </summary>
	public interface IInitializableStage
	{
		/// <summary>
		/// gets all configuration errors in this stage .
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		[NotNull]
		IEnumerable<string> ConfigErrors([NotNull] HediffDef parentDef);

		/// <summary>
		/// Resolves all references in this instance.
		/// </summary>
		/// <param name="parent">The parent.</param>
		void ResolveReferences([NotNull] HediffDef parent);
	}
}