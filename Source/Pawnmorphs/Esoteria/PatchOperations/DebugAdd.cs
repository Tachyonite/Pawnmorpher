// DebugAdd.cs created by Iron Wolf for Pawnmorph on 10/24/2020 8:35 AM
// last updated 10/24/2020  8:35 AM

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.PatchOperations
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Verse.PatchOperationPathed" />
    public class DebugAdd : PatchOperationPathed
    {
        private enum Order
        {
            Append,
            Prepend
        }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        private XmlContainer value;
        
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        private Order order;

        /// <summary>
        /// Applies the worker.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNode node = value.node;
            bool result = false;

            List<XmlNode> foundNodes = xml.SelectNodes(xpath).OfType<XmlNode>().ToList();

            if (foundNodes.Count == 0)
            {
                Log.Error($"unable to find any nodes matching xpath \n\"{xpath}\"!");
                return false; 
            }

            foreach (var xmlNode in foundNodes)
            {
                result = true;
                if (order == Order.Append)
                {
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(childNode, deep: true));
                    }
                }
                else if (order == Order.Prepend)
                {
                    for (int num = node.ChildNodes.Count - 1; num >= 0; num--)
                    {
                        xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[num], deep: true));
                    }
                }
            }
            return result;
        }
    }
}