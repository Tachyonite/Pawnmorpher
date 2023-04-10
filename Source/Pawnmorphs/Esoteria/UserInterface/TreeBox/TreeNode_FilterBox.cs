using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface.TreeBox
{
	internal class TreeNode_FilterBox : TreeNode
	{
		/// <summary>
		/// Delegate action type that takes an in.
		/// </summary>
		public delegate void ActionIn<T>(in T value);

		private bool _canOpen = false;

		/// <summary>
		/// Gets or sets whether this node is has label and value on separate lines.
		/// </summary>
		public bool SplitRow { get; set; }

		/// <summary>
		/// Gets the node's label/caption.
		/// </summary>
		public string Label { get; }

		/// <summary>
		/// Gets or sets the tooltip.
		/// </summary>
		/// <value>
		/// The tooltip.
		/// </value>
		public string Tooltip { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="TreeNode_FilterBox"/> is visible in a tree.
		/// </summary>
		public bool Visible { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="TreeNode_FilterBox"/> is enabled.
		/// </summary>
		public bool Enabled { get; set; }

		public ActionIn<Rect> Callback { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TreeNode_FilterBox"/> node to use with the <see cref="FilterTreeBox"/>.
		/// </summary>
		/// <param name="label">The label of the node.</param>
		/// <param name="tooltip">Tooltip for the node.</param>
		/// <param name="callback">Draw callback used to add widgets to the node.</param>
		public TreeNode_FilterBox(string label, string tooltip = null, ActionIn<Rect> callback = null)
			: base()
		{
			Enabled = true;
			Label = label;
			Tooltip = tooltip;
			Callback = callback;
			children = new List<TreeNode>();
		}

		/// <summary>
		/// Triggers draw callback if any.
		/// </summary>
		/// <param name="rect">The rect.</param>
		public void Draw(in Rect rect)
		{
			if (Callback == null)
				return;

			Callback(in rect);
		}

		/// <summary>
		/// Creates a new child node under this node.
		/// </summary>
		/// <param name="labelKey">Translation key to use for label.</param>
		/// <param name="tooltipKey">Optional translation key to use for tooltip.</param>
		/// <param name="callback">Optional draw callback.</param>
		/// <param name="splitRow">Optional draw callback.</param>
		/// <returns></returns>
		public TreeNode_FilterBox AddChild(string labelKey, string tooltipKey = null, ActionIn<Rect> callback = null, bool splitRow = false)
		{
			TreeNode_FilterBox node = new TreeNode_FilterBox(labelKey.Translate(), tooltipKey?.Translate(), callback);
			node.SplitRow = splitRow;
			node.parentNode = this;
			node.nestDepth = nestDepth + 1;
			children.Add(node);
			_canOpen = true;
			return node;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="TreeNode_FilterBox"/> can be expanded in a tree.
		/// </summary>
		/// <value>
		///   <c>true</c> if node has children.
		/// </value>
		public override bool Openable => _canOpen;

		/// <summary>
		/// Sets the visibility.
		/// </summary>
		/// <param name="visible">Whether or not to draw the node.</param>
		/// <param name="recursive">If true assigns recursively to children..</param>
		/// <returns>Number of nodes affected.</returns>
		public void SetVisibility(bool visible, bool recursive)
		{
			Visible = visible;
			if (recursive)
			{
				for (int i = children.Count - 1; i >= 0; i--)
				{
					((TreeNode_FilterBox)children[i]).SetVisibility(visible, recursive);
				}
			}
		}

		/// <summary>
		/// Gets all visible nodes and child nodes.
		/// </summary>
		/// <param name="nodes">Output collection to return nodes.</param>
		public void GetVisibleNodes(List<TreeNode_FilterBox> nodes)
		{
			if (Visible && Enabled)
			{
				nodes.Add(this);

				if (IsOpen(1))
				{
					int count = children.Count;
					for (int i = 0; i < count; i++)
					{
						((TreeNode_FilterBox)children[i]).GetVisibleNodes(nodes);
					}
				}
			}
		}
	}
}
