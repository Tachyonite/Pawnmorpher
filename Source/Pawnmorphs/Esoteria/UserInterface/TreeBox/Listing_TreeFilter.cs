using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface.TreeBox
{
	internal class Listing_TreeFilter : Listing_Tree
	{
		Rect _rect = new Rect();

		internal bool Node(TreeNode_FilterBox node, int indentLevel, float editOffset, bool recursiveDraw = true)
		{
			if (node.Visible == false || node.Enabled == false)
				return false;

			bool toggled = OpenCloseWidget(node, indentLevel, -1);
			LabelLeft(node.Label, node.Tooltip, indentLevel, editOffset);

			_rect.x = LabelWidth - editOffset;
			_rect.y = curY;
			_rect.width = EditAreaWidth + editOffset;
			_rect.height = lineHeight;

			if (node.Callback != null)
				node.Draw(in _rect);

			EndLine();
			if (recursiveDraw)
			{
				if (IsOpen(node, -1))
				{
					int count = node.children.Count;
					for (int i = 0; i < count; i++)
					{
						toggled |= Node((TreeNode_FilterBox)node.children[i], indentLevel + 1, editOffset);
					}
				}
			}

			return toggled;
		}
	}
}
