// SkillBaseGiver.cs modified by Iron Wolf for Pawnmorph on 12/16/2019 5:17 PM
// last updated 12/16/2019  5:17 PM

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph.Aspects
{
	/// <summary>
	///  aspect giver that only gives an aspect when a skill is above a certain threshold 
	/// </summary>
	/// <seealso cref="Pawnmorph.AspectGiver" />
	public class SkillBasedGiver : AspectGiver
	{
		/// <summary>
		/// Gets the aspects available to be given to pawns.
		/// </summary>
		/// <value>
		/// The available aspects.
		/// </value>
		public override IEnumerable<AspectDef> AvailableAspects
		{
			get { yield return aspect; }
		}


		/// <summary>
		/// Tries to give aspects to the given pawn 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="outList">if not null, all given aspects will be placed into the list</param>
		/// <returns>if any aspects were successfully given to the pawn</returns>
		public override bool TryGiveAspects(Pawn pawn, List<Aspect> outList = null)
		{
			var skill = pawn.skills?.GetSkill(skillDef);
			if (skill == null) return false;
			if (skill.levelInt > skillThreshold && Rand.Value < chance)
				return ApplyAspect(pawn, aspect, stageIndex, outList);
			return false;
		}

		/// <summary>
		/// The aspect def to give 
		/// </summary>
		public AspectDef aspect;
		/// <summary>
		/// The chance to give the aspect when above the given skill threshold
		/// should be between 0-1 
		/// </summary>
		public float chance = 1;

		/// <summary>
		/// The stage index
		/// </summary>
		public int stageIndex = 0;

		/// <summary>
		/// The skill definition
		/// </summary>
		public SkillDef skillDef;

		/// <summary>
		/// The skill threshold, the pawn's skill must be above this to give the aspect 
		/// </summary>
		public int skillThreshold;

		/// <summary>
		/// get all configuration errors with this giver 
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			if (aspect == null)
			{
				yield return $"aspect def is null on {nameof(SkillBasedGiver)}";
			}
		}
	}
}