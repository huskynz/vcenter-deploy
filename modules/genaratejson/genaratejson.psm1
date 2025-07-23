function Write-Json {

    # Import required Modules
    Import-Module (Join-Path -Path $env:PROJECT_ROOT -ChildPath "modules\config\config.psm1")
    Import-Module (Join-Path -Path $env:PROJECT_ROOT -ChildPath "modules\environment\environment.psm1")
    # Load environment variables
    $config = Get-EnvironmentVariables

        # Generate JSON config
    if (Test-Path (Join-Path -Path $env:PROJECT_ROOT -ChildPath "vcenter-deploy.json")) {
        Remove-Item (Join-Path -Path $env:PROJECT_ROOT -ChildPath "vcenter-deploy.json") -Force
    }
    $tempJsonPath = New-VcenterJsonConfig -config $config
    Write-Log "[+] Generated JSON config file at $tempJsonPath" "Success"

}


Export-ModuleMember -Function Write-Json