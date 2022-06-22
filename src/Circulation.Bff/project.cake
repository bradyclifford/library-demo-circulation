/*
 This cake script is not meant to be used stand-alone. It is included into the repository's top-level cake script
*/
SharedTask("ci-test")
    .Does(() => Information("TODO: run tests on the CI server."));

SharedTask("package")
    .Does(() =>
    {
        Information("Packaging Circulation.Bff");
        DotNetCorePublish("./src/Circulation.Bff/Circulation.Bff.csproj", new DotNetCorePublishSettings {
            Configuration = configuration,
            NoBuild = true
        });
        PackageForOcto("./src/Circulation.Bff/bin/Release/netcoreapp2.1/publish", "Circulation.Bff");
    });

SharedTask("publish-release-artifacts")
    .Does(() => 
    { 
        PublishReleaseArtifacts("./artifacts/Circulation.Bff.*.nupkg", "Circulation.Bff"); 
    });

SharedTask("create-octo-release")
    .Does(() =>
    {
        CreateOctoRelease("Circulation BFF");
    });
