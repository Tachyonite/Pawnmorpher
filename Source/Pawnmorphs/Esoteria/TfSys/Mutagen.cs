// Mutagen.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/14/2019 2:45 PM
// last updated 08/14/2019  2:45 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Multiplayer.API;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.TfSys
{
    /// <summary>
    /// base class for all mutagen types 
    /// </summary>
    public abstract class Mutagen
    {
        //some convenience stuff 

        private static PawnmorphGameComp _comp;

        //do this here so all the derived code doesn't have to         
        /// <summary>Gets the game comp.</summary>
        /// <value>The game comp.</value>
        protected static PawnmorphGameComp GameComp => _comp ?? (_comp = Find.World.GetComponent<PawnmorphGameComp>());

        /// <summary>
        /// tries to infer the faction responsible for turning the original pawn into an animal 
        /// </summary>
        /// <param name="originalPawn">The original pawn.</param>
        /// <returns></returns>
        [CanBeNull]
        protected Faction GetFactionResponsible([NotNull] Pawn originalPawn)
        {
            Faction responsibleFaction;
            if (originalPawn.IsPrisonerOfColony)
                responsibleFaction = Faction.OfPlayer;
            else if (originalPawn.IsColonist && originalPawn.IsFreeColonist)
                responsibleFaction = Faction.OfPlayer;
            else if (originalPawn.guest != null)
                responsibleFaction = originalPawn.guest.HostFaction;
            else
                responsibleFaction = null;

            return responsibleFaction;
        }


        /// <summary>The definition</summary>
        [NotNull]
        public MutagenDef def;

        /// <summary>
        /// Applies the apparel damage to the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="newRace">The new race.</param>
        /// <exception cref="System.ArgumentNullException">
        /// pawn
        /// or
        /// newRace
        /// </exception>
        protected void ApplyApparelDamage([NotNull] Pawn pawn, [NotNull] ThingDef newRace)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (newRace == null) throw new ArgumentNullException(nameof(newRace));
            TransformerUtility.ApplyTfDamageToApparel(pawn, newRace, def);
        }

        /// <summary>
        /// Transforms the specified request and preforms all necessary cleanup after the transformation if successful 
        /// </summary>
        /// implementers should make sure to preform all necessary cleanup of the pawn post transformation  
        /// <param name="request">The request.</param>
        /// <returns>the transformed pawn instance to be added to the database, should return null if the request cannot be met</returns>
        [CanBeNull] public abstract TransformedPawn Transform(TransformationRequest request); 

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
            if (!def.canInfectMechanoids && pawn.RaceProps.FleshType != FleshTypeDefOf.Normal) return false;
            var ext = pawn.def.GetModExtension<RaceMutagenExtension>();
            if (ext != null)
            {
                return !ext.immuneToAll && !ext.blackList.Contains(def);
            }

            return !HasAnyImmunizingHediffs(pawn); 
        }

        /// <summary>
        /// Determines whether the given pawn has any immunizing hediffs 
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if the given pawn has any immunizing hediffs; otherwise, <c>false</c>.
        /// </returns>
        protected bool HasAnyImmunizingHediffs([NotNull] Pawn pawn)
        {
            foreach (HediffDef immunizingDef in def.immunizingHediffs)
            {
                if (pawn.health.hediffSet.GetFirstHediffOfDef(immunizingDef) != null) return true; 
            }

            return false; 
        }


        /// <summary>
        /// Determines whether this instance can infect the specified race definition.
        /// </summary>
        /// <param name="raceDef">The race definition.</param>
        /// <returns>
        ///   <c>true</c> if this instance can infect the specified race definition; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanInfect(ThingDef raceDef)
        {
            if (raceDef.race == null) return false; 
            if (!def.canInfectAnimals && raceDef.race.Animal) return false;
            if (!def.canInfectMechanoids && raceDef.race.IsMechanoid) return false;
            var ext = raceDef.GetModExtension<RaceMutagenExtension>();
            if (ext != null)
            {
                return !ext.immuneToAll && !ext.blackList.Contains(def);
            }

            return true;
        }

        /// <summary>
        /// Determines whether this instance can transform the specified pawns.
        /// </summary>
        /// <param name="pawns">The pawns.</param>
        public virtual bool CanTransform(IEnumerable<Pawn> pawns)
        {
            int i = 0;
            foreach (var pawn in pawns)
            {
                if (!CanTransform(pawn))
                    return false;
                i++;
            }

            return i > 0; //just make sure there is actually a pawn to transform 
        }
        /// <summary>
        /// Determines whether this instance can transform the specified pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can transform the specified pawn; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanTransform(Pawn pawn)
        {
            return CanInfect(pawn) && pawn.Map != null; 
        }
        /// <summary>
        /// Determines whether this instance can transform the specified race definition.
        /// </summary>
        /// <param name="raceDef">The race definition.</param>
        /// <returns>
        ///   <c>true</c> if this instance can transform the specified race definition; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanTransform(ThingDef raceDef)
        {
            return CanInfect(raceDef); 
        }

        

        /// <summary>
        /// Try to revert the given instance of the transformed.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        public abstract bool TryRevert([NotNull]TransformedPawn transformedPawn);

        /// <summary>
        /// Tries to revert the given pawn.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        public abstract bool TryRevert([NotNull] Pawn transformedPawn); 

        /// <summary>
        /// Determines whether this instance can revert the specified transformed pawn.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can revert the specified transformed pawn; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool CanRevert([NotNull] TransformedPawn transformedPawn);

        /// <summary>
        /// Applies the post tf effects.
        /// this should be called just before the original pawn is cleaned up 
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="transformedPawn">The transformed pawn.</param>
        protected virtual void ApplyPostTfEffects(Pawn original, Pawn transformedPawn)
        {
            List<Aspect> aspects = new List<Aspect>(); 
            foreach (AspectGiver aspectGiver in def.tfAspectGivers.MakeSafe())
            {
                aspects.Clear();
                if (aspectGiver.TryGiveAspects(original, aspects))
                {
                    foreach (Aspect aspect in aspects) //make sure we add them to both pawns 
                    {
                        var aDef = aspect;
                        var tracker = transformedPawn.GetAspectTracker();
                        tracker?.Add(aDef, aspect.StageIndex); 
                    }
                }
            }

            transformedPawn.health.AddHediff(TfHediffDefOf.TransformationParalysis); 
        }
    }

    //generic base for convenience 

    /// <summary>
    /// generic base class for all mutagens for convenience 
    /// </summary>
    /// <typeparam name="T">the type of TransformedPawn this type consumes</typeparam>
    public abstract class Mutagen<T> : Mutagen where T: TransformedPawn
    {
        /// <summary>
        /// Determines whether this instance can revert pawn the specified transformed pawn.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can revert pawn  the specified transformed pawn; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool CanRevertPawnImp(T transformedPawn);

        /// <summary>
        /// preform the requested transform.
        /// </summary>
        /// implementers should make sure to preform any cleanup/hiding of the original pawns 
        /// <param name="request">The request.</param>
        /// <returns></returns>
        protected abstract T TransformImpl(TransformationRequest request);
        /// <summary>Returns true if the given request is valid.</summary>
        /// <param name="request">The request.</param>
        /// <returns>
        ///   <c>true</c> if the specified request is valid; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsValid(TransformationRequest request)
        {
            return request.IsValid;
        }

        /// <summary>
        /// Transforms the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [CanBeNull]
        public sealed override TransformedPawn Transform(TransformationRequest request)
        {
            if (!IsValid(request))
            {
                Log.Warning($"{def.defName} received an invalid transformation request!");

                return null;
            }

            if (!CanTransform(request.originals)) return null; 

            return TransformImpl(request); 
        }

        /// <summary>
        /// Determines whether this instance can revert the specified pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can revert the specified pawn; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">pawn</exception>
        /// <exception cref="InvalidTransformedPawnInstance">tfPawn instance of type {pawn.GetType().Name} can not be cast to {typeof(<typeparamref name="T"/>).Name}</exception>
        public sealed override bool CanRevert(TransformedPawn pawn)
        {
            if (pawn == null)
                throw new ArgumentNullException(nameof(pawn));
            bool reverted;

            if (MP.IsInMultiplayer) Rand.PushState(RandUtilities.MPSafeSeed);

            try
            {
                reverted = CanRevertPawnImp((T) pawn);
            }
            catch (InvalidCastException e)
            {
                if (MP.IsInMultiplayer) Rand.PopState();
                throw new
                    InvalidTransformedPawnInstance($"tfPawn instance of type {pawn.GetType().Name} can not be cast to {typeof(T).Name}",
                                                   e);
            }

            if (MP.IsInMultiplayer) Rand.PopState();

            return reverted; 
        }


        /// <summary>
        /// Try to revert the given instance of the transformed.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        public sealed override bool TryRevert(TransformedPawn transformedPawn)
        {
            if (transformedPawn == null) throw new ArgumentNullException(nameof(transformedPawn));
            bool reverted;
            try
            {
                if (MP.IsInMultiplayer) Rand.PushState(RandUtilities.MPSafeSeed);

                reverted =
                    TryRevertImpl((T) transformedPawn); //this class will handle all casting for us, and error appropriately 
            }
            catch (InvalidCastException e)
            {
                if (MP.IsInMultiplayer) Rand.PopState();
                throw new InvalidTransformedPawnInstance(
                                                         $"tfPawn instance of type {transformedPawn.GetType().Name} can not be cast to {typeof(T).Name}",
                                                         e);
            }
            if (MP.IsInMultiplayer) Rand.PopState();

            return reverted;
        }

        /// <summary>
        /// Tries to revert the transformed pawn instance, implementation.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        protected abstract bool TryRevertImpl(T transformedPawn);
    }


    /// <summary>
    /// exception thrown when an invalid TransformedPawn instance is encountered 
    /// </summary>
    /// <seealso cref="System.Exception" />
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