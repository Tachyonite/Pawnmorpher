// ISapientVariantHolder.cs modified by Iron Wolf for Pawnmorph on 12/22/2019 9:11 AM
// last updated 12/22/2019  9:11 AM

namespace Pawnmorph.FormerHumans
{
    /// <summary>
    /// interface for something that holds variants of a type for  use with various sapience levels  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISapientVariantHolder<out T>
    {
        /// <summary>
        /// Gets the <see cref="T"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="T"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        T this[SapienceLevel key] { get; }
    }
}