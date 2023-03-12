// MutagenicDamageExtension.cs modified by Iron Wolf for Pawnmorph on 11/10/2019 1:56 PM
// last updated 11/10/2019  1:56 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Damage
{
	/// <summary>
	/// def extension to give damage types mutagenic effects 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class MutagenicDamageExtension : DefModExtension
	{
		/// <summary>
		/// The mutagenic buildup hediff def 
		/// </summary>
		public HediffDef mutagenicBuildup;

		/// <summary>
		/// The hediff severity per damage point
		/// </summary>
		public float severityPerDamage = 0.04f;

		/// <summary>the amount by which to reduce the raw damage to the pawn, mutagenic</summary>
		public float reduceValue = 1 / 3f;

		/// <summary>
		/// The mutagen to use for the mutagenic damage
		/// if null the default is used 
		/// </summary>
		[CanBeNull]
		public MutagenDef mutagen;

		/// <summary>
		/// gets all Configuration errors with this instance.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors())
			{
				yield return configError;
			}

			if (mutagenicBuildup == null) yield return "no mutagenic buildup hediff set";
		}
	}
}