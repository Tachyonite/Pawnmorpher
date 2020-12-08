using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace Pawnmorph.ThingComps
{
    class DrawStoredPawnComp : ThingComp
    {
        private DrawStoredPawnProperties Props => (DrawStoredPawnProperties)props;

        private Vector3 Offset => Props.offset;

        /// <summary>Called after the parent's graphic is drawn.</summary>
        public override void PostDraw()
        {
            Building_Casket chamber = (Building_Casket)parent;
            if (chamber.HasAnyContents)
            {
                foreach(Pawn pawn in chamber.GetDirectlyHeldThings().OfType<Pawn>())
                {
                    if (pawn != null)
                    {
                        pawn.Rotation = Rot4.South;
                        pawn.DrawAt(GenThing.TrueCenter(parent.Position, Rot4.South, parent.def.size, Props.Altitude) + Offset);
                    }
                }
            }
        }
    }
}
