// IPMThingComp.cs created by Iron Wolf for Pawnmorph on 05/17/2020 8:35 AM
// last updated 05/17/2020  8:35 AM

namespace Pawnmorph
{
	/// <summary>
	/// interface for thing comps so they can receive messages when they are being added/remove dynamically by hybrid race changes  
	/// </summary>
	public interface IPMThingComp
	{
		/// <summary>
		/// called just before a comp is about to be removed from the pawn 
		/// </summary>
		void PreRemove();

		/// <summary>
		/// called just after a comp is removed from the pawn
		/// </summary>
		void PostRemove();

		/// <summary>
		/// Initializes this instance after being added during a race change
		/// </summary>
		void Init();
	}


}