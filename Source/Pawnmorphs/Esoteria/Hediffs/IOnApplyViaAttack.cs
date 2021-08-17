using System;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Interaface that adds a callback to the hediff that will be called when it
    /// is applied via a mutagenic injury
    /// </summary>
    public interface IOnApplyViaAttack
    {
        /// <summary>
        /// A callback that's called when this hediff is added via an injury
        /// </summary>
        /// <param name="dinfo">the damage info of the attack that added the hediff</param>
        void OnApplyViaAttack(DamageInfo dinfo);
    }
}
