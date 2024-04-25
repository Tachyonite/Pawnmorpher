using Pawnmorph.Hediffs;
using Verse;

namespace Pawnmorph.UserInterface.PartPicker
{
	/// <summary>
	/// Mutation data stored in a template to be saved and loaded.
	/// </summary>
	/// <seealso cref="Verse.IExposable" />
	public class MutationTemplateData : IExposable
	{
		private MutationDef _mutationDef;
		private string _partLabelCap;
		private float _severity;
		private bool _halted;

		/// <summary>
		/// The saved mutation def
		/// </summary>
		public MutationDef MutationDef => _mutationDef;

		/// <summary>
		/// The name of the body part the mutation is attached to.
		/// </summary>
		public string PartLabelCap => _partLabelCap;

		/// <summary>
		/// The severity of the mutation
		/// </summary>
		public float Severity => _severity;

		/// <summary>
		/// Whether the mutation is halted or not
		/// </summary>
		public bool Halted => _halted;

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationTemplateData"/> class. Intended only for loading.
		/// </summary>
		public MutationTemplateData()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationTemplateData"/> class.
		/// </summary>
		/// <param name="mutation">The mutation def itself.</param>
		/// <param name="partLabelCap">The name of the body part the mutation is attached to.</param>
		/// <param name="severity">The severity of the stored mutation.</param>
		/// <param name="halted">Whether or not the mutation is halted.</param>
		public MutationTemplateData(MutationDef mutation, string partLabelCap, float severity, bool halted)
		{
			_mutationDef = mutation;
			_partLabelCap = partLabelCap;
			_severity = severity;
			_halted = halted;
		}

		/// <summary>
		/// Saves and loads data.
		/// </summary>
		public void ExposeData()
		{
			Scribe_Defs.Look(ref _mutationDef, nameof(MutationDef));
			Scribe_Values.Look(ref _partLabelCap, nameof(PartLabelCap));
			Scribe_Values.Look(ref _severity, nameof(Severity));
			Scribe_Values.Look(ref _halted, nameof(Halted));
		}
	}
}
