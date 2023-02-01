﻿// MutaniteCentrifuge.cs created by Iron Wolf for Pawnmorph on 03/25/2020 6:14 AM
// last updated 03/25/2020  6:14 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;

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
            CompResourceStorage comp = GetComp<CompResourceStorage>();
            GenDraw.FillableBarRequest r = new GenDraw.FillableBarRequest
            {
                center = DrawPos + Vector3.up * 0.1f + new Vector3(0.05f, 0, upFactor),
                size = BarSize,
                fillPercent = comp.AmountStoredPct,
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