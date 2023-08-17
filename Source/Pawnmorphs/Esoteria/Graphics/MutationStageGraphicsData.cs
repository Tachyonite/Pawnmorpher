// MutationGraphicsData.cs created by Iron Wolf for Pawnmorph on 08/15/2021 12:52 PM
// last updated 08/15/2021  12:52 PM

using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using AlienRace.ExtendedGraphics;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace Pawnmorph.GraphicSys
{
	/// <summary>
	/// simple class containing data about a specific set of mutations graphics 
	/// </summary>
	public class MutationStageGraphicsData : ExtendedHediffSeverityGraphic
	{
		/// <summary>
		/// The anchor identifier
		/// </summary>
		public string anchorID;

		[UsedImplicitly]
		public new void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			anchorID = xmlRoot.Name;
			Traverse traverse = Traverse.Create(this);
			foreach (XmlNode childNode in xmlRoot.ChildNodes)
			{
				Traverse field = traverse.Field(childNode.Name);

				if (field.FieldExists())
				{
					field.SetValue(field.GetValueType().IsGenericType ? DirectXmlToObject.GetObjectFromXmlMethod(field.GetValueType())(childNode, arg2: false) : ParseHelper.FromString(childNode.InnerXml.Trim(), field.GetValueType()));
				}
				else
				{
					path = childNode.Value;
				}
			}

		}
	}
}