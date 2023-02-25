// TreeUtilities.cs created by Iron Wolf for Pawnmorph on 12/30/2019 10:56 AM
// last updated 12/30/2019  10:56 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Utilities
{
	/// <summary>
	///     static container for various tree related utilities
	/// </summary>
	public static class TreeUtilities
	{
		/// <summary>
		///     delegate for getting the children of a root
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="node">The root.</param>
		/// <returns></returns>
		[NotNull]
		public delegate IEnumerable<T> GetChildrenAction<T>([NotNull] T node);

		/// <summary>
		///     traverse a tree using the postorder traversal
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="root">The root.</param>
		/// <param name="getChildren">The get children.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		///     root
		///     or
		///     getChildren
		/// </exception>
		public static List<T> Postorder<T>([NotNull] T root, [NotNull] GetChildrenAction<T> getChildren)
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (getChildren == null) throw new ArgumentNullException(nameof(getChildren));

			var outList = new List<T>();
			PostorderWorker(root, getChildren, outList);
			return outList;
		}

		/// <summary>traverses the tree in preorder</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="root">The root.</param>
		/// <param name="getChildrenAction">The get children action.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		///     root
		///     or
		///     getChildrenAction
		/// </exception>
		[NotNull]
		public static IEnumerable<T> Preorder<T>([NotNull] T root, [NotNull] GetChildrenAction<T> getChildrenAction)
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (getChildrenAction == null) throw new ArgumentNullException(nameof(getChildrenAction));
			var stack = new Stack<T>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				T node = stack.Pop();
				yield return node;
				foreach (T child in getChildrenAction(node).MakeSafe()) stack.Push(child);
			}
		}


		/// <summary>
		///     prints a pretty tree
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="root">The root.</param>
		/// <param name="getChildren">The get children.</param>
		/// <param name="toStringFunc">To string function.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		///     root
		///     or
		///     getChildren
		/// </exception>
		public static string PrettyPrintTree<T>([NotNull] T root, [NotNull] GetChildrenAction<T> getChildren,
												Func<T, string> toStringFunc = null)
		{
			if (root == null) throw new ArgumentNullException(nameof(root));
			if (getChildren == null) throw new ArgumentNullException(nameof(getChildren));
			toStringFunc = toStringFunc ?? (f => f.ToString());
			var builder = new StringBuilder();
			var indent = "";

			PrettyPrintTreeWorker(root, getChildren, toStringFunc, builder, true, indent);
			return builder.ToString();
		}

		/// <summary>add body part defs in to the given list in order of a 'randomized spread traversal' of the given body def</summary>
		/// <param name="bodyDef">The body definition.</param>
		/// <param name="outList">The out list.</param>
		/// <exception cref="ArgumentNullException">
		///     bodyDef
		///     or
		///     outList
		/// </exception>
		public static void RandomizedSpreadOrder([NotNull] this BodyDef bodyDef, [NotNull] List<BodyPartRecord> outList)
		{
			if (bodyDef == null) throw new ArgumentNullException(nameof(bodyDef));
			if (outList == null) throw new ArgumentNullException(nameof(outList));

			BodyPartRecord startLeaf = bodyDef.corePart;
			while (startLeaf.parts.Count > 0) //start traversal on a random leaf 
				startLeaf = startLeaf.parts.RandElement();

			BodyPartRecord curNode = startLeaf;
			BodyPartRecord lastNode = null;
			while (curNode != null)
			{
				outList.Add(curNode);
				foreach (BodyPartRecord child in RandomizedChildren(curNode)) //traverse each child in preorder 
				{
					if (child == lastNode) //except the child that was already checked 
						continue;

					foreach (BodyPartRecord bodyPartRecord in Preorder(child, RandomizedChildren))
						outList.Add(bodyPartRecord);
				}

				lastNode = curNode; //go up one level in the tree 
				curNode = curNode.parent;
			}
		}

		private static void PostorderWorker<T>([NotNull] T node, [NotNull] GetChildrenAction<T> getChildren,
											   [NotNull] List<T> outList)
		{
			foreach (T child in getChildren(node).MakeSafe()) PostorderWorker(child, getChildren, outList);

			outList.Add(node);
		}

		private static void PrettyPrintTreeWorker<T>([NotNull] T node, [NotNull] GetChildrenAction<T> getChildren,
													 [NotNull] Func<T, string> toStringFunc, StringBuilder builder, bool last,
													 string indent)
		{
			builder.Append(indent);
			if (last)
			{
				builder.Append("\\-");
				indent += "  ";
			}
			else
			{
				builder.Append("|-");
				indent += "| ";
			}

			builder.AppendLine(toStringFunc(node));

			List<T> lst = getChildren(node).MakeSafe().ToList();
			for (var i = 0; i < lst.Count; i++)
			{
				T child = lst[i];
				PrettyPrintTreeWorker(child, getChildren, toStringFunc, builder, i == lst.Count - 1, indent);
			}
		}

		[NotNull]
		private static List<BodyPartRecord> RandomizedChildren(BodyPartRecord record)
		{
			List<BodyPartRecord> children = record.parts.MakeSafe().ToList();
			children.Shuffle();
			return children;
		}
	}
}