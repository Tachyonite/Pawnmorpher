// HediffCompBase.cs modified by Iron Wolf for Pawnmorph on 08/09/2019 9:05 AM
// last updated 08/09/2019  9:05 AM

using Verse;

namespace Pawnmorph.Utilities
{
    /// <summary> convenient base class for hediff comps that know their properties type. </summary>
    public class HediffCompBase<T> : HediffComp where T : HediffCompProperties
    {
        /// <summary>
        /// Gets the props.
        /// </summary>
        /// <value>
        /// The props.
        /// </value>
        public T Props => (T) props; 
    }

    /// <summary> Convenient base class for comp properties that know their comp type. </summary>
    public class HediffCompPropertiesBase<T> : HediffCompProperties where T: HediffComp
    {
        /// <summary>
        /// create a new instance of this type 
        /// </summary>
        public HediffCompPropertiesBase()
        {
            compClass = typeof(T); 
        }
    }
}