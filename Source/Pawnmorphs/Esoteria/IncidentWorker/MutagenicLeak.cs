using System.Linq;
using RimWorld;
using Pawnmorph.SlurryNet;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace Pawnmorph.IncidentWorkers
{
    /// <summary>
    /// Incident worker for the mutagenic leak
    /// </summary>
    /// <seealso cref="RimWorld.IncidentWorker" />
    class MutagenicLeak : IncidentWorker
    {
        /// <summary>
        ///     Determines whether this instance with the specified parms [can fire now sub]
        /// </summary>
        /// <param name="parms">The params.</param>
        /// <returns>
        ///     <c>true</c> If this instance with the specified parms  [can fire now sub] otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return GetLeakableNetworks((Map)parms.target).Any();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            List<SlurryNet.SlurryNet> leakableNetworks = GetLeakableNetworks((Map)parms.target);
            if (!leakableNetworks.Any())
                return false;

            int netIndex = Random.Range(0, leakableNetworks.Count);

            DoLeak(leakableNetworks[netIndex]);
            return true;
        }

        public static List<SlurryNet.SlurryNet> GetLeakableNetworks(Map map)
        {
            List<SlurryNet.SlurryNet> leakableNetworks = new List<SlurryNet.SlurryNet>();
            SlurryNetManager netManager = SlurryNetUtilities.GetSlurryNetManager(map);
            IReadOnlyList<SlurryNet.SlurryNet> nets = netManager.Nets;
            for (int i = 0; i < nets.Count; i++)
            {
                if (nets[i].Stored == 0)
                    continue;

                leakableNetworks.Add(nets[i]);
            }
            return leakableNetworks;
        }

        public static void DoLeak(SlurryNet.SlurryNet culpritNetwork)
        {
            List<SlurryNetComp> connectors = culpritNetwork.Connectors.ToList();
            int pipeIndex = Random.Range(0, connectors.Count);

            Building culpritPipe = (Building)connectors[pipeIndex].parent;
            Map map = culpritPipe.Map;
            float explosionRadius = 3f;

            GenExplosion.DoExplosion(culpritPipe.Position, map, explosionRadius, PMDamageDefOf.MutagenCloud, null,-1,-1,null,null,null,null,PMThingDefOf.PM_Filth_Slurry,0.5f,1);
            Find.LetterStack.ReceiveLetter("LetterLabelMutagenicLeak".Translate(), "LetterTextMutagenicLeak".Translate(), LetterDefOf.NegativeEvent, new TargetInfo(culpritPipe.Position, map));

            //To Complete.
        }

    }
}
