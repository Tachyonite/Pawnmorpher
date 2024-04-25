// MutRate_MutationsPerSeverityQuadratic.cs created by Iron Wolf for Pawnmorph on 09/05/2021 5:24 PM
// last updated 09/05/2021  5:24 PM

using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
	/// <summary>
	/// mute rate class where the mutation rate is proportional to a*s^2 + b * s + c where s is severity of the hediff 
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.Composable.MutRate" />
	public class MutRate_MutationsPerSeverityQuadratic : MutRate
	{

		[UsedImplicitly]
		float a, b, c;

		/// <summary>
		/// Whether or not the mutation rate is affected by mutagen sensitivity
		/// </summary>
		[UsedImplicitly] public bool affectedBySensitivity = true;

		/// <summary>
		/// How many mutations to queue up for the next second.
		/// 
		/// Called once a second by Hediff_MutagenicBase.  Queued up mutations will
		/// be spread out by that class, so no rate limiting needs to happen here.
		/// </summary>
		/// <returns>The number of mutations to add.</returns>
		/// <param name="hediff">Hediff.</param>
		public override int GetMutationsPerSecond(Hediff_MutagenicBase hediff)
		{
			float mutationPerDay = CurrentMutationRate(hediff);

			//Don't worry about division by zero, MTBEventOccurs handles positive infinity
			if (Rand.MTBEventOccurs(1f / mutationPerDay, 60000, 60))
				return 1;

			return 0;
		}


		float CurrentMutationRate(Hediff_MutagenicBase mBase) => (a * mBase.Severity * mBase.Severity + b * mBase.Severity + c) * SensitivityMultiplier(mBase);

		float SensitivityMultiplier(Hediff_MutagenicBase mBase) => affectedBySensitivity ? mBase.MutagenSensitivity : 1;

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public override string DebugString(Hediff_MutagenicBase hediff)
		{
			return $"mtb={CurrentMutationRate(hediff)}";
		}
	}
}