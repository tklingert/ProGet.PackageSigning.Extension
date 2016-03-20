using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using Inedo.Diagnostics;
using Inedo.IO;
using NuGet;
using ProGet.PackageSigning.Extension.Keys;
using ProGet.PackageSigning.Extension.NuGet.Internal;
using ZipPackage = System.IO.Packaging.ZipPackage;

namespace ProGet.PackageSigning.Extension.NuGet
{
    /// <summary>   <see cref="IPackageSigner" />. </summary>
    public class NuGetPackageSigner : IPackageSigner
    {
        #region Static

        #region - Private

        /// <summary>   Attempts to get the NuGet package metadata from the given package. </summary>
        /// <param name="package">  The package.</param>
        /// <param name="metadata"> The package metadata.</param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        private static bool TryGetPackageMetadata( ZipPackage package, out ManifestMetadata metadata )
        {
            var packageRelationship = package.GetRelationshipsByType( "http://schemas.microsoft.com/packaging/2010/07/manifest" )
                                             .SingleOrDefault( );
            if ( packageRelationship == null )
            {
                metadata = null;
                return false;
            }
            var manifestPart = package.GetPart( packageRelationship.TargetUri );
            using ( var partStream = manifestPart.GetStream( ) )
            {
                var manifest = Manifest.ReadFrom( partStream, NullPropertyProvider.Instance, validateSchema: false );
                metadata = manifest.Metadata;
                return true;
            }
        }

        #endregion

        #endregion

        #region Private

        /// <summary>   Gets the key files manager for the current instance. </summary>
        /// <returns>   The key files manager. </returns>
        private IKeyFilesManager CreateKeyFilesManager( )
        {
            var manager = new KeyFilesManager( );

            // register additional paths to the manager
            if ( this.AdditionalKeyPaths != null && this.AdditionalKeyPaths.Length > 0 )
                Array.ForEach( this.AdditionalKeyPaths, manager.AdditionalPaths.Add );

            return manager;
        }

        /// <summary>   Splits the additional key paths. </summary>
        /// <param name="additionalKeyPaths">   The additional key paths to split.</param>
        /// <returns>   The path array. </returns>
        private string[] SplitAdditionalKeyPaths( string additionalKeyPaths )
        {
            if ( string.IsNullOrWhiteSpace( additionalKeyPaths ) )
                return new string[0];

            return additionalKeyPaths.Split( new[] { ";", ",", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries );
        }

        #endregion

        #region Protected / Internal

        /// <summary>   Gets the full path to the default key file which should be used to sign the package. </summary>
        /// <returns>   The key file path. </returns>
        protected internal virtual string GetDefaultKeyFilePath( )
        {
            var manager = this.KeyFilesManager;
            return string.IsNullOrWhiteSpace( this.DefaultKeyName ) ? manager.GetDefaultKeyFilePath( ) : manager.GetKeyFilePath( this.DefaultKeyName );
        }

        #endregion

        #region Internal

        /// <summary>   Initializes a new instance of the PackageSignerWrapper class. </summary>
        /// <exception cref="ArgumentNullException">    Thrown when one or more required arguments are null.</exception>
        /// <param name="core">                 The core.</param>
        /// <param name="feedName">             Name of the feed the package came from/is pushed to.</param>
        /// <param name="defaultKeyName">       The default key name.</param>
        /// <param name="additionalKeyPaths">   The additional key paths.</param>
        internal NuGetPackageSigner( INuGetPackageSignerCore core, string feedName, string defaultKeyName = null, string additionalKeyPaths = null )
        {
            if ( core == null ) throw new ArgumentNullException( nameof( core ) );
            this.Core = core;
            this.DefaultKeyName = defaultKeyName;
            this.AdditionalKeyPaths = this.SplitAdditionalKeyPaths( additionalKeyPaths );
            this.KeyFilesManager = this.CreateKeyFilesManager( );
            this.FeedName = feedName ?? "??";
        }
        /// <summary>   Gets the underlying core signer. </summary>
        /// <value> The core signer. </value>
        internal INuGetPackageSignerCore Core { get; }

        #endregion

        #region Public

        /// <summary>   Initializes a new instance of the PackageSignerWrapper class. </summary>
        /// <param name="feedName">             Name of the feed the package came from/is pushed to.</param>
        /// <param name="defaultKeyName">       The default key name.</param>
        /// <param name="additionalKeyPaths">   The additional key paths.</param>
        public NuGetPackageSigner( string feedName, string defaultKeyName = null, string additionalKeyPaths = null )
            : this( new NuGetPackageSignerCoreWrapper( ), feedName, defaultKeyName, additionalKeyPaths ) { }
        /// <summary>   Gets the additional key paths. </summary>
        /// <value> The additional key paths. </value>
        public string[] AdditionalKeyPaths { get; }
        /// <summary>   Gets the default key name. </summary>
        /// <value> The default key name or <c>null</c>. </value>
        public string DefaultKeyName { get; }
        /// <summary>   Gets the name of the feed this signer is intended for. </summary>
        /// <value> The name of the feed. </value>
        public string FeedName { get; }
        /// <summary>   Gets the manager for key files. </summary>
        /// <value> The key files manager. </value>
        public IKeyFilesManager KeyFilesManager { get; }

        #endregion

        #region IPackageSigner Members

        /// <summary>   Attempts to sign the given package stream. </summary>
        /// <param name="packageStream">    The package stream.</param>
        /// <returns>   true if it signed, false if not, e.g. if it already has been signed before. </returns>
        public virtual bool TrySignPackage( ref Stream packageStream )
        {
            var core = this.Core;
            var tempOutput = TemporaryStream.Create( packageStream.Length );
            var cleanupTempOutput = false;
            try
            {
                // copy package content to temporary output
                packageStream.CopyTo( tempOutput );
                // reset temporary output stream position
                tempOutput.Seek( 0, SeekOrigin.Begin );
                bool signed;
                using ( var package = ( ZipPackage )Package.Open( tempOutput, FileMode.Open, FileAccess.ReadWrite ) )
                {
                    ManifestMetadata metadata;
                    string keyFile;
                    if ( !TryGetPackageMetadata( package, out metadata )
                         || !this.KeyFilesManager.TryGetKeyFilePath( metadata.Id, out keyFile ) )
                        keyFile = this.GetDefaultKeyFilePath( );
                    var keyFilePassword = this.KeyFilesManager.GetKeyFilePassword( keyFile );

                    var packageName = metadata == null ? "??" : $"{metadata.Id}-{metadata.Version}";
                    var keyName = PathEx.GetFileName( keyFile );
                    Logger.Information( $"Signing NuGet package [{packageName} on feed [{this.FeedName}] with key file [{keyName}]..." );
                    signed = core.SignPackage( package, keyFile, keyFilePassword );
                    Logger.Information( signed ? $"Successfully signed NuGet package [{packageName} on feed [{this.FeedName}] with key file [{keyName}]." : $"Did not sign NuGet package [{packageName} on feed [{this.FeedName}], most likely its already signed." );
                    if ( !signed && packageStream.CanSeek )
                    {
                        // nothing changed => reset original stream position and return it.
                        packageStream.Seek( 0, SeekOrigin.Begin );
                        // mark temporary output stream for cleanup, since it's no longer necessary
                        cleanupTempOutput = true;
                        return false;
                    }
                }

                // reset temporary output stream position
                tempOutput.Seek( 0, SeekOrigin.Begin );
                packageStream = tempOutput;
                return signed;
            }
            finally
            {
                if ( cleanupTempOutput )
                    tempOutput.Dispose( );
            }
        }

        #endregion
    }
}