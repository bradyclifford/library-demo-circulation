# Building locally

Open Circulation.sln and build it, run `Build.ps1` or run `Build.sh`

## To also package the artifacts for deployment

Add `-t ci` to the command line above. 

I.e., run `Build.ps1 -t ci` or `Build.sh -t ci`.

## To also run the CI automated tests

Add `-test` to the command line.

I.e. run `Build.ps1 -test` or `Build.sh -test`

# Build on a build server

Run `Build.ps1 -t ci`

It will detect it's running on a build server and actually push artifacts and releases to Artifactory and Octopus.

It will also push a Git tag to GitHub if it's on the `master` branch.
