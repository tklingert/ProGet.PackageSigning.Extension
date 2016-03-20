using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProGet.PackageSigning.Extension.Keys;

namespace ProGet.PackageSigning.Extension.Tests.Keys
{
    /// <summary>   (Unit Test Class) containing the tests for the <see cref='KeyFilesManager' /> implementation. </summary>
    [TestClass]
    [DeploymentItem( @"..\..\TestData\Keys", "Keys" )]
    public class KeyFilesManagerTests
    {
        #region Helpers

        /// <summary>   Creates a testee. </summary>
        /// <param name="defaultPath">      The default path.</param>
        /// <returns>   The new testee. </returns>
        private KeyFilesManager CreateTestee( string defaultPath = null )
        {
            if ( defaultPath == null )
                defaultPath = TestKeysPath;
            return new KeyFilesManager( defaultPath );
        }
        /// <summary>   Gets the full pathname of the key test files. </summary>
        /// <value> The path. </value>
        protected static string TestKeysPath => Path.Combine( Environment.CurrentDirectory, "Keys" );

        #endregion

        [TestMethod]
        public void Ctor_InitializesManagerProperly_GivenNoParameters( )
        {
            var testee = new KeyFilesManager( );
            var expectedPath = Path.GetDirectoryName( typeof( KeyFilesManager ).Assembly.Location );

            testee.DefaultPath.Should( ).Be( expectedPath );
            testee.AdditionalPaths.Should( ).NotBeNull( );
            testee.FileExtensions.Should( ).BeEquivalentTo( KeyFilesManager.DefaultFileExtensions );
        }
        [TestMethod]
        public void GetDefaultKeyFilePath_ReturnsExpectedPath( )
        {
            var testee = this.CreateTestee( );

            var path = testee.GetDefaultKeyFilePath( );

            path.Should( ).NotBeNullOrWhiteSpace( );
            File.Exists( path ).Should( ).BeTrue( "the 'DefaultKey.snk' file should be been deployed and returned here." );
            path.Should( ).EndWith( "DefaultKey.snk" );
        }
        [TestMethod]
        public void GetKeyFilePassword_ReturnsEmptyString_GivenKeyFileWithoutPassword( )
        {
            var testee = this.CreateTestee( );

            var keyFile = testee.GetDefaultKeyFilePath( );

            var password = testee.GetKeyFilePassword( keyFile );

            password.Should( ).NotBeNull( ).And.BeEmpty( );
        }
        [TestMethod]
        public void GetKeyFilePassword_ReturnsPassword_GivenKeyFileWithPassword( )
        {
            var testee = this.CreateTestee( );

            var keyFile = testee.GetKeyFilePath( "FooKey" );
            var password = testee.GetKeyFilePassword( keyFile );

            password.Should( ).Be( "Bar" );
        }
        [TestMethod]
        public void GetKeyFilePassword_Throws_GivenInvalidName( )
        {
            var testee = this.CreateTestee( );

            Action act = ( ) => testee.GetKeyFilePassword( @"C:\Foo.file" );

            act.ShouldThrow<FileNotFoundException>( )
               .WithMessage( "*foo*" );
        }
        [TestMethod]
        public void GetKeyFilePath_ReturnsPath_GivenValidNameAndPfxFile( )
        {
            var testee = this.CreateTestee( );

            var path = testee.GetKeyFilePath( "AnotherKey" );

            path.Should( ).NotBeNullOrWhiteSpace( );
            File.Exists( path ).Should( ).BeTrue( "the 'AnotherKey.pfx' file should be been deployed and returned here." );
            path.Should( ).EndWith( "AnotherKey.pfx" );
        }
        [TestMethod]
        public void GetKeyFilePath_ReturnsPath_GivenValidNameAndSnkFile( )
        {
            var testee = this.CreateTestee( );

            var path = testee.GetKeyFilePath( "OtherKey" );

            path.Should( ).NotBeNullOrWhiteSpace( );
            File.Exists( path ).Should( ).BeTrue( "the 'OtherKey.snk' file should be been deployed and returned here." );
            path.Should( ).EndWith( "OtherKey.snk" );
        }
        [TestMethod]
        public void GetKeyFilePath_Throws_GivenInvalidName( )
        {
            var testee = this.CreateTestee( );

            Action act = ( ) => testee.GetKeyFilePath( "Foo" );

            act.ShouldThrow<FileNotFoundException>( )
               .WithMessage( "*foo*" );
        }
        [TestMethod]
        public void TryGetKeyFilePath_ReturnsFalse_GivenInvalidName( )
        {
            var testee = this.CreateTestee( );
            string dummy;

            var result = testee.TryGetKeyFilePath( "Foo", out dummy );

            result.Should( ).BeFalse( );
            dummy.Should( ).BeNull( );
        }
        [TestMethod]
        public void TryGetKeyFilePath_ReturnsTrueAndPath_GivenValidNameFromAdditionalPath( )
        {
            var testee = this.CreateTestee( defaultPath: Path.GetTempPath( ) );

            string path;
            var result = testee.TryGetKeyFilePath( "OtherKey", out path );
            result.Should( ).BeFalse( "the additional keys directory is not yet set" );

            testee.AdditionalPaths.Add( TestKeysPath );

            result = testee.TryGetKeyFilePath( "OtherKey", out path );
            result.Should( ).BeTrue( "the additional keys directory is now set" );
            path.Should( ).NotBeNullOrWhiteSpace( );
            File.Exists( path ).Should( ).BeTrue( "the 'OtherKey.snk' file should be been deployed and returned here." );
            path.Should( ).EndWith( "OtherKey.snk" );
        }
        [TestMethod]
        public void TryGetKeyFilePath_ReturnsTrueAndSetsPath_GivenValidName( )
        {
            var testee = this.CreateTestee( );

            string path;
            var result = testee.TryGetKeyFilePath( "OtherKey", out path );

            result.Should( ).BeTrue( );
            path.Should( ).NotBeEmpty( ).And.EndWith( "OtherKey.snk" );
            File.Exists( path ).Should( ).BeTrue( "the 'OtherKey.snk' file should be been deployed and returned here." );
        }
    }
}