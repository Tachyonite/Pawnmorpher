using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
	/// <summary>
	/// A class that determines what kind(s) of animals a pawn can be transformed into
	/// </summary>
	public abstract class TFTypes : IInitializableStage
	{
		/// <summary>
		/// Gets a pawn kind to transform the pawn into
		/// </summary>
		/// <returns>The mutations.</returns>
		/// <param name="hediff">Hediff.</param>
		public abstract PawnKindDef GetTF(Hediff_MutagenicBase hediff);

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public virtual string DebugString(Hediff_MutagenicBase hediff) => "";


		/// <summary>
		/// gets all configuration errors in this stage .
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public virtual IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Resolves all references in this instance.
		/// </summary>
		/// <param name="parent">The parent.</param>
		public virtual void ResolveReferences(HediffDef parent)
		{

		}
	}

	/// <summary>
	/// A simple TFTypes that allows a transformation into a random chaomorph
	/// </summary>
	public class TFTypes_Chao : TFTypes
	{
		/// <summary>
		/// Gets a pawn kind to transform the pawn into
		/// </summary>
		/// <returns>The mutations.</returns>
		/// <param name="hediff">Hediff.</param>
		public override PawnKindDef GetTF(Hediff_MutagenicBase hediff)
		{
			return ChaomorphUtilities.GetRandomChaomorphPK(ChaomorphType.Chaomorph);
		}
	}

	/// <summary>
	/// A simple TFTypes that allows a transformation into ALL THE ANIMALS _O/
	/// Good for chaotic mutations.
	/// </summary>
	public class TFTypes_All : TFTypes
	{
		/// <summary>
		/// The black list of animals that will not be chosen 
		/// </summary>
		[NotNull]
		public List<PawnKindDef> blackList = new List<PawnKindDef>();

		/// <summary>
		/// Gets a pawn kind to transform the pawn into
		/// </summary>
		/// <returns>The mutations.</returns>
		/// <param name="hediff">Hediff.</param>
		public override PawnKindDef GetTF(Hediff_MutagenicBase hediff)
		{
			return FormerHumanUtilities.AllRegularFormerHumanPawnkindDefs
									   .Where(pk => !blackList.Contains(pk))
									   .RandomElement();
		}
	}

	/// <summary>
	/// A simple TFTypes that accepts a list of pawn kinds directly from the XML
	/// </summary>
	public class TFTypes_List : TFTypes
	{
		/// <summary>
		/// The list of PawnKindDefs that this TF can potentially transform into.
		/// </summary>
		[UsedImplicitly] public List<PawnKindDef> animals;

		/// <summary>
		/// Gets a pawn kind to transform the pawn into
		/// </summary>
		/// <returns>The mutations.</returns>
		/// <param name="hediff">Hediff.</param>
		public override PawnKindDef GetTF(Hediff_MutagenicBase hediff)
		{
			return animals.RandomElement();
		}
	}

	/// <summary>
	/// A simple TFTypes that selects a random pawn kind from a morph def
	/// </summary>
	public class TFTypes_Morph : TFTypes
	{
		/// <summary>
		/// The morph def to get potential animal forms from.
		/// </summary>
		[UsedImplicitly] public MorphDef morphDef;

		/// <summary>
		/// Gets a pawn kind to transform the pawn into
		/// </summary>
		/// <returns>The mutations.</returns>
		/// <param name="hediff">Hediff.</param>
		public override PawnKindDef GetTF(Hediff_MutagenicBase hediff)
		{
			return morphDef.FeralPawnKinds.RandomElement();
		}

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public override string DebugString(Hediff_MutagenicBase hediff) => $"MorphDef: {morphDef.defName}";
	}

	/// <summary>
	/// A simple TFTypes that selects a random pawn kind from a class (including child classes)
	/// </summary>
	public class TFTypes_Class : TFTypes
	{
		/// <summary>
		/// The class def to get potential animals from.
		/// </summary>
		[UsedImplicitly] public AnimalClassDef classDef;

		/// <summary>
		/// Gets a pawn kind to transform the pawn into
		/// </summary>
		/// <returns>The mutations.</returns>
		/// <param name="hediff">Hediff.</param>
		public override PawnKindDef GetTF(Hediff_MutagenicBase hediff)
		{
			return classDef.GetAllMorphsInClass()
					.SelectMany(m => m.FeralPawnKinds)
					.Distinct()
					.RandomElement();
		}

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public override string DebugString(Hediff_MutagenicBase hediff) => $"ClassDef: {classDef.defName}";
	}

	/// <summary>
	/// A TFTypes that selects a random pawn kind from in HediffComp_MutTypes
	/// 
	/// Most "dynamic" hediffs that want to share mutation data across stages will
	/// want to use this TFTypes, as TFTypes are stateless.
	/// </summary>
	public class TFTypes_FromComp : TFTypes
	{
		/// <summary>
		/// Gets a pawn kind to transform the pawn into
		/// </summary>
		/// <returns>The mutations.</returns>
		/// <param name="hediff">Hediff.</param>
		public override PawnKindDef GetTF(Hediff_MutagenicBase hediff)
		{
			return hediff.TryGetComp<HediffComp_MutTypeBase>()
					.GetTFs()
					.RandomElement();
		}
	}
}
