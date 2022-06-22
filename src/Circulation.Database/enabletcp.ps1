# Enable TCP protocol for SQL Express

Import-Module "sqlps"
$smo = 'Microsoft.SqlServer.Management.Smo.'  
$wmi = new-object ($smo + 'Wmi.ManagedComputer').  

# Enable the TCP protocol on the default instance.  
$uri = "ManagedComputer[@Name='$env:computername']/ServerInstance[@Name='SQLEXPRESS']/ServerProtocol[@Name='Tcp']"  
$Tcp = $wmi.GetSmoObject($uri)
if($Tcp.IsEnabled) {
  # Nothing to do if the TCP protocol is already enabled.
  exit 0
}
$wmi.GetSmoObject($uri + "/IPAddress[@Name='IPAll']").IPAddressProperties[1].Value = "1433"
$Tcp.IsEnabled = $true  
$Tcp.Alter()  
$Tcp

# Function for waiting until a sql service instance is in a certain state
function Wait-AMinuteOrUntilState($state)
{
	$i = 0;
	while($true)
	{
		Start-Sleep 6
		$i++
		# Refresh the cache.  
		$SqlServiceInstance.Refresh();
		if ($SqlServiceInstance.ServiceState -eq $state) { break }
		elseif ($i -ge 10) { throw "The SQL Server instance could not be put into $state" }
	}
}

# Get a reference to the default instance of the Database Engine.  
$SqlServiceInstance = $wmi.Services['MSSQL$SQLEXPRESS']
# Display the state of the service.  
$SqlServiceInstance.ServiceState

# Stop the service.  
$SqlServiceInstance.Stop();

# Wait until the service has time to stop.
Wait-AMinuteOrUntilState("Stopped")
# Display the state of the service.  
$SqlServiceInstance.ServiceState

# Start the service again.  
$SqlServiceInstance.Start();

# Wait until the service has time to start.
Wait-AMinuteOrUntilState("Running")
# Display the state of the service.  
$SqlServiceInstance.ServiceState
