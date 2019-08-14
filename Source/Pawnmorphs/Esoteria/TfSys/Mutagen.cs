// Mutagen.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/14/2019 2:45 PM
// last updated 08/14/2019  2:45 PM

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.TfSys
{
    /// <summary>
    /// base class for all mutagen types 
    /// </summary>
    public abstract class Mutagen
    {
        public MutagenDef def;

        /// <summary>
        /// Transforms the pawns into one instance of the given race.
        /// </summary>
        /// <param name="originals">The originals.</param>
        /// <param name="outputRace">The output race.</param>
        /// <param name="forcedGender"></param>
        /// <param name="forceGenderChance"></param>
        /// <param name="cause">The cause of the transformation.</param>
        /// <returns></returns>
        public abstract TransformedPawn TransformPawns(IEnumerable<Pawn> originals,
            ThingDef outputRace, TFGender forcedGender = TFGender.Original, float forceGenderChance = 50F,
            [CanBeNull] Hediff_Morph cause = null);


        /// <summary>
        /// Determines whether this instance can infect the specified pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can infect the specified pawn; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanInfect(Pawn pawn)
        {
            if (!def.canInfectAnimals && pawn.RaceProps.Animal) return false;
            if (!def.canInfectMechanoids && pawn.RaceProps.IsMechanoid) return false;
            var ext = pawn.def.GetModExtension<RaceMutagenExtension>();
            if (ext != null)
            {
                return !ext.immuneToAll && !ext.blackList.Contains(def);
            }

            return true;
        }

        /// <summary>
        /// Try to revert the given instance of the transformed.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        public abstract bool TryRevert([NotNull]TransformedPawn transformedPawn);
    }

    //generic base for convenience 

    public abstract class Mutagen<T> : Mutagen where T: TransformedPawn
    {
        /// <summary>
        /// Transforms the pawns into a TransformedPawn instance of the given ace .
        /// </summary>
        /// <param name="originals">The originals.</param>
        /// <param name="outputRace">The output race.</param>
        /// <param name="forcedGenderChance"></param>
        /// <param name="cause">The cause.</param>
        /// <param name="forcedGender"></param>
        /// <returns></returns>
        protected abstract T TransformPawnsImpl(IEnumerable<Pawn> originals, ThingDef outputRace, TFGender forcedGender, float forcedGenderChance,
            Hediff_Morph cause);


        /// <summary>
        /// Transforms the pawns into one instance of the given race.
        /// </summary>
        /// <param name="originals">The originals.</param>
        /// <param name="outputRace">The output race.</param>
        /// <param name="forcedGender"></param>
        /// <param name="forceGenderChance"></param>
        /// <param name="cause">The cause of the transformation.</param>
        /// <returns></returns>
        public sealed override TransformedPawn TransformPawns(IEnumerable<Pawn> originals, ThingDef outputRace,  TFGender forcedGender=TFGender.Original, float forceGenderChance=50, [CanBeNull] Hediff_Morph cause = null)
        {
            var tfPawn = TransformPawnsImpl(originals, outputRace, forcedGender, forceGenderChance,  cause);
            tfPawn.mutagenDef = def; //base class will always assign the correct def 
            return tfPawn; 
        }

        /// <summary>
        /// Try to revert the given instance of the transformed.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        public sealed override bool TryRevert(TransformedPawn transformedPawn)
        {
            if (transformedPawn == null) throw new ArgumentNullException(nameof(transformedPawn));
            try
            {
                return TryRevertImpl((T) transformedPawn); //this class will handle all casting for us, and error appropriately 
            }
            catch (InvalidCastException e)
            {
                throw new InvalidTransformedPawnInstance(
                    $"tfPawn instance of type {transformedPawn.GetType().Name} can not be cast to {typeof(T).Name}", e); 
            }
        }

        /// <summary>
        /// Tries to revert the transformed pawn instance, implementation.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        protected abstract bool TryRevertImpl(T transformedPawn); 


    }


    public class InvalidTransformedPawnInstance : System.Exception
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Exception" /> class.</summary>
        public InvalidTransformedPawnInstance()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message.</summary>
        /// <param name="message">The message that describes the error. </param>
        public InvalidTransformedPawnInstance(string message) : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Exception" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
        /// <param name="message">The error message that explains the reason for the exception. </param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public InvalidTransformedPawnInstance(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Exception" /> class with serialized data.</summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0). </exception>
        protected InvalidTransformedPawnInstance([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}