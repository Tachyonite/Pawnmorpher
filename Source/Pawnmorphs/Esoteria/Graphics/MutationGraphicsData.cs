// MutationGraphicsData.cs created by Iron Wolf for Pawnmorph on 08/15/2021 12:52 PM
// last updated 08/15/2021  12:52 PM

using System.Collections.Generic;
using System.Xml;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace Pawnmorph.GraphicSys
{
	/// <summary>
	/// simple class containing data about a specific set of mutations graphics 
	/// </summary>
	public class MutationGraphicsData
	{
		/// <summary>
		/// The anchor identifier
		/// </summary>
		public string anchorID;

		/// <summary>
		/// Base texture path
		/// </summary>
		public string path;

		[XmlInheritanceAllowDuplicateNodes]
		public List<ExtendedConditionGraphic> extendedGraphics;


		[UsedImplicitly]
		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			extendedGraphics = new List<ExtendedConditionGraphic>();
			anchorID = xmlRoot.Name;
			if (xmlRoot.ChildNodes.Count == 0)
			{
				path = xmlRoot.Value;
				return;
			}

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

		/// <summary>
		/// Gets the first valid texture path in the graphic tree.
		/// </summary>
		/// <returns></returns>
		public string GetPath()
		{
			if (string.IsNullOrWhiteSpace(path) == false)
				return path;

			foreach (var item in extendedGraphics)
			{
				string path = item.GetPath();
				if (string.IsNullOrWhiteSpace(path) == false)
					return path;
			}

			return null;
		}
	}
}