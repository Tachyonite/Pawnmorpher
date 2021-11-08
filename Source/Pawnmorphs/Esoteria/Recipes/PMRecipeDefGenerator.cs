// PMRecipeDefGenerator.cs created by Iron Wolf for Pawnmorph on 10/26/2021 7:19 AM
// last updated 10/26/2021  7:19 AM

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Pawnmorph.Things;
using RimWorld;
using Verse;

namespace Pawnmorph.Recipes
{
    /// <summary>
    ///     static class that generated implicit recipe defs
    /// </summary>
    public static class PMRecipeDefGenerator
    {
        [NotNull] private static readonly MethodInfo _createRecipeDefFromMaker;

        [NotNull] private static readonly List<RecipeDef> _generatedRecipeDefs = new List<RecipeDef>();

        [NotNull] private static readonly object[] _argList = new object[2];

        static PMRecipeDefGenerator()
        {
            _createRecipeDefFromMaker =
                typeof(RecipeDefGenerator).GetMethod("CreateRecipeDefFromMaker", BindingFlags.Static | BindingFlags.NonPublic);
        }

        /// <summary>
        ///     Gets all recipe defs .
        /// </summary>
        /// <value>
        ///     All recipes.
        /// </value>
        [NotNull]
        public static IReadOnlyList<RecipeDef> AllRecipes => _generatedRecipeDefs;

        /// <summary>
        ///     Generates the recipe defs.
        /// </summary>
        public static void GenerateRecipeDefs()
        {
            foreach (ThingDef injectorDef in InjectorGenerator.GeneratedInjectorDefs)
            {
                RecipeDef recipe = CreateRecipeDefFromMaker(injectorDef);
                _generatedRecipeDefs.Add(recipe);
            }
        }

        private static RecipeDef CreateRecipeDefFromMaker(ThingDef def, int adjustedCount = 1)
        {
            _argList[0] = def;
            _argList[1] = adjustedCount;
            return (RecipeDef) _createRecipeDefFromMaker.Invoke(null, _argList);
        }
    }
}