#tool "nuget:?package=OctopusTools&version=6.8.2"
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"
#addin "nuget:?package=Cake.Git&version=0.19.0"
#addin "nuget:?package=NuGet.Core&version=2.14.0"
#addin "nuget:?package=Cake.ExtendedNuGet&version=1.0.0.27"

const string RELEASE_VERSION = "RELEASE_VERSION";
const string BRANCH_NAME = "BRANCH_NAME";
const string COMMITBOT_CRED = "COMMITBOT_CRED";
const string BOT_USERNAME = "svc-commitbot";
const string OCTO_RELEASE = "OCTO_RELEASE";
const string OCTOPUS_URL = "OCTOPUS_URL";

var branchInfo = GitBranchCurrent("./");
var branchName = branchInfo.FriendlyName;
System.Environment.SetEnvironmentVariable(BRANCH_NAME, branchName, System.EnvironmentVariableTarget.Process);
Information("BranchName is: {0}", EnvironmentVariable(BRANCH_NAME));
var isMasterBranch = branchName == "master";
var nugetPublishDestination = isMasterBranch ? EnvironmentVariable("NUGET_MASTER_URL") : EnvironmentVariable("NUGET_BRANCH_URL");

//TODO:  Is there a way we can get this without hardcoding?  This returns it in this format git@github.mktp.io:mische/library-demo-circulation.git
//var repoRemoteUrl = branchInfo.Remotes[0].Url;
var repoRemoteUrl = "https://github.mktp.io/mische/library-demo-circulation";

Task("set-release-version")
    .Description("Determine the version of the build, artifact, and release")
    .Does(() =>
    {
        if(BuildSystem.IsLocalBuild)
        {
            System.Environment.SetEnvironmentVariable(RELEASE_VERSION, "0.0.1-local." + DateTime.Now.ToString("yyyyMMddHHmmss"), System.EnvironmentVariableTarget.Process);
            return;
        }

        var version = GitVersion(new GitVersionSettings
        {
            UpdateAssemblyInfo = false
        });

        //Artifactory does not handle full semver, so we are rolling our own that meets all of our needs and still works with Artifactory
        var packageVersion = version.MajorMinorPatch;
        if (!isMasterBranch)
        {
            var process = StartAndReturnProcess("git", new ProcessSettings
            {
                RedirectStandardOutput = true,
                Arguments = "rev-parse --short " + version.Sha
            });
            process.WaitForExit();
            var shortSha = process.GetStandardOutput().First();

            packageVersion += "-" + version.PreReleaseLabel + "-" + shortSha;
        }

        System.Environment.SetEnvironmentVariable(RELEASE_VERSION, packageVersion, System.EnvironmentVariableTarget.Process);
		Information("Set-Version Release version: {0}", EnvironmentVariable(RELEASE_VERSION));

        Information("##teamcity[buildNumber '{0}']", EnvironmentVariable(RELEASE_VERSION));

    });

Task("build-solution")
    .Does(() =>
    {
        var dotNetCoreBuildSettings = new DotNetCoreBuildSettings
        {
          Configuration = configuration,
          //TODO: allow the verbosity to be specified
          Verbosity = DotNetCoreVerbosity.Minimal
        };
        var solutionFile = GetFiles("*.sln");
        DotNetCoreBuild(solutionFile.Single().ToString(), dotNetCoreBuildSettings);
    })
    ;

Task("clean-solution")
    .Does(() =>
    {
        var dotNetCoreCleanSettings = new DotNetCoreCleanSettings
        {
          Configuration = configuration,
          //TODO: allow the verbosity to be specified
          Verbosity = DotNetCoreVerbosity.Minimal
        };
        var solutionFile = GetFiles("*.sln");
        DotNetCoreClean(solutionFile.Single().ToString(), dotNetCoreCleanSettings);
    })
    ;

Task("push-git-tag")
    .WithCriteria(!BuildSystem.IsLocalBuild)
    .WithCriteria(isMasterBranch)
    .Does(() =>
    {
        var password = System.Net.WebUtility.UrlEncode(EnvironmentVariable(COMMITBOT_CRED));
        var uri = new Uri(repoRemoteUrl);
        var url = String.Format("{0}://{1}:{2}@{3}{4}.git", uri.Scheme, BOT_USERNAME, password, uri.Host, uri.PathAndQuery);
        var parameters = String.Format("push {0} {1}", url, EnvironmentVariable(RELEASE_VERSION));

           try
        {
            GitTag("./", EnvironmentVariable(RELEASE_VERSION));
            StartProcess("git", parameters);
        }
        catch (LibGit2Sharp.NameConflictException ex) //if the tag already exists, just move on
        {
            Warning(ex.Message);
        }
    });





Dictionary<string,CakeTaskBuilder> taskMap = new Dictionary<string,CakeTaskBuilder>();

CakeTaskBuilder SharedTask(string name) {
    Verbose("looking for task " + name + " in " + taskMap.GetHashCode());
    if(taskMap.TryGetValue(name, out var tsk))
    {
        Verbose("Found task");
        return tsk;
    }
    Verbose("Didn't find task");
    var toReturn = Task(name);
    taskMap.Add(name, toReturn);
    return toReturn;
}

void PackageForOcto(string srcDir, string packageName)
{
    var octopusSettings = new OctopusPackSettings
    {
        Version = EnvironmentVariable(RELEASE_VERSION),
        BasePath = srcDir,
        OutFolder = "./artifacts",
        Title = packageName
    };
    OctoPack(packageName, octopusSettings);
}

void PublishReleaseArtifacts(string artifactLocation, string projectTitle)
{
    var nugetPackageFiles = GetFiles(artifactLocation);
    foreach (var package in nugetPackageFiles)
    {
        if (IsNuGetPublished(package, nugetPublishDestination))
        {
            Warning("{0} has already been pushed.", package.GetFilename());
        }
        else
        {
            NuGetPush(package, new NuGetPushSettings
            {
                Source = nugetPublishDestination + "/" + projectTitle,
                ApiKey = EnvironmentVariable("NUGET_APIKEY")
            });
        }
    }
}

void CreateOctoRelease(string octoProjectName)
{
    var version = EnvironmentVariable(RELEASE_VERSION);
    var channel = EnvironmentVariable(BRANCH_NAME).Equals("master", StringComparison.OrdinalIgnoreCase) ? "Master" : "Branches";
    var releaseUrl = String.Format("{0}/releases/tag/", repoRemoteUrl);
    var releaseNotes = channel == "Master" ? String.Format("[{0}{1}]({0}{1})", releaseUrl, version) : null;

    var releaseSettings = new CreateReleaseSettings
    {
        Server = EnvironmentVariable(OCTOPUS_URL),
        ApiKey = EnvironmentVariable(OCTO_RELEASE),
        IgnoreSslErrors = true,
        DefaultPackageVersion = version,
        ReleaseNumber = version,
        Channel = channel,
        ReleaseNotes = releaseNotes,
        IgnoreExisting = true
    };

    OctoCreateRelease(octoProjectName, releaseSettings);
}
