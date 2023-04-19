// MorphTransformationType.cs modified by Iron Wolf for Pawnmorph on 08/25/2019 11:34 AM
// last updated 08/25/2019  11:34 AM

using System;

namespace Pawnmorph
{
	/// <summary>
	/// enum for telling the different kinds of morph transformation hediffs 
	/// </summary>
	[Flags]
	public enum MorphTransformationTypes
	{
		/// <summary>a full transformation</summary>
		Full = 1 << 0,
		/// <summary>a partial transformation</summary>
		Partial = 1 << 1
	}
}