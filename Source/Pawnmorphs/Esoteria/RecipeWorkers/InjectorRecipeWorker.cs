using Pawnmorph.Chambers;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph.RecipeWorkers
{
	/// <summary>
	/// Recipe worker that adds the tagging condition to making injectors.
	/// </summary>
	/// <seealso cref="Verse.RecipeWorker" />
	internal class InjectorRecipeWorker : RecipeWorker
	{
		static PawnmorpherSettings _settings;

		/// <summary>
		/// Assigns this recipe worker to all injector recipes.
		/// </summary>
		internal static void PatchInjectors()
		{
			_settings = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>();

			List<ThingDef> _injectors = PMThingCategoryDefOf.Injector.SortedChildThingDefs;
			List<RecipeDef> recipes = DefDatabase<RecipeDef>.AllDefsListForReading;
			foreach (var injector in _injectors)
			{
				RecipeDef recipe = recipes.FirstOrDefault(x => x.ProducedThingDef == injector);
				if (recipe != null)
					recipe.workerClass = typeof(InjectorRecipeWorker);
			}
		}

		private MorphDef _morph;
		private bool _initialized;

		private MorphDef GetMorphDef()
		{
			IngestionOutcomeDoer_GiveHediff hediff = recipe.ProducedThingDef.ingestible?.outcomeDoers.OfType<IngestionOutcomeDoer_GiveHediff>().FirstOrDefault();
			if (hediff != null && hediff.hediffDef != null)
				return MorphDef.AllDefs.FirstOrDefault(m => m.fullTransformation == hediff.hediffDef);

			return null;
		}

		public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
		{
			if (_settings.injectorsRequireTagging == false)
				return true;

			if (_initialized == false)
			{
				_morph = GetMorphDef();
				_initialized = true;
			}

			if (_morph == null)
				return true;

			return _morph.IsTagged();
		}
	}
}
