using System;
using System.IO;
using System.Linq;

namespace ProGet.PackageSigning.Extension
{
    /// <summary>   Used to sign packages with a strong name. </summary>
    public interface IPackageSigner
    {
        /// <summary>   Attempts to sign the given package stream. </summary>
        /// <remarks>
        ///     The <paramref name="packageStream" /> needs to be in a valid state after this call:
        ///     <para>if not signed and <paramref name="packageStream" /> is seekable, it's position needs to be reset.</para>
        ///     <para>otherwise a new stream at position 0 needs to be provided.</para>
        /// </remarks>
        /// <param name="packageStream">    The package stream.</param>
        /// <returns>   true if it signed, false if not, e.g. if it already has been signed before. </returns>
        bool TrySignPackage( ref Stream packageStream );
    }
}