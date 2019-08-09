using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class HediffCompProperties_Production : HediffCompProperties
    {
        public float daysToProduce = 1f;
        public int amount = 1;
        public int chance = 0;
        public ThoughtDef thought = null;
        public ThoughtDef wrongGenderThought = null;
        public ThoughtDef etherBondThought = null;
        public ThoughtDef etherBrokenThought = null;
        public Gender genderAversion = Gender.None;
        public string resource = "Chemfuel";
        public string rareResource;
        public List<HediffComp_Staged> stages = null;

        public HediffCompProperties_Production()
        {
            compClass = typeof(HediffComp_Production);
        }
    }
}
