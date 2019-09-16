// MorphPawnKindExtension.cs created by Iron Wolf for Pawnmorph on 09/15/2019 9:43 PM
// last updated 09/15/2019  9:43 PM

using System.Collections.Generic;
using Verse;
using Multiplayer.API;
using Pawnmorph.Utilities;

namespace Pawnmorph
{
    /// <summary>
    /// mod extension for applying morphs to various PawnKinds 
    /// </summary>
    public class MorphPawnKindExtension : DefModExtension
    {
        public int maxHediffs = 5;
        public int maxInfluences = 1; //TODO also, I'm calling our custom 'traits' 'Influences' to avoid confusion with RM's traits until I can think of a better name 
        public float morphChange = 0.6f; //percentage, [0,1]

        public List<MorphCategoryDef> morphCategories = new List<MorphCategoryDef>();  
        //public List<InfluenceDef> influences; 
        public List<MutationCategoryDef> mutationCategories = new List<MutationCategoryDef>(); 
    }

    public class AddMorphsToPawn
    {
        public Pawn GetMorphsForPawnKind(Pawn pawn, PawnKindDef pawnKind)
        {
            /*
            if (MP.IsInMultiplayer)
            {
                Rand.PushState(RandUtilities.MPSafeSeed);
            }

            if (pawnKind.HasModExtension<MorphPawnKindExtension>()){

                MorphPawnKindExtension pKE = pawnKind.GetModExtension<MorphPawnKindExtension>();
                IEnumerable<MorphDef> mcd = MorphCategoryDefOf.Combat.AllMorphsInCategories;

                for (var i = 0; i < pKE.maxHediffs; i++)
                {
                    HediffDef hediff = mcd.RandomElement();
                    

                }

            }
            */
            return pawn;
        }
    }
}