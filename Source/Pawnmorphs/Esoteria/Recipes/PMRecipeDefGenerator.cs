// PMRecipeDefGenerator.cs created by Iron Wolf for Pawnmorph on 10/26/2021 7:19 AM
// last updated 10/26/2021  7:19 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Pawnmorph.ModExtensions;
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

		[NotNull] private static readonly object[] _argList = new object[3];

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

			foreach (RecipeDef drugAdministerDef in DrugAdministerDefs())
			{
				if (drugAdministerDef != null)
					_generatedRecipeDefs.Add(drugAdministerDef);
			}
		}

		[NotNull]
		static IEnumerable<RecipeDef> DrugAdministerDefs()
		{

			List<ThingDef> items = new List<ThingDef>();

			items.AddRange(MorphDef.AllDefs.Select(m => m.injectorDef).Where(m => m != null && !m.IsDrug));

			//really hacky way of getting the serums but can't think of a better way 
			var serums = DefDatabase<ThingDef>.AllDefs.Where(td => td.HasModExtension<AdministerableExtension>());

			Type administerClass = typeof(Recipe_AdministerIngestible);
			foreach (ThingDef thingDef in serums)
			{
				if (!items.Contains(thingDef))
				{
					// Don't generate a recipe if a recipe already exists.
					// No sure way to know if a mod or item already has an administer recipe
					// Checking for identical label at least ensures we don't create a 100% duplicate.
					if (DefDatabase<RecipeDef>.AllDefsListForReading.Any(x => x.workerClass == administerClass && x.jobString == "RecipeAdministerJobString".Translate(thingDef.label)) == false)
						items.Add(thingDef);
				}
			}


			foreach (ThingDef thingDef in items)
			{
				yield return DrugAdministerDef(thingDef);
			}
		}

		private static RecipeDef DrugAdministerDef(ThingDef item)
		{
			var recipeDef = new RecipeDef
			{
				defName = "Administer_" + item.defName,
				label = "RecipeAdminister".Translate(item.label),
				jobString = "RecipeAdministerJobString".Translate(item.label),
				workerClass = typeof(Recipe_AdministerIngestible),
				targetsBodyPart = false,
				anesthetize = false,
				surgerySuccessChanceFactor = 99999f,
				modContentPack = item.modContentPack,
				workAmount = item.ingestible.baseIngestTicks,
				dontShowIfAnyIngredientMissing = true,
				recipeUsers = new List<ThingDef>()
			};
			var ingredientCount = new IngredientCount();
			ingredientCount.SetBaseCount(1f);
			ingredientCount.filter.SetAllow(item, true);
			recipeDef.ingredients.Add(ingredientCount);
			recipeDef.fixedIngredientFilter.SetAllow(item, true);
			foreach (ThingDef item2 in
				DefDatabase<ThingDef>.AllDefs.Where(d => d.category == ThingCategory.Pawn && d.race.IsFlesh))
				recipeDef.recipeUsers.Add(item2);
			return recipeDef;
		}

		private static RecipeDef CreateRecipeDefFromMaker(ThingDef def, int adjustedCount = 1, bool hotReload = false)
		{
			_argList[0] = def;
			_argList[1] = adjustedCount;
			_argList[2] = hotReload;
			return (RecipeDef)_createRecipeDefFromMaker.Invoke(null, _argList);
		}
	}
}