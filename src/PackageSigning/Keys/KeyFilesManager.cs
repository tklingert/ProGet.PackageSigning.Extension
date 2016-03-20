using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Inedo.IO;

namespace ProGet.PackageSigning.Extension.Keys
{
    /// <summary>   <see cref="IKeyFilesManager" />. </summary>
    public class KeyFilesManager : IKeyFilesManager
    {
        #region Static

        #region - Public

        /// <summary>   The default file extensions. </summary>
        public static readonly string[] DefaultFileExtensions = { ".snk", ".pfx" };

        #endregion

        #endregion

        #region Private

        /// <summary>   Returns all key file search paths. </summary>
        /// <returns>   The path collection. </returns>
        private IEnumerable<string> GetSearchPaths( )
        {
            yield return this.DefaultPath;
            if ( this.AdditionalPaths.Count == 0 ) yield break;

            foreach ( var additionalPath in this.AdditionalPaths )
                yield return additionalPath;
        }

        #endregion

        #region Public

        /// <summary>   Initializes a new instance of the KeyFilesManager class. </summary>
        public KeyFilesManager( )
            : this( PathEx.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly( ).Location ) ) ) { }
        /// <summary>   Initializes a new instance of the KeyFilesManager class. </summary>
        /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or illegal values.</exception>
        /// <param name="defaultPath">      The default path.</param>
        public KeyFilesManager( string defaultPath )
        {
            if ( string.IsNullOrWhiteSpace( defaultPath ) ) throw new ArgumentException( "Argument is null or whitespace", nameof( defaultPath ) );

            this.DefaultPath = defaultPath;
            this.AdditionalPaths = new List<string>( );
            this.FileExtensions = new List<string>( DefaultFileExtensions );
        }
        /// <summary>   Gets the default path of the directory which contains the default key files provided. </summary>
        /// <value> The path. </value>
        public string DefaultPath { get; }

        #endregion

        #region IKeyFilesManager Members

        /// <summary>   Gets the path to the the default keyfile. </summary>
        /// <returns>   The path. </returns>
        public string GetDefaultKeyFilePath( )
        {
            return this.GetKeyFilePath( "DefaultKey" );
        }
        /// <summary>   Gets the password for the given key file, if available. </summary>
        /// <param name="keyFilePath">  The full path to the key file to get the password for.</param>
        /// <returns>   The password or an empty string. </returns>
        public string GetKeyFilePassword( string keyFilePath )
        {
            if ( string.IsNullOrWhiteSpace( keyFilePath ) ) throw new ArgumentException( "Argument is null or whitespace", nameof( keyFilePath ) );
            if ( !FileEx.Exists( keyFilePath ) )
                throw new FileNotFoundException( $"The key file [{keyFilePath}] does not exist !", keyFilePath );

            var pwPath = Path.ChangeExtension( keyFilePath, ".pw" );
            if ( !FileEx.Exists( pwPath ) )
                return string.Empty;

            using ( var file = FileEx.Open( pwPath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            using ( var reader = new StreamReader( file ) )
                return reader.ReadToEnd( );
        }
        /// <summary>   Gets the corresponding path for the given key file. </summary>
        /// <exception cref="ArgumentException">        Thrown when one or more arguments have unsupported or illegal values.</exception>
        /// <exception cref="FileNotFoundException">    Thrown when the requested file is not present.</exception>
        /// <param name="name"> The name.</param>
        /// <returns>   The path. </returns>
        public string GetKeyFilePath( string name )
        {
            if ( string.IsNullOrWhiteSpace( name ) ) throw new ArgumentException( "Argument is null or whitespace", nameof( name ) );

            string keyFilePath;
            if ( this.TryGetKeyFilePath( name, out keyFilePath ) )
                return keyFilePath;

            var attemptedPaths = string.Join( "\n\t- ", this.GetSearchPaths( ) );
            var attemptedFileExtensions = string.Join( "\n\t- ", this.FileExtensions );

            var msg = $"A key named [{name}] could not be located !\nAttempted paths:\n\t- {attemptedPaths}\nAttempted file extensions:\n\t- {attemptedFileExtensions}";
            throw new FileNotFoundException( msg );
        }
        /// <summary>   Attempts to get the corresponding path for the given key name. </summary>
        /// <exception cref="ArgumentException">        Thrown when one or more arguments have unsupported or illegal values.</exception>
        /// <exception cref="FileNotFoundException">    Thrown when the requested file is not present.</exception>
        /// <param name="name">         The name to search for.</param>
        /// <param name="keyFilePath">  The full path to the key file.</param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        public bool TryGetKeyFilePath( string name, out string keyFilePath )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                keyFilePath = null;
                return false;
            }
            var possibleKeyFiles = this.GetSearchPaths( )
                                       .SelectMany( path => this.FileExtensions.Select( ext => PathEx.Combine( path, $"{name}{ext}" ) ) );

            keyFilePath = possibleKeyFiles.FirstOrDefault( FileEx.Exists );
            return !string.IsNullOrWhiteSpace( keyFilePath );
        }

        /// <summary>   Gets a list of paths which should be searched for additional key files. </summary>
        /// <value> The additional paths. </value>
        public IList<string> AdditionalPaths { get; }
        /// <summary>   Gets the known file extensions to consider. </summary>
        /// <value> The file extensions. </value>
        public IList<string> FileExtensions { get; }

        #endregion
    }
}