function Get-Metadata {
    return @{
        ScriptVersion = "4.0"
        ScriptCreatedBy = "HuskyNZ"
        ScriptLastUpdatedOn = "22/07/2025"
        envExample = "env.example"
        envFile = ".env"
    }
}

Export-ModuleMember -Function Get-Metadata