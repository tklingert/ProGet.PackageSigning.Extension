﻿using System;
using System.IO.Packaging;
using System.Linq;
using Signature.Core;

namespace ProGet.PackageSigning.Extension.NuGet.Internal
{
    /// <summary>   Used to encapsulate the external <see cref="PackageSigner" /> for testing purposes. </summary>
    internal interface INuGetPackageSignerCore
    {
        /// <summary>   Signs the given package. </summary>
        /// <param name="package">          The package.</param>
        /// <param name="keyFilePath">      Full pathname of the key file.</param>
        /// <param name="keyFilePassword">  The key file password.</param>
        /// <returns>   true if the package has been signed, false if not, e.g. if its already signed. </returns>
        bool SignPackage( ZipPackage package, string keyFilePath, string keyFilePassword );
    }
}