// RaceMutagenExtension.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:10 PM
// last updated 08/13/2019  4:10 PM

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
	/// <summary> Extension used to blacklist a race from one or more mutagen strains. </summary>
	public class RaceMutationSettingsExtension : DefModExtension
	{
		/// <summary>if to make this race immune to all mutations</summary>
		public bool immuneToAll;


		/// <summary>
		/// The mutation retrievers
		/// </summary>
		[CanBeNull]
		public List<IRaceMutationRetriever> mutationRetrievers;

		/// <summary>
		/// gets all configuration errors with this instance 
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors().MakeSafe())
			{
				yield return configError;
			}

			if (immuneToAll && (mutationRetrievers != null && mutationRetrievers.Count != 0))
			{
				yield return $"{nameof(immuneToAll)} is true but {nameof(mutationRetrievers)} is set!";
			}

			if (mutationRetrievers != null)
			{
				List<string> lst = new List<string>();
				StringBuilder builder = new StringBuilder();
				foreach (IRaceMutationRetriever retriever in mutationRetrievers)
				{
					lst.Clear();
					builder.Clear();
					lst.AddRange(retriever.GetConfigErrors());
					if (lst.Count != 0)
					{
						builder.AppendLine($"encountered errors in retriever: {retriever.GetType().Name}!");
						foreach (string err in lst)
						{
							builder.AppendLine("\t" + err);
						}

						yield return builder.ToString();
					}
				}
			}
		}
	}


	/// <summary>
	/// 
	/// </summary>
	public static class RaceMutationSettingsCacher
	{
		private static Dictionary<ThingDef, RaceMutationSettingsExtension> _cache =
			new Dictionary<ThingDef, RaceMutationSettingsExtension>();


		/// <summary>
		/// Tries to get the race mutation settings.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		[CanBeNull]
		public static RaceMutationSettingsExtension TryGetRaceMutationSettings([NotNull] this Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			return pawn.def.TryGetRaceMutationSettings();
		}

		/// <summary>
		/// Tries the get race mutation settings.
		/// </summary>
		/// <param name="raceDef">The race definition.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">raceDef</exception>
		[CanBeNull]
		public static RaceMutationSettingsExtension TryGetRaceMutationSettings([NotNull] this ThingDef raceDef)
		{
			if (raceDef == null) throw new ArgumentNullException(nameof(raceDef));

			if (_cache.TryGetValue(raceDef, out var ext))
			{
				return ext;
			}

			ext = raceDef.GetModExtension<RaceMutationSettingsExtension>();
			_cache[raceDef] = ext;
			return ext;
		}


	}

}