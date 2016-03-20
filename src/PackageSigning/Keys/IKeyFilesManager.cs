using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProGet.PackageSigning.Extension.Keys
{
    /// <summary>   Used to mange the key files. </summary>
    public interface IKeyFilesManager
    {
        /// <summary>   Gets the path to the the default keyfile. </summary>
        /// <returns>   The path. </returns>
        string GetDefaultKeyFilePath( );
        /// <summary>   Gets the password for the given key file, if available. </summary>
        /// <param name="keyFilePath">  The full path to the key file to get the password for.</param>
        /// <returns>   The password or an empty string. </returns>
        string GetKeyFilePassword( string keyFilePath );
        /// <summary>   Gets the corresponding path for the given key name. </summary>
        /// <exception cref="ArgumentException">        Thrown when one or more arguments have unsupported or illegal values.</exception>
        /// <exception cref="FileNotFoundException">    Thrown when the requested file is not present.</exception>
        /// <param name="name"> The name to search for.</param>
        /// <returns>   The path. </returns>
        string GetKeyFilePath( string name );
        /// <summary>   Attempts to get the corresponding path for the given key name. </summary>
        /// <exception cref="ArgumentException">        Thrown when one or more arguments have unsupported or illegal values.</exception>
        /// <exception cref="FileNotFoundException">    Thrown when the requested file is not present.</exception>
        /// <param name="name">         The name to search for.</param>
        /// <param name="keyFilePath">  The full path to the key file.</param>
        /// <returns>   true if it succeeds, false if it fails. </returns>
        bool TryGetKeyFilePath( string name, out string keyFilePath );
        /// <summary>   Gets a list of paths which should be searched for additional key files. </summary>
        /// <value> The additional paths. </value>
        IList<string> AdditionalPaths { get; }
        /// <summary>   Gets the known file extensions to consider. </summary>
        /// <value> The file extensions. </value>
        IList<string> FileExtensions { get; }
    }
}