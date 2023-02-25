// DrawSecondChamberProperties.cs created by Iron Wolf for Pawnmorph on 07/26/2020 11:55 AM
// last updated 07/26/2020  11:55 AM

using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// comp properties for <see cref="DrawSecondChamberComp"/>
	/// </summary>
	/// <seealso cref="Verse.CompProperties" />
	public class DrawSecondChamberProperties : CompProperties
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DrawSecondChamberProperties"/> class.
		/// </summary>
		public DrawSecondChamberProperties()
		{
			compClass = typeof(DrawSecondChamberComp);
		}

		/// <summary>
		/// The graphic data to be drawn 
		/// </summary>
		public GraphicData graphicData;

		/// <summary>
		/// The offset
		/// </summary>
		public Vector3 offset;


		/// <summary>
		/// The altitude layer to draw the graphic on 
		/// </summary>
		public AltitudeLayer altitudeLayer;


		/// <summary>
		/// Gets the altitude to draw the graphic on 
		/// </summary>
		/// <value>
		/// The altitude.
		/// </value>
		public float Altitude => altitudeLayer.AltitudeFor();
	}


	/// <summary>
	/// comp that draws the second part of the mutagen chamber 
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	public class DrawSecondChamberComp : ThingComp
	{
		private DrawSecondChamberProperties Props => (DrawSecondChamberProperties)props;


		private Graphic _graphic;

		private Vector3 Offset => Props.offset;


		[NotNull]
		Graphic Graphic
		{
			get
			{
				if (_graphic == null)
				{
					if (Props.graphicData == null)
					{
						var msg = $"unable to create graphic for {parent.ThingID} as the comp props has no graphic data";
						Log.ErrorOnce(msg, msg.GetHashCode());
						return BaseContent.BadGraphic;
					}

					_graphic = Props.graphicData.GraphicColoredFor(parent);

				}

				return _graphic;
			}
		}

		/// <summary>
		///  called after the parent's graphic is drawn
		/// </summary>
		public override void PostDraw()
		{

			Graphic.Draw(GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.size, Props.Altitude) + Offset,
						 parent.Rotation, parent);
		}

	}
}