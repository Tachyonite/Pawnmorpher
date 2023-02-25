// ICustomVerb.cs created by Iron Wolf for Pawnmorph on 08/13/2020 5:06 PM
// last updated 08/13/2020  5:06 PM

using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// interface for verbs with customizable commands 
	/// </summary>
	public interface ICustomVerb
	{
		/// <summary>
		/// Gets the label.
		/// </summary>
		/// <param name="ownerThing">The owner thing.</param>
		/// <returns></returns>
		string GetLabel([NotNull] Thing ownerThing);

		/// <summary>
		/// Gets the description for this verb 
		/// </summary>
		/// <param name="ownerThing">The owner thing.</param>
		/// <returns></returns>
		string GetDescription([NotNull] Thing ownerThing);

		/// <summary>
		/// Gets the UI icon for this verb 
		/// </summary>
		/// <param name="ownerThing">The owner thing.</param>
		/// <returns></returns>
		[CanBeNull]
		Texture2D GetUIIcon([NotNull] Thing ownerThing);


	}
}