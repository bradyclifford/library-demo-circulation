/*
The overall build and publish process:
0. Clean Checkout
0. bootstrap cake (get dependencies)
1. Calculate version number from GIT history (and branch name and local vs. build server)
   * and Set Team City build version number (*)
2. dotnet build the solution file
3. Let the individual deployable projects build themselves if needed
4. Run CI tests (*)
5. Package the releasable artifacts (project-by-project)
6. Push a Git tag for this release (*) (**)
7. Push the releaseable artifacts to Artifactory (project-by-project) (*)
8. Create the Octopus release (project-by-project) (*)

Steps marked (*) only run on the build server
Steps marked (**) only run on the master branch
*/

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var runTests = HasArgument("test");

#load ./cake-scripts/globals.cake


Task("Default")
    .IsDependentOn("build")
    ;

Task("ci")
    .IsDependentOn("rebuild")
    .IsDependentOn("publish")
    ;

Task("rebuild")
    .IsDependentOn("clean")
    .IsDependentOn("build")
    ;

SharedTask("build")
    .IsDependentOn("set-release-version")
    .IsDependentOn("build-solution")
    // Projects that need to run their own build steps separate from the solution build do so in their respective project.cake files
    ;

SharedTask("package")
    .IsDependentOn("build")
    // The actual packaging happens in the project.cake files
    ;

SharedTask("ci-test")
    .WithCriteria(!BuildSystem.IsLocalBuild || runTests)
    .IsDependentOn("build")

    // The specific CI testing happens in the project.cake files
    ;

SharedTask("publish-release-artifacts")
    .WithCriteria(!BuildSystem.IsLocalBuild)
    // The actual publishing happens in the project.cake files
    ;

SharedTask("create-octo-release")
    .WithCriteria(!BuildSystem.IsLocalBuild)
    // The actual release creation happens in the project.cake files
    ;

SharedTask("publish")
    .IsDependentOn("ci-test")
    .IsDependentOn("package")
    .IsDependentOn("push-git-tag")
    .IsDependentOn("publish-release-artifacts")
    .IsDependentOn("create-octo-release")
    ;

SharedTask("clean")
    .IsDependentOn("clean-solution")
    .Does(() => 
    {
        // Now wipe out the artifacts directory
        if (DirectoryExists("./artifacts"))
        {
            CleanDirectory("./artifacts");
        }
    })
    ;

#load ./cake-scripts/projects-to-include.cake

RunTarget(target)
