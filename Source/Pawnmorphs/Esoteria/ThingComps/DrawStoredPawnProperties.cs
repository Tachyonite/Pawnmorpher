using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace Pawnmorph.ThingComps
{
    class DrawStoredPawnProperties : CompProperties
    {
        /// <summary>Offset to draw pawn at.</summary>
        public Vector3 offset;

        /// <summary>The altitude layer to draw the pawn at.</summary>
        public AltitudeLayer layer;

        public DrawStoredPawnProperties()
        {
            compClass = typeof(DrawStoredPawnComp);
        }

        /// <summary>Gets the layer to draw the pawn at as a float.</summary>
        /// <value>The altitude as a float.</value>
        public float Altitude => layer.AltitudeFor();

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
