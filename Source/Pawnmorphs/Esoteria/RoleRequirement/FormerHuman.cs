// FormerHuman.cs created by Iron Wolf for Pawnmorph on 07/23/2021 3:58 PM
// last updated 07/23/2021  3:58 PM

using RimWorld;
using Verse;

namespace Pawnmorph.RoleRequirement
{
	/// <summary>
	/// role requirement for ensuring a pawn is always a former human 
	/// </summary>
	/// <seealso cref="Pawnmorph.RoleRequirement.BaseRoleRequirement" />
	public class FormerHuman : BaseRoleRequirement
	{
		/// <summary>
		/// The minimum sapience level
		/// </summary>
		public SapienceLevel minSapienceLevel = SapienceLevel.PermanentlyFeral;

		/// <summary>
		///     determine if the given pawn meets the conditions of this requirement for the given role.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="role">The role.</param>
		/// <returns></returns>
		protected override bool Met_Internal(Pawn p, Precept_Role role)
		{
			if (!p.IsFormerHuman()) return false;
			var sapience = p.GetQuantizedSapienceLevel();
			return sapience < minSapienceLevel; //sapience levels are in inverse order. permanently feral is the max value 
		}
	}
}