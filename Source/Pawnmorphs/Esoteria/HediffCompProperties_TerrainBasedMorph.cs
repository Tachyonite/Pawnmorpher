using Verse;

namespace Pawnmorph
{
	/// <summary>Properties for the terrain based morph hediff comp <see cref="HediffComp_TerrainBasedMorph"/></summary>
	/// <seealso cref="Verse.HediffCompProperties" />
	public class HediffCompProperties_TerrainBasedMorph : HediffCompProperties
	{
		/// <summary>The hediffDef to add when over the specified terrain </summary>
		public HediffDef hediffDef = null;
		/// <summary>The terrain that triggers adding the given hediff</summary>
		public TerrainDef terrain = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="HediffCompProperties_TerrainBasedMorph"/> class.
		/// </summary>
		public HediffCompProperties_TerrainBasedMorph()
		{
			compClass = typeof(HediffComp_TerrainBasedMorph);
		}
	}
}
