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
            get { return morphDef; }
            set
            {
                morphDef = value;
                ResetMutationCaches();
            }
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref morphDef, nameof(morphDef));
            base.ExposeData();
        }
    }
}