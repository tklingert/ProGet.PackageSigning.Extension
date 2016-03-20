using System;
using System.IO;
using System.Linq;
using Inedo.Diagnostics;
using Inedo.ProGet.Extensibility.Adapters;
using Inedo.ProGet.Feeds;
using Inedo.Serialization;

namespace ProGet.PackageSigning.Extension
{
    /// <summary>   Abstract base implementation for adapters used to automatically sign packages before they are downloaded from a feed. </summary>
    public abstract class SignPackageBeforeDownloadAdapter : BeforeDownloadPackageAdapter
    {
        #region Protected / Internal

        /// <summary>   Creates the package signer. </summary>
        /// <param name="packageInfo">  Information about the package to sign.</param>
        /// <returns>   The new package signer. </returns>
        protected internal abstract IPackageSigner CreatePackageSigner( PackageAdapterContext packageInfo );
        /// <summary>   Returns a value indicating whether the given feed is supported. </summary>
        /// <param name="feed"> The feed.</param>
        /// <returns>   true if supported, false if not. </returns>
        protected internal abstract bool IsFeedSupported( Feed feed );

        #endregion

        #region Public

        /// <summary>   Gets or sets the semicolon separated list of paths to look for additional key files. </summary>
        /// <value> The path. </value>
        [Persistent]
        public string AdditionalKeyPaths { get; set; }
        /// <summary>   Gets or sets the name of the default key to use for signing the package. </summary>
        /// <value> The name of the key or <c>null</c>. </value>
        [Persistent]
        public string DefaultKeyName { get; set; }

        #endregion

        #region Overrides of PackageAdapterBase

        /// <summary>
        ///     Returns a <see cref="T:System.IO.Stream" /> that contains the adapted output.
        /// </summary>
        /// <param name="packageInfo">Information about the package to adapt.</param>
        /// <returns>
        ///     <see cref="T:System.IO.Stream" /> that contains the adapted output.
        /// </returns>
        public sealed override Stream Adapt( PackageAdapterContext packageInfo )
        {
            if ( this.IsFeedSupported( packageInfo.Feed ) )
            {
                var packageSigner = this.CreatePackageSigner( packageInfo );
                var packageStream = packageInfo.GetPackageStream( takeOwnership: false );
                packageSigner.TrySignPackage( ref packageStream );
                return packageStream;
            }

            Logger.Warning( $"Skipping sign before download as the [{packageInfo.Feed.FeedName}|{packageInfo.Feed.FeedType}] is not supported by this signer [{this.GetType( ).Name}]." );
            return packageInfo.GetPackageStream( takeOwnership: false );
        }

        #endregion
    }
}