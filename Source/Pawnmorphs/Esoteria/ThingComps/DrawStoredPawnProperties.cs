using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.ThingComps
{

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Verse.CompProperties" />
	public class DrawStoredPawnProperties : CompProperties
	{
		/// <summary>Offset to draw pawn at.</summary>
		public Vector3 offset;

		/// <summary>The altitude layer to draw the pawn at.</summary>
		public AltitudeLayer layer;

		/// <summary>
		/// Initializes a new instance of the <see cref="DrawStoredPawnProperties"/> class.
		/// </summary>
		public DrawStoredPawnProperties()
		{
			compClass = typeof(DrawStoredPawnComp);
		}

		/// <summary>Gets the layer to draw the pawn at as a float.</summary>
		/// <value>The altitude as a float.</value>
		public float Altitude => layer.AltitudeFor();

		/// <summary>
		/// gathers all configuration errors in this instance.
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (!parentDef.thingClass.IsSubclassOf(typeof(Building_Casket)) && parentDef.thingClass != typeof(Building_Casket))
			{
				yield return $"{parentDef.defName}'s thingClass is not a subclass of {nameof(Building_Casket)}, but has the DrawStoredPawnProperties comp.";
			}
			base.ConfigErrors(parentDef);
		}
	}
}
