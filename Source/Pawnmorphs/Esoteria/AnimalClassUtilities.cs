// AnimalClassUtilities.cs modified by Iron Wolf for Pawnmorph on 01/10/2020 6:51 PM
// last updated 01/10/2020  6:51 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// static container for various animal classification related utility functions 
    /// </summary>
    [StaticConstructorOnStartup]
    public class AnimalClassUtilities
    {

        internal static List<IAnimalClass> PreorderTreeInternal { get; }

        internal static List<IAnimalClass> PostorderTreeInternal { get; }



        static AnimalClassUtilities()
        {
            foreach (AnimalClassDef animalClassDef in DefDatabase<AnimalClassDef>.AllDefs)
                animalClassDef.FindChildren(); //have to do this after all other def's 'ResolveReferences' have been called 

            foreach (AnimalClassDef animalClassDef in DefDatabase<AnimalClassDef>.AllDefs)
            {
                if(animalClassDef.parent != null) continue;
                if(animalClassDef != AnimalClassDefOf.Animal)
                    Log.Warning($"{animalClassDef.defName} does not have a parent! only {nameof(AnimalClassDefOf.Animal)} should not have a parent!");
            }

            
            if (CheckForCycles()) return; //don't precede if there are any cycles in the tree 


            //save the pre and post order traversal orders for performance reasons 
            PostorderTreeInternal = TreeUtilities.Postorder<IAnimalClass>(AnimalClassDefOf.Animal, c => c.Children);
            PreorderTreeInternal = TreeUtilities.Preorder<IAnimalClass>(AnimalClassDefOf.Animal, c => c.Children).ToList();

            var treeStr =
                TreeUtilities.PrettyPrintTree<IAnimalClass>(AnimalClassDefOf.Animal, a => a.Children, a => ((Def)a).defName);

            Log.Message(treeStr); //print a pretty tree c: 

        }

        private static bool CheckForCycles()
        {
            var visitedSet = new HashSet<AnimalClassDef>();

            var stk = new Stack<AnimalClassDef>();
            stk.Push(AnimalClassDefOf.Animal);

            var anyLoops = false;
            while (stk.Count > 0) //simple preorder traversal while checking for loops 
            {
                AnimalClassDef classification = stk.Pop();
                visitedSet.Add(classification);
                foreach (AnimalClassDef subClass in classification.SubClasses)
                {
                    if (visitedSet.Contains(subClass))
                    {
                        anyLoops = true;
                        Log.Error($"visited {subClass.defName} more then once! there must be a cycle in the classifications somewhere!");
                        continue;
                    }

                    stk.Push(subClass);
                }
            }

            return anyLoops;
        }
    }
}