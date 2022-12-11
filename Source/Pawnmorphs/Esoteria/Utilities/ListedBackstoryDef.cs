using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph
{
    public class ListedBackstoryDef : AlienRace.AlienBackstoryDef
    {

        public List<WorkTags> workAllowsList = new List<WorkTags>();


        public override void ResolveReferences()
        {
            if (workAllowsList.Count > 0)
            {
                WorkTags allowed = WorkTags.None;

                foreach (WorkTags workAllowEntry in workAllowsList)
                    allowed |= workAllowEntry;

                workAllows = allowed;
            }

            base.ResolveReferences();

            workDisables ^= WorkTags.AllWork;
        }

    }
}
