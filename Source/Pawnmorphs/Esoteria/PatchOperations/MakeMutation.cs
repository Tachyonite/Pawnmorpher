using Pawnmorph.Hediffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace Pawnmorph.PatchOperations
{
	public class MakeMutation : PatchOperationPathed
	{
		private XmlContainer value;

		private static readonly string NodeName = typeof(MutationDef).FullName;
		private string parentName = "PawnmorphPart";
		protected override bool ApplyWorker(XmlDocument xml)
		{
			if (value == null)
			{
				Log.Error($"unable to preform patch {nameof(MakeMutation)}! no value set");
				return false;
			}

			var node = value.node;
			bool result = false;
			foreach (var cNode in xml.SelectNodes(xpath).OfType<XmlElement>())
			{
				result = true;
				var newNode = cNode.OwnerDocument.CreateElement(NodeName);
				newNode.InnerXml = cNode.InnerXml;
				foreach (XmlAttribute attr in cNode.Attributes)
				{
					newNode.SetAttribute(attr.LocalName, attr.Value);
				}


				newNode.SetAttribute("ParentName", parentName);

				cNode.ParentNode.InsertBefore(newNode, cNode);
				cNode.ParentNode.RemoveChild(cNode);

				foreach (var newCNode in node.ChildNodes.OfType<XmlNode>())
				{
					newNode.AppendChild(newNode.OwnerDocument.ImportNode(newCNode, true));
				}

			}

			return result;
		}
	}
}
