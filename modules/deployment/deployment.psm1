function Start-VCSADeployment {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [string]$VCSADeployCLI,
        [Parameter(Mandatory=$true)]
        [string]$tempJsonPath
    )

    $deployScript = @"
`$ErrorActionPreference = 'Stop'
function Write-Log {
    param(
        [string]`$Message,
        [string]`$Level = 'Info'
    )
    `$isRedirected = [Console]::IsOutputRedirected
    switch (`$Level) {
        'Success' { `$color = 'Green' }
        'Warning' { `$color = 'Yellow' }
        'Error'   { `$color = 'Red' }
        default   { `$color = 'Cyan' }
    }
    if (-not `$isRedirected) {
        Write-Host `$Message -ForegroundColor `$color
    } else {
        if (`$Level -eq 'Error') {
            Write-Error `$Message
        } else {
            Write-Output `$Message
            [Console]::Out.Flush()
        }
    }
}
Write-Log '[+] Starting VCSA deployment...' 'Info'
& "$VCSADeployCLI" install "$tempJsonPath" --accept-eula --no-ssl-certificate-verification
`$exitCode = `$LASTEXITCODE
if (`$exitCode -eq 0) {
    Write-Log '[SUCCESS] VCSA deployment completed successfully.' 'Success'
} else {
    Write-Log '[ERROR] VCSA deployment failed.' 'Error'
}
exit `$exitCode
"@

    $tempDeployScriptPath = "$env:TEMP\vcsa-deploy-temp.ps1"
    $deployScript | Set-Content -Path $tempDeployScriptPath -Encoding UTF8

    $process = Start-Process powershell.exe -ArgumentList "-ExecutionPolicy Bypass -File `"$tempDeployScriptPath`"" -Wait -PassThru
    return $process.ExitCode
}

Export-ModuleMember -Function Start-VCSADeployment