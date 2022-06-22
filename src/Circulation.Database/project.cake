/*
 This cake script is not meant to be used stand-alone. It is included into the repository's top-level cake script
*/

SharedTask("build")
    .IsDependentOn("local-sql-build")
    ;
SharedTask("package")
    .Does(() =>
    {
        Information("Packaging Circulation.Database");
        PackageForOcto("./src/Circulation.Database/Migrations", "Circulation.Database");
    });

SharedTask("publish-release-artifacts")
    .Does(() => 
    { 
        PublishReleaseArtifacts("./artifacts/Circulation.Database.*.nupkg", "Circulation.Database"); 
    });

SharedTask("create-octo-release")
    .Does(() =>
    {
        CreateOctoRelease("Circulation Database");
    });

SharedTask("clean")
    .IsDependentOn("drop-local-sql")
    ;

string sqlHostName = null;
if(!BuildSystem.IsLocalBuild)
{
    sqlHostName = "localhost\\SQLEXPRESS";
}
if(HasArgument("sqlHost"))
{
    sqlHostName = Argument<string>("sqlHost");
}

Task("sql-dependencies")
    .Does(() =>
    {
        var prereqScriptExitCode = RunPowerShellScript(@".\src\Circulation.Database\ensure-db-tools-dependencies.ps1");
        if(prereqScriptExitCode != 0)
            throw new Exception("Failed to install database build prerequisites");

        if(!BuildSystem.IsLocalBuild)
        {
            // We don't want to do this on local builds because it requires an elevated session.

            // Enable tcp protocol support on MS SQL Server Express on the build agent.
            var tcpScriptExitCode = RunPowerShellScript(@".\src\Circulation.Database\enabletcp.ps1");
            if(tcpScriptExitCode != 0)
                throw new Exception("Unable to ensure TCP protocol is enabled on local SQL Server express");
        }
    }
    );

Task("drop-local-sql")
    .WithCriteria(IsRunningOnWindows())
    .IsDependentOn("sql-dependencies")
    .Does(() =>
    {
        var scriptArgs = new List<string>();
        if(sqlHostName != null)
        {
            // then we need to set the SQL Server address
            scriptArgs.Add("-HostName");
            scriptArgs.Add(sqlHostName);
        }

        var exitCode = RunPowerShellScript(@".\src\Circulation.Database\db-clean.ps1", scriptArgs);
        if(exitCode != 0)
            throw new Exception("Failed to drop the local database.");
    });

Task("local-sql-build")
    .WithCriteria(IsRunningOnWindows())
    .IsDependentOn("sql-dependencies")
    .Does(() =>
    {
        Information("Bringing local SQL Database up-to-date...");

        var scriptArgs = new List<string>();
        if(sqlHostName != null)
        {
            // then we need to set the SQL Server address
            scriptArgs.Add("-HostName");
            scriptArgs.Add(sqlHostName);
        }

        var exitCode = RunPowerShellScript(@".\src\Circulation.Database\db-build.ps1", scriptArgs);
        if(exitCode != 0)
            throw new Exception("Failed to setup the local database.");

    });


int RunPowerShellScript(string script, IEnumerable<string> scriptArgs = null)
{
    var settings = new ProcessSettings().WithArguments(args =>
    {
        args
            .Append("-File")
            .Append(script);

        foreach(string arg in scriptArgs ?? new string[0])
        {
            args.Append(arg);
        }
    });
    var exitCode = StartProcess("powershell", settings);
    return exitCode;
}
