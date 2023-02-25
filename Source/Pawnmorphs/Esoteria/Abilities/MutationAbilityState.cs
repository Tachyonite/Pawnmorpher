namespace Pawnmorph.Abilities
{
	/// <summary>
	/// Different mutation ability states.
	/// </summary>
	public enum MutationAbilityState
	{
		/// <summary>
		/// Ability is not currently doing anything.
		/// </summary>
		None,

		/// <summary>
		/// Ability is currently active.
		/// </summary>
		Active,

		/// <summary>
		/// Ability is currently being cast.
		/// </summary>
		Casting,

		/// <summary>
		/// Ability is currently cooling down.
		/// </summary>
		Cooldown,
	}
}
