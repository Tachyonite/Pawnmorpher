// TransformationRequest.cs modified by Iron Wolf for Pawnmorph on 08/18/2019 8:55 AM
// last updated 08/18/2019  8:55 AM

using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph.TfSys
{
    public struct TransformationRequest
    {
        /// <summary>
        /// Returns true if this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid => originals != null && originals.Length > 0 && outputDef != null;

        
        public TransformationRequest(PawnKindDef outputDef, Pawn original)
        {
            originals = new [] {original};
            this.outputDef = outputDef;
            forcedGender = TFGender.Original;
            forcedGenderChance = 50; 
            cause = null;
            tale = null;

        }

        public TransformationRequest(PawnKindDef outputDef, params Pawn[] originals)
        {
            this.originals = originals;
            this.outputDef = outputDef;
            forcedGender = TFGender.Original;
            forcedGenderChance = 50;
            cause = null;
            tale = null; 
        }
        
        public Pawn[] originals;
        public PawnKindDef outputDef;
        public TFGender forcedGender;
        public float forcedGenderChance;
        public Hediff cause;
        public TaleDef tale; 


    }
}