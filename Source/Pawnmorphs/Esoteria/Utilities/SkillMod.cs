// SkillMod.cs modified by Iron Wolf for Pawnmorph on 09/29/2019 7:32 AM
// last updated 09/29/2019  7:32 AM

using RimWorld;
using UnityEngine;

namespace Pawnmorph.Utilities
{
    public class SkillMod
    {
        public SkillDef skillDef;
        public float addedXp;
        public int passionOffset;
        public Passion? forcedPassion;
        /// <summary>
        /// the new passion of the skill with this mod 
        /// </summary>
        /// <param name="oldPassion"></param>
        /// <returns></returns>
        public Passion GetNewPassion(Passion oldPassion)
        {
            if (forcedPassion != null) return forcedPassion.Value;

            int nP = (int) oldPassion + passionOffset;
            nP = Mathf.Clamp(nP, 0, 2);
            return (Passion) nP; 

        }
    }

}