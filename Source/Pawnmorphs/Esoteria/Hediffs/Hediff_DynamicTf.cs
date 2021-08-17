using System.Linq;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// A TF hediff type where the morph type can be changed dynamically
    /// </summary>
    /// <seealso cref="Verse.Hediff" />
    public class Hediff_DynamicTf : MorphTf
    {
        private MorphDef morphDef;

        /// <summary>
        /// The morph def associated with this hediff, which controls the mutations
        /// and transformation it can cause.
        /// </summary>
        /// <value>
        /// The MorphDef
        /// </value>
        public MorphDef MorphDef
        {
            get {
                if (morphDef == null)
                {
                    // Pick a random one if this hediff doesn't have one defined
                    morphDef = DefDatabase<MorphDef>.AllDefs.Where(m => !m.Restricted).RandomElement();
                    ResetMutationCaches();
                }
                return morphDef;
            }

            set
            {
                morphDef = value;
                ResetMutationCaches();
            }
        }

        /// <summary>
        /// Gets the severity label.
        /// </summary>
        /// <value>
        /// The severity label.
        /// </value>
        public override string SeverityLabel
        {
            get
            {
                if (def.maxSeverity <= 0) return null;
                return (Severity / def.maxSeverity).ToStringPercent();
            }
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref morphDef, nameof(morphDef));
            base.ExposeData();
        }
    }
}