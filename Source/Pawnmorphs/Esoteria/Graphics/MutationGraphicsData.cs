// MutationGraphicsData.cs created by Iron Wolf for Pawnmorph on 08/15/2021 12:52 PM
// last updated 08/15/2021  12:52 PM

using System.Xml;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace Pawnmorph.GraphicSys
{
	/// <summary>
	/// simple class containing data about a specific set of mutations graphics 
	/// </summary>
	public class MutationGraphicsData : ExtendedConditionGraphic
	{
		/// <summary>
		/// The anchor identifier
		/// </summary>
		public string anchorID;
		public string path;
		public MutationDef hediff;
		


		[UsedImplicitly]
		public virtual void LoadDataFromXmlCustom(XmlNode xmlRoot)
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