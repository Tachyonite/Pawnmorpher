// Comp_TFStageConfigChecker.cs modified by Iron Wolf for Pawnmorph on 01/12/2020 2:14 PM
// last updated 01/12/2020  2:14 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// dummy comp for the tf stage config checker 
	/// </summary>
	/// <seealso cref="Verse.HediffComp" />
	public class Comp_TFStageConfigChecker : HediffComp
	{
		//empty 
	}

	/// <summary>
	/// simple comp for hediffs that will check all transformation stages for errors 
	/// </summary>
	/// <seealso cref="Verse.HediffCompProperties" />
	public class CompProps_TfStageConfigChecker : HediffCompProperties
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompProps_TfStageConfigChecker"/> class.
		/// </summary>
		public CompProps_TfStageConfigChecker()
		{
			compClass = typeof(Comp_TFStageConfigChecker);
		}

		/// <summary>
		/// returns all configuration errors 
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			var stages = parentDef.stages;
			if (stages == null || stages.Count == 0) yield break;
			for (int i = 0; i < stages.Count; i++)
			{
				HediffStage hediffStage = stages[i];
				if (hediffStage is IInitializable tfStage)
				{
					foreach (string configError in tfStage.ConfigErrors())
					{
						yield return $"in stage{i}) {configError}";
					}
				}

				if (hediffStage.hediffGivers != null)
				{
					for (int j = 0; j < hediffStage.hediffGivers.Count; j++)
					{
						var giver = hediffStage.hediffGivers[j];
						if (giver is IInitializable initGiver)
						{
							foreach (string configError in initGiver.ConfigErrors())
							{
								yield return $"in stage[{i}] hediffGiver[{j}]: {configError}";
							}
						}
					}
				}
			}
		}
	}
}