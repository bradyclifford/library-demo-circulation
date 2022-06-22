# Update the path environment variable to include anything that was added to the registry since the parent process started. For instance, when the TeamCity agent process started, flyway was not installed, but later it got installed and modified the machine path.
$newPath = $env:Path + ";" + [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
$newPath = ($newPath -split ";" | Select-Object -Unique) -join ";"
$env:Path = $newPath
