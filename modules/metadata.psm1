function Get-Metadata {
    [CmdletBinding()]
    param()
    $ScriptVersion = "3.0"
    $ScriptCreatedBy = "HuskyNZ"
    $ScriptLastUpdatedOn = "22/07/2025"
    $envExample = "env.example"
    $envFile = ".env"

    return @{
        ScriptVersion = $ScriptVersion
        ScriptCreatedBy = $ScriptCreatedBy
        ScriptLastUpdatedOn = $ScriptLastUpdatedOn
        envExample = $envExample
        envFile = $envFile
    }
}

Export-ModuleMember -Function Get-Metadata