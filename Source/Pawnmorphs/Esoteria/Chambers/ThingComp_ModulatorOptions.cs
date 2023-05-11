// ThingComp_ModulatorOptions.cs modified by Iron Wolf for Pawnmorph on 08/27/2019 8:53 AM
// last updated 08/27/2019  8:53 AM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Chambers
{
	/// <summary>
	/// comp representing the mutation chamber modulator options 
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	public class ThingComp_ModulatorOptions : ThingComp
	{
		private ThingCompProperties_ModulatorOptions Props => (ThingCompProperties_ModulatorOptions)props;



	}
	/// <summary>
	/// property for the mutagen chamber to get it's default set animal options 
	/// </summary>
	/// <seealso cref="Verse.CompProperties" />
	public class ThingCompProperties_ModulatorOptions : CompProperties
	{
		/// <summary>The default animals to be available without having to tag them</summary>
		public List<PawnKindDef> defaultAnimals = new List<PawnKindDef>();
		/// <summary>The merges that this modulator can create</summary>
		public List<PawnKindDef> merges = new List<PawnKindDef>();

		/// <summary>
		/// Initializes a new instance of the <see cref="ThingCompProperties_ModulatorOptions"/> class.
		/// </summary>
		public ThingCompProperties_ModulatorOptions()
		{
			compClass = typeof(ThingComp_ModulatorOptions);
		}
	}
}