@{
    db = @{
        name = "LocalCirculation"
        hostName = "localhost"
        port = "1433"
        # table names that should not available for a database name because it conflicts with a 
        # linked server in the Connecture database
        forbiddenDbNames = @()
        # This value will instruct the New-Migration function whether new scripts will be squashed. 
        # For more information see documentation for Invoke-CISquashMigrationVersion
        # http://kb.extendhealth.com/display/PD/PowerShell+Migrator+Modules
        willNewMigrationScriptsBeSquashed = $false
    }
    paths = @{
        snapshot = "snapshot"
        bin = "bin"
        migrations = "Migrations"
        unitTests = "Testing"
        databaseSeedData = "SeedData"
        init = "Init"
    }
    DbScripts = @{
        databaseInitialization = "Init\\Initialization.sql"
    }
    Flyway = @{
        # Comma-separated case-sensitive list of schemas managed by Flyway.
        # The first schema in the list will be automatically set as the default one 
        # during the migration. It will also be the one containing the schema history table.
        managedSchema = "dbo"
    }
}
