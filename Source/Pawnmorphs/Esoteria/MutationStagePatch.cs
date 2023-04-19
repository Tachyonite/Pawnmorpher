using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// Mutation stage patch that can be included in a <see cref="MutationDef" /> to allow modifying mutation stages in derived xml files with shared stages.
	/// </summary>
	public class MutationStagePatch
	{
		/// <summary>
		/// Key of the stage to affect when using modify or remove function.
		/// </summary>
		[CanBeNull]
		string stageKey = null;

		/// <summary>
		/// The patch behavior. Can be either "add", "modify" or "remove".
		/// </summary>
		[NotNull]
		string function = "modify";

		/// <summary>
		/// The mutation stage containing the values to use to either update an existing stage or append and entirely new stage.
		/// </summary>
		[CanBeNull]
		MutationStage values = null;

		/// <summary>
		/// Applies the specified stage patch.
		/// </summary>
		/// <param name="mutation">The mutation.</param>
		public void Apply(MutationDef mutation)
		{
			switch (function)
			{
				case "add":
					Add(mutation);
					break;

				case "modify":
					Modify(mutation);
					break;

				case "remove":
					Remove(mutation);
					break;

				default:
					Log.Warning($"Invalid mutation stage patch function: {function} in {mutation.ToString()}");
					break;
			}

		}

		private void Add(MutationDef mutation)
		{
			if (values == null)
				return;

			// Keep stage list sorted by minSeverity ASC, otherwise Rimworld throws a warning in the log.
			for (int i = 0; i < mutation.stages.Count; i++)
			{
				if (mutation.stages[i].minSeverity < values.minSeverity)
					continue;

				mutation.stages.Insert(i, values);
			}
		}

		private void Remove(MutationDef mutation)
		{
			if (string.IsNullOrWhiteSpace(this.stageKey))
				return;

			MutationStage stage = TryGetStage(mutation);
			if (stage != null)
				mutation.stages.Remove(stage);
		}



		private void Modify(MutationDef mutation)
		{
			if (string.IsNullOrWhiteSpace(this.stageKey))
				return;

			if (values == null)
				return;

			MutationStage stage = TryGetStage(mutation);
			if (stage != null)
			{
				// Get public instance fields that can be set.
				System.Reflection.FieldInfo[] members = typeof(MutationStage).GetFields(System.Reflection.BindingFlags.Public |
																						System.Reflection.BindingFlags.Instance);

				MutationStage defaultValues = new MutationStage();
				foreach (System.Reflection.FieldInfo member in members)
				{
					object newValue = member.GetValue(values);

					if (newValue != null)
					{
						// get default value
						object defaultValue = member.GetValue(defaultValues);

						if (newValue is ICollection collection)
						{
							if (collection.Count == 0)
								continue;

							IList currentCollection = member.GetValue(stage) as IList;

							// If there is already a collection, then append new values otherwise simply assign the new one
							if (currentCollection != null)
							{
								Type genericType = currentCollection.GetType().GenericTypeArguments[0];
								foreach (object value in collection)
								{
									if (genericType == typeof(PawnCapacityModifier))
									{
										IList<PawnCapacityModifier> capMods = (IList<PawnCapacityModifier>)currentCollection;
										PawnCapacityModifier newCapValue = (PawnCapacityModifier)value;

										int index = capMods.FirstIndexOf(x => x.capacity == newCapValue.capacity);
										if (index > -1)
											currentCollection.RemoveAt(index);
									}

									if (genericType == typeof(StatModifier))
									{
										IList<StatModifier> statMods = (IList<StatModifier>)currentCollection;
										StatModifier newStatValue = (StatModifier)value;
										int index = statMods.FirstIndexOf(x => x.stat == newStatValue.stat);
										if (index > -1)
											currentCollection.RemoveAt(index);
									}
									currentCollection.Add(value);
								}
							}
							else
								currentCollection = (IList)collection;

							newValue = currentCollection;
						}
						else if (newValue.Equals(defaultValue))
							continue;

						member.SetValue(stage, newValue);
					}
				}
			}
		}

		/// <summary>
		/// Attempts to find and return the stage with identical key from the provided <see cref="MutationDef"/>. Logs a warning and returns null if not found.
		/// </summary>
		private MutationStage TryGetStage(MutationDef mutation)
		{
			MutationStage stage = mutation.stages.FirstOrDefault(x => ((MutationStage)x).key == stageKey) as MutationStage;
			if (stage == null)
				Log.Warning($"unable to find stage with key: {stageKey} in {mutation.ToString()}");

			return stage;
		}

	}
}
