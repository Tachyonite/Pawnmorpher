// FormerHumanSettings.cs modified by Iron Wolf for Pawnmorph on 12/24/2019 9:20 AM
// last updated 12/24/2019  9:20 AM

using Pawnmorph.FormerHumans;
using Pawnmorph.TfSys;
using RimWorld;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// def extension meant to be used on race defs to add setting specific to former human 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class FormerHumanSettings : DefModExtension

	{
		/// <summary>
		/// if true, the attached race will never be a former human 
		/// </summary>
		public bool neverFormerHuman;

		/// <summary>
		/// The backstory, uses a default if not set 
		/// </summary>
		public BackstoryDef backstory;

		/// <summary>
		/// The food thought settings
		/// </summary>
		public FoodThoughtSettings foodThoughtSettings;


		/// <summary>
		/// The manhunter settings
		/// </summary>
		public ManhunterTfSettings manhunterSettings = ManhunterTfSettings.Default;

		/// <summary>
		/// if non null this thought will be given when a pawn transforms into this pawn 
		/// </summary>
		public ThoughtDef transformedThought;
	}
}