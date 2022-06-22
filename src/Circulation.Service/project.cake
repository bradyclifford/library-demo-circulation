/*
 This cake script is not meant to be used stand-alone. It is included into the repository's top-level cake script
*/



SharedTask("package")
    .Does(() =>
    {
        Information("Packaging Circulation.Service");
        DotNetCorePublish("./src/Circulation.Service/Circulation.Service.csproj", new DotNetCorePublishSettings {
            Configuration = configuration,
            NoBuild = true
        });
        PackageForOcto("./src/Circulation.Service/bin/Release/netcoreapp2.1/publish", "Circulation.Service");
    });

SharedTask("publish-release-artifacts")
    .Does(() => 
    { 
        PublishReleaseArtifacts("./artifacts/Circulation.Service.*.nupkg", "Circulation.Service"); 
    });

SharedTask("create-octo-release").Does(() => { CreateOctoRelease("Circulation Service");});