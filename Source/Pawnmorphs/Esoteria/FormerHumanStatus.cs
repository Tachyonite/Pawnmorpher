// FormerHumanStatus.cs modified by Iron Wolf for Pawnmorph on 11/29/2019 7:53 AM
// last updated 11/29/2019  7:53 AM

namespace Pawnmorph
{
	/// <summary>
	/// enum that represents the 'quantized sapience level' of a former human 
	/// </summary>
	public enum SapienceLevel //TODO sapient levels are in reverse order, refactor this so they are in the expected order of Sapient > Feral. Note, lots of places uses Sapient < Feral so there will be alot of changes 
	{
		/// <summary>The former human if fully aware</summary>
		Sapient,
		/// <summary> The pawn loses the ability to speak and perform work.</summary>
		MostlySapient,
		/// <summary>The conflicted</summary>
		Conflicted,
		/// <summary>The pawn can no longer sleep in beds, does not care about being naked and can be trained a bit easier. Has "hunting" mental breaks.</summary>
		MostlyFeral,
		/// <summary>The pawn cannot hold weapons in its mouth, is fine with eating kibble off the floor, is fine with sleeping outside, training is much easier</summary>
		Feral,
		/// <summary>the pawn is permanently feral</summary>
		PermanentlyFeral
	}

}