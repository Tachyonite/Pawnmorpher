// TreeUtilities.cs created by Iron Wolf for Pawnmorph on 12/30/2019 10:56 AM
// last updated 12/30/2019  10:56 AM

using System;
using System.Collections.Generic;
using System.Linq;
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

        [NotNull]
        private static List<BodyPartRecord> RandomizedChildren(BodyPartRecord record)
        {
            List<BodyPartRecord> children = record.parts.MakeSafe().ToList();
            children.Shuffle();
            return children;
        }


        /// <summary>add body part defs in to the given list in order of a 'randomized spread traversal' of the given body def</summary>
        /// <param name="bodyDef">The body definition.</param>
        /// <param name="outList">The out list.</param>
        /// <exception cref="ArgumentNullException">
        /// bodyDef
        /// or
        /// outList
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
    }
}