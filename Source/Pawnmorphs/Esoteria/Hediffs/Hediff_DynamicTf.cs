using System.Linq;
using System.Text;
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

        /// <summary>
        /// Creates a debug string for this hediff 
        /// </summary>
        /// <returns></returns>
        public override string DebugString()
        {
            StringBuilder builder = new StringBuilder(base.DebugString());
            builder.AppendLine($"{nameof(Hediff_DynamicTf)}: ");
            builder.AppendLine($"  MorphDef: {MorphDef.defName}");

            return builder.ToString();
        }

        /// <summary>
        /// Save/Loads data.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Defs.Look(ref morphDef, nameof(morphDef));
            base.ExposeData();
        }
    }
}