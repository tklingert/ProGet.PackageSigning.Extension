using System;
using System.IO.Packaging;
using System.Linq;
using Signature.Core;

namespace ProGet.PackageSigning.Extension.NuGet.Internal
{
    /// <summary>   Wraps the <see cref="PackageSigner" /> and implements <see cref="INuGetPackageSignerCore" />. </summary>
    internal class NuGetPackageSignerCoreWrapper : INuGetPackageSignerCore
    {
        #region Public

        /// <summary>   Initializes a new instance of the PackageSignerWrapper class. </summary>
        public NuGetPackageSignerCoreWrapper( )
        {
            this.Signer = new PackageSigner( );
        }
        /// <summary>   Gets the signer. </summary>
        /// <value> The signer. </value>
        public PackageSigner Signer { get; }

        #endregion

        #region INuGetPackageSignerCore Members

        /// <summary>   Signs the given package. </summary>
        /// <param name="package">          The package.</param>
        /// <param name="keyFilePath">      Full pathname of the key file.</param>
        /// <param name="keyFilePassword">  The key file password.</param>
        /// <returns>   true if the package has been signed, false if not, e.g. if its already signed. </returns>
        public bool SignPackage( ZipPackage package, string keyFilePath, string keyFilePassword )
        {
            return this.Signer.SignPackage( package, keyFilePath, keyFilePassword, signedPackageId: null );
        }

        #endregion
    }
}