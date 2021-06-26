using System.Text;
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
        ///     Determines whether this instance with the specified parms [can fire now sub].
        /// </summary>
        /// <param name="parms">The params.</param>
        /// <returns>
        ///     <c>true</c> If this instance with the specified parms  [can fire now sub] otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!PMUtilities.GetSettings().enableMutagenLeak) 
                return false;
            return GetLeakableNetworks((Map)parms.target).Any();
        }

        /// <summary>
        ///     Tries the execute worker.
        /// </summary>
        /// <param name="parms">The parms.</param>
        /// <returns>
        ///     <c>true</c> If the worker was executed, <c>false</c> otherwise.
        /// </returns>
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            List<SlurryNet.SlurryNet> leakableNetworks = GetLeakableNetworks((Map)parms.target);
            if (!leakableNetworks.Any())
                return false;

            int netIndex = UnityEngine.Random.Range(0, leakableNetworks.Count);

            DoLeak(leakableNetworks[netIndex]);
            return true;
        }

        /// <summary>
        ///     Get all the slurry networks that can leak.
        /// </summary>
        /// <param name="map">The map of the colony.</param>
        /// <returns>
        ///     The networks that can leak.
        /// </returns>
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

        /// <summary>
        ///     Make a network explode and leak.
        /// </summary>
        /// <param name="culpritNetwork">The network where the leak happens.</param>
        public static void DoLeak(SlurryNet.SlurryNet culpritNetwork)
        {
            List<SlurryNetComp> connectors = culpritNetwork.Connectors.ToList();
            int pipeIndex = UnityEngine.Random.Range(0, connectors.Count);

            Building culpritPipe = (Building)connectors[pipeIndex].parent;
            Map map = culpritPipe.Map;

            float slurryPercentageToRemove = 0.25f;
            float sluryAmount = culpritNetwork.Stored;
            float maxExplosionRadius = 5f;
            float unitsSlurryOneRadiusIncrease = 20;
            float explosionRadius = Mathf.Min(sluryAmount / unitsSlurryOneRadiusIncrease, maxExplosionRadius); //An increase of one per 10 slurry stored.

            string text = "LetterTextMutagenicLeak".Translate();
            StringBuilder stringBuilder = new StringBuilder(text);

            if (explosionRadius >= 0.9*maxExplosionRadius) //Hello Zap or Iron! Feel free to change the value/relation. And to remove this message, unless you want to do archeology in one year!
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.Append("MutagenicLeakWasLarge".Translate());
            }

            foreach (ISlurryNetStorage storage in culpritNetwork.StorageComps)
                storage.Storage = (1- slurryPercentageToRemove) * storage.Storage;

            GenExplosion.DoExplosion(culpritPipe.Position, map, explosionRadius, PMDamageDefOf.MutagenCloud, null,-1,-1,null,null,null,null,PMThingDefOf.PM_Filth_Slurry,0.5f,1);
            Find.LetterStack.ReceiveLetter("LetterLabelMutagenicLeak".Translate(), stringBuilder.ToString(), LetterDefOf.NegativeEvent, new TargetInfo(culpritPipe.Position, map));
        }

    }
}
