# Validate required environment variables
function Import-EnvCheck {

    # Import required Modules
    Import-Module (Join-Path -Path $env:PROJECT_ROOT -ChildPath "modules\config\config.psm1")
    Import-Module (Join-Path -Path $env:PROJECT_ROOT -ChildPath "modules\environment\environment.psm1")
    # Load environment variables
    $config = Get-EnvironmentVariables

    $requiredVariables = @(
        'VCSADeployCLI',
        'ESXiHost',
        'ESXiUser',
        'ESXiPassword',
        'VCSAName',
        'VCPassword',
        'VCSARootPass',
        'IPAddress',
        'Gateway',
        'DnsServers',
        'NetworkPrefix',
        'Datastore',
        'VmName'
    )

    foreach ($variable in $requiredVariables) {
        if (-not $config[$variable]) {
            Write-Log "[ERROR] Required environment variable '$variable' is not set in the .env file. Exiting." "Error"
            exit 1
        }
    }
}

Export-ModuleMember -Function Import-EnvCheck
