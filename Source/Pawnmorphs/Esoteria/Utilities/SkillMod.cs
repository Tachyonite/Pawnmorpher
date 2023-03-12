// SkillMod.cs modified by Iron Wolf for Pawnmorph on 09/29/2019 7:32 AM
// last updated 09/29/2019  7:32 AM

using RimWorld;
using UnityEngine;

namespace Pawnmorph.Utilities
{
	/// <summary>
	///     represents a modification to a pawn's skills
	/// </summary>
	public class SkillMod
	{
		/// <summary>The skill definition to affect</summary>
		public SkillDef skillDef;

		/// <summary>The added xp</summary>
		public float addedXp;

		/// <summary>The passion offset </summary>
		public int passionOffset;

		/// <summary>The forced passion</summary>
		public Passion? forcedPassion;

		/// <summary> The new passion of the skill with this mod. </summary>
		public Passion GetNewPassion(Passion oldPassion)
		{
			if (forcedPassion != null) return forcedPassion.Value;

			int nP = (int)oldPassion + passionOffset;
			nP = Mathf.Clamp(nP, 0, 2);
			return (Passion)nP;
		}
	}
}