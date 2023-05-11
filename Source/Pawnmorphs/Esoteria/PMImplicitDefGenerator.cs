// PMImplicitDefGenerator.cs created by Iron Wolf for Pawnmorph on 10/09/2021 9:59 AM
// last updated 10/09/2021  9:59 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.Recipes;
using Pawnmorph.Things;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	///     static class that generates all implicit defs in the mod
	/// </summary>
	public static class PMImplicitDefGenerator
	{

		[NotNull] private static readonly List<Def> _defList = new List<Def>();
		[NotNull] private static readonly List<DefSt> _defSts = new List<DefSt>();

		[NotNull] private static readonly object[] tmpArr = new object[2];

		/// <summary>
		///     Generates the implicit defs.
		/// </summary>
		public static void GenerateImplicitDefs()
		{
			//note: do not put hybrid race generation here. that needs to be handled in main initialization 

			GenomeDefGenerator.GenerateGenomes(); //handles it's own hash's and resolve refs 
			_defList.Clear();
			MorphHediffGenerator.GenerateAllMorphHediffs();
			InjectorGenerator.GenerateInjectorDefs();
			//resolve non recipe def references on the generated defs 

			_defList.AddRange(InjectorGenerator.GeneratedInjectorDefs);
			_defList.AddRange(MorphHediffGenerator.AllGeneratedHediffDefs);

			foreach (Def def in _defList) def?.ResolveReferences();


			PMRecipeDefGenerator.GenerateRecipeDefs();
			foreach (RecipeDef allRecipe in PMRecipeDefGenerator.AllRecipes) allRecipe.ResolveReferences();


			_defList.AddRange(PMRecipeDefGenerator.AllRecipes);
			//configErrors 

			foreach (Def def in _defList)
			{
				List<string> errStrList = def.ConfigErrors().MakeSafe().ToList();
				if (errStrList.Count > 0)
				{
					string errStr = $"Errors in {def.defName}:\n{string.Join("\n", errStrList)}";
					Log.Error(errStr);
				}
			}

			_defList.Clear();
			//short hashs
			GiveHashes(InjectorGenerator.GeneratedInjectorDefs);
			GiveHashes(MorphHediffGenerator.AllGeneratedHediffDefs);
			GiveHashes(PMRecipeDefGenerator.AllRecipes);

			//debug log 
			//DebugOutput();

			//register defs 


			DefDatabase<HediffDef>.Add(MorphHediffGenerator.AllGeneratedHediffDefs);
			DefDatabase<RecipeDef>.Add(PMRecipeDefGenerator.AllRecipes);

			foreach (ThingDef tDef in InjectorGenerator.GeneratedInjectorDefs)
			{
				DefGenerator.AddImpliedDef(tDef);
			}

			ResourceCounter.ResetDefs();

		}

		private static void GiveHashes<T>(IEnumerable<T> items) where T : Def
		{
			foreach (T def in items)
				HashGiverUtils.GiveShortHash(def);
		}

		//private static void DebugOutput()
		//{
		//    var joinEnm = _defSts.GroupBy(d => d.type);
		//    StringBuilder builder = new StringBuilder();
		//    foreach (IGrouping<Type, DefSt> grouping in joinEnm)
		//    {
		//        builder.AppendLine($"def type {grouping.Key.Name}:");
		//        foreach (DefSt defSt in grouping)
		//        {
		//            builder.AppendLine($"\t{defSt.def.defName}");
		//        }
		//    }

		//    Log.Message(builder.ToString());
		//}

		private struct DefSt
		{
			public readonly Def def;
			public readonly Type type;

			public DefSt(Def d, Type tp)
			{
				def = d;
				type = tp;
			}
		}
	}
}