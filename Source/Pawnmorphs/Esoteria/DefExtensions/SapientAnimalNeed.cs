// SapientAnimalNeed.cs created by Iron Wolf for Pawnmorph on 11/16/2020 12:26 PM
// last updated 11/16/2020  12:26 PM

using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// def extension for marking a need for use in sapient animals 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class SapientAnimalNeed : DefModExtension
	{
		/// <summary>
		/// if the sapient animal must be a colonist 
		/// </summary>
		public bool mustBeColonist;
	}
}