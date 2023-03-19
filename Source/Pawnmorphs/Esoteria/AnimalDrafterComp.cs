// AnimalDrafterComp.cs modified by Iron Wolf for Pawnmorph on 11/28/2019 8:24 AM
// last updated 11/28/2019  8:24 AM

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// thing comp to make animals draft-able when they have the 'former human (sapient)' hediff 
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	[StaticConstructorOnStartup]
	public class AnimalDrafterComp : ThingComp
	{
		[NotNull]
		private static readonly MethodInfo _getGizmoMethod;

		static AnimalDrafterComp()
		{

			_getGizmoMethod = typeof(Pawn_DraftController).GetMethod("GetGizmos", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (_getGizmoMethod == null)
			{
				Log.Error($"unable to find method \"GetGizmos\" from {nameof(Pawn_DraftController)}");

			}
		}


		[NotNull]
		Pawn Pawn
		{
			get
			{
				try
				{
					var p = (Pawn)parent;
					if (p == null) throw new InvalidCastException($"parent is null");
					return p;
				}
				catch (InvalidCastException e)
				{
					Log.Error($"could not cast {parent.Label} of type {parent.GetType().Name} to Pawn \n{e}");
					throw;
				}
			}
		}

		/// <summary>
		/// called to initialize this comp
		/// </summary>
		/// <param name="props">The props.</param>
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);

		}

		/// <summary>
		/// add gizmos to the thing 
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (!Pawn.RaceProps.Animal) yield break;
			//Log.Message("adding gizmos");
			//make sure it has the right comps 
			if (Pawn.drafter == null) yield break;

			var intelligence = Pawn.GetSapienceTracker()?.CurrentIntelligence ?? Intelligence.Animal;
			if (intelligence >= Intelligence.ToolUser)
			{
				var enumerator = (IEnumerable<Gizmo>)_getGizmoMethod.Invoke(Pawn.drafter, new object[] { });
				if (enumerator != null)
				{
					foreach (Gizmo gizmo in enumerator)

					{
						yield return gizmo;
					}
				}
			}

		}

	}
}