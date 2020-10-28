// MutaniteCentrifuge.cs created by Iron Wolf for Pawnmorph on 03/25/2020 6:14 AM
// last updated 03/25/2020  6:14 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph.Buildings
{
	/// <summary>
	///     building class for the mutagen tank
	/// </summary>
	/// <seealso cref="Verse.Building" />
	[StaticConstructorOnStartup]
	public class MutagenTank : Building
    {
		private static readonly Vector2 BarSize = new Vector2(0.55f, 0.4f);

		private static readonly Material BatteryBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.0f, 1.0f, 0.0f));

		private static readonly Material BatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

        /// <summary>
        /// Draws this instance.
        /// </summary>
        public override void Draw()
        {
            const float upFactor = 0.05f; 
			base.Draw();
			CompRefuelable comp = GetComp<CompRefuelable>();
            GenDraw.FillableBarRequest r = new GenDraw.FillableBarRequest
            {
                center = DrawPos + Vector3.up * 0.1f + new Vector3(0.005f, 0, upFactor),
                size = BarSize,
                fillPercent = comp.FuelPercentOfMax,
                filledMat = BatteryBarFilledMat,
                unfilledMat = BatteryBarUnfilledMat,
                margin = 0.05f
            };
            Rot4 rotation = base.Rotation;
			rotation.Rotate(RotationDirection.Clockwise);
			r.rotation = rotation;
			GenDraw.DrawFillableBar(r);
		}
	}
}