// ChamberAnimalTfController.cs created by Iron Wolf for Pawnmorph on 06/25/2021 5:31 PM
// last updated 06/25/2021  5:31 PM

using System;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace Pawnmorph.Chambers
{
	/// <summary>
	///     abstract class for all chamber animal tf controllers
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public abstract class ChamberAnimalTfController : DefModExtension
	{
		/// <summary>
		///     Determines whether this instance with the specified pawn can initiate the transformation into the specified animal
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="targetAnimal">The target animal.</param>
		/// <param name="chamber">The chamber.</param>
		/// <returns></returns>
		public abstract ChamberTfInitiationReport CanInitiateTransformation([NotNull] Pawn pawn,
																			[NotNull] PawnKindDef targetAnimal,
																			[NotNull] MutaChamber chamber);

		/// <summary>
		///     Initiates the transformation of the specified pawn in the given chamber into the target animal
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="targetAnimal">The target animal.</param>
		/// <param name="chamber">The chamber.</param>
		/// <returns>struct containing the pawnkindDef the pawn will turn into and the duration of the transformation</returns>
		public abstract ChamberAnimalTfInitStruct InitiateTransformation([NotNull] Pawn pawn, [NotNull] PawnKindDef targetAnimal,
														   [NotNull] MutaChamber chamber);

		/// <summary>
		/// Called when the pawn is ejected either in a full tf or an aborted transformation 
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="transformedPawn">The transformed pawn. null if the chamber ejected the pawn before the transformation finished</param>
		/// <param name="chamber">The chamber.</param>
		public abstract void OnPawnEjects([NotNull] Pawn original, [CanBeNull] Pawn transformedPawn, [NotNull] MutaChamber chamber);

	}


	/// <summary>
	/// simple struct wrapping output of an initialized transformation 
	/// </summary>
	/// POD that contains the pawnkinddef and duration of an animal tf 
	/// <seealso cref="ChamberAnimalTfInitStruct" />
	public readonly struct ChamberAnimalTfInitStruct : IEquatable<ChamberAnimalTfInitStruct>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ChamberAnimalTfInitStruct" /> struct.
		/// </summary>
		/// <param name="pawnKindDef">The pawn kind definition.</param>
		/// <param name="duration">The duration of the tf in days.</param>
		/// <param name="specialResource">The special resource needed to start the transformation</param>
		public ChamberAnimalTfInitStruct([NotNull] PawnKindDef pawnKindDef, float duration, ThingDef specialResource = null)
		{
			pawnkindDef = pawnKindDef;
			this.duration = duration;
			this.specialResource = specialResource;
		}

		/// <summary>
		/// the special resource needed to start the transformation 
		/// </summary>
		[CanBeNull]
		public readonly ThingDef specialResource;
		/// <summary>
		/// The pawnkind the pawn will turn into 
		/// </summary>
		[NotNull]
		public readonly PawnKindDef pawnkindDef;
		/// <summary>
		/// The duration 
		/// </summary>
		public readonly float duration;

		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>The fully qualified type name.</returns>
		public override string ToString()
		{
			return $"{pawnkindDef?.defName ?? "NULL"}:{duration} {specialResource?.defName ?? ""}";
		}


		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
		public bool Equals(ChamberAnimalTfInitStruct other)
		{
			return Equals(specialResource, other.specialResource)
				&& Equals(pawnkindDef, other.pawnkindDef)
				&& duration == other.duration;
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with the current instance. </param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />. </returns>
		public override bool Equals(object obj)
		{
			return obj is ChamberAnimalTfInitStruct other && Equals(other);
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = (specialResource != null ? specialResource.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (pawnkindDef != null ? pawnkindDef.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Mathf.RoundToInt(duration);
				return hashCode;
			}
		}

		/// <summary>Returns a value that indicates whether the values of two <see cref="T:Pawnmorph.Chambers.ChamberAnimalTfInitStruct" /> objects are equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
		public static bool operator ==(ChamberAnimalTfInitStruct left, ChamberAnimalTfInitStruct right)
		{
			return left.Equals(right);
		}

		/// <summary>Returns a value that indicates whether two <see cref="T:Pawnmorph.Chambers.ChamberAnimalTfInitStruct" /> objects have different values.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
		public static bool operator !=(ChamberAnimalTfInitStruct left, ChamberAnimalTfInitStruct right)
		{
			return !left.Equals(right);
		}
	}
}