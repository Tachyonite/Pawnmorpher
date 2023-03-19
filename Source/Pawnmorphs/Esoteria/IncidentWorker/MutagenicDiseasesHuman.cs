// MutagenicDiseasesHuman.cs created by Iron Wolf for Pawnmorph on 02/22/2020 2:30 PM
// last updated 02/22/2020  2:30 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.IncidentWorkers
{
	/// <summary>
	/// incident worker for mutagenic diseases 
	/// </summary>
	/// <seealso cref="RimWorld.IncidentWorker_DiseaseHuman" />
	public class MutagenicDiseasesHuman : IncidentWorker_DiseaseHuman
	{
		private bool? _isEnabled;

		bool IsEnabled
		{
			get
			{
				if (_isEnabled == null)
				{
					_isEnabled = PMUtilities.GetSettings().enableMutagenDiseases;
				}

				return _isEnabled.Value;
			}
		}

		/// <summary>
		/// Gets all Potential victim candidates.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <returns></returns>
		protected override IEnumerable<Pawn> PotentialVictimCandidates(IIncidentTarget target)
		{
			return base.PotentialVictimCandidates(target).MakeSafe().Where(p => !p.IsFormerHuman()); //don't let former humans get these diseases
		}


		/// <summary>
		/// Determines whether this instance with the specified parms can fire now
		/// </summary>
		/// <param name="parms">The parms.</param>
		/// <returns>
		///   <c>true</c> if this instance with the specified parms can fire now otherwise, <c>false</c>.
		/// </returns>
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!IsEnabled) return false;//if mutagenic diseases are disabled in the settings menu never fire 
			return base.CanFireNowSub(parms);
		}
	}
}