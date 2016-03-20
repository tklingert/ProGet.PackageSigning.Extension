[![Build status](https://ci.appveyor.com/api/projects/status/od6bnkdvpib2hrue/branch/master?svg=true)](https://ci.appveyor.com/project/tklingert/proget-packagesigning-extension/branch/master)
 
# ProGet - Package Signing Extension

The ProGet extensions integrates automatic NuGet package signing/strong naming into ProGet feeds via [feed adapters](http://inedo.com/support/sdk-reference/proget/Inedo.ProGet.Extensibility.Adapters).

The project uses and combines the of the excellent work of:

* [Werner van Deventer](https://twitter.com/brutaldev) described in his blog post [".NET Assembly Strong-Name Signer"](http://brutaldev.com/post/2013/10/18/NET-Assembly-Strong-Name-Signer).
* [Rory Plaire](https://github.com/myget/webhooks-sign-package/) webhooks-sign-package

## Currently provided feed adapters

### SignNuGetPackageAfterPushAdapter

As the name implies, it signs packages after they are pushed to the feed.

#### Configuration

See [ProGet - Tutorial - Step 3](http://inedo.com/support/tutorials/extending-proget-package-store)

Adapter Configuration XML:

```xml
<ProGet.PackageSigning.Extension.NuGet.SignNuGetPackageAfterPushAdapter Assembly="ProGet.PackageSigning.Extension">
    <Properties />
</ProGet.PackageSigning.Extension.NuGet.SignNuGetPackageAfterPushAdapter>
```

or

```xml
<ProGet.PackageSigning.Extension.NuGet.SignNuGetPackageAfterPushAdapter Assembly="ProGet.PackageSigning.Extension">
    <Properties AdditionalKeyPaths="{optional comma separated list of path where custom [snk|pfx] key files are located}" AdditionalKeyPaths="{optional specify the default key to use}" />
</ProGet.PackageSigning.Extension.NuGet.SignNuGetPackageAfterPushAdapter>
```

### SignNuGetPackageBeforeDownloadAdapter

As the name implies, it signs packages before they are downloaded from the feed.

#### Configuration

See [ProGet - Tutorial - Step 3](http://inedo.com/support/tutorials/extending-proget-package-store)

Adapter Configuration XML:

```xml
<ProGet.PackageSigning.Extension.NuGet.SignNuGetPackageBeforeDownloadAdapter Assembly="ProGet.PackageSigning.Extension">
    <Properties />
</ProGet.PackageSigning.Extension.NuGet.SignNuGetPackageBeforeDownloadAdapter>
```

or

```xml
<ProGet.PackageSigning.Extension.NuGet.SignNuGetPackageBeforeDownloadAdapter Assembly="ProGet.PackageSigning.Extension">
    <Properties AdditionalKeyPaths="{optional comma separated list of path where custom [snk|pfx] key files are located}" AdditionalKeyPaths="{optional specify the default key to use}" />
</ProGet.PackageSigning.Extension.NuGet.SignNuGetPackageBeforeDownloadAdapter>
```

## Known issues

ProGet doesn't seem to invoke the adapters reliable:

* PUSH
 * via NuGet commandline: **YES**
 * via Web upload from disk: **NO**
 * via Bulk Import from disk: **NO**
* DOWNLOAD
 * via NuGet install/pull: **NO**
 * via Web frontend download: **YES**