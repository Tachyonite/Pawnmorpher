// AspectGiverExtension.cs modified by Iron Wolf for Pawnmorph on 12/16/2019 4:41 PM
// last updated 12/16/2019  4:41 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// def extension for adding aspect givers to defs 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class AspectGiverExtension : DefModExtension
	{
		/// <summary>
		/// The aspect givers
		/// </summary>
		public List<AspectGiver> aspectGivers = new List<AspectGiver>();

		/// <summary>
		/// Tries to apply aspects to the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <exception cref="ArgumentNullException">pawn</exception>
		public void TryApply([NotNull] Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			foreach (AspectGiver aspectGiver in aspectGivers.MakeSafe())
			{
				aspectGiver.TryGiveAspects(pawn);
			}
		}

	}
}