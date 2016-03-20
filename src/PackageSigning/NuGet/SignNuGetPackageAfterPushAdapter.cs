using System;
using System.ComponentModel;
using System.Linq;
using Inedo.ProGet.Extensibility.Adapters;
using Inedo.ProGet.Feeds;

namespace ProGet.PackageSigning.Extension.NuGet
{
    /// <summary>   Used to automatically sign NuGet pacakges after they are pushed to the feed. </summary>
    [DisplayName( "Sign NuGet packages after pushed" )]
    [Description( "Automatically signs NuGet packages after they are pushed to the feed." )]
    public class SignNuGetPackageAfterPushAdapter : SignPackageAfterPushAdapter
    {
        #region Overrides of SignPackageAfterPushAdapter

        /// <summary>   Creates the package signer. </summary>
        /// <param name="packageInfo">  Information about the package to sign.</param>
        /// <returns>   The new package signer. </returns>
        protected internal override IPackageSigner CreatePackageSigner( PackageAdapterContext packageInfo )
        {
            return new NuGetPackageSigner( packageInfo.Feed.FeedName, this.DefaultKeyName, this.AdditionalKeyPaths );
        }
        /// <summary>   Returns a value indicating whether the given feed is supported. </summary>
        /// <param name="feed"> The feed.</param>
        /// <returns>   true if supported, false if not. </returns>
        protected internal override bool IsFeedSupported( Feed feed )
        {
            return feed.FeedType == FeedType.NuGet;
        }

        #endregion
    }
}