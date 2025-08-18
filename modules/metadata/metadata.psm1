# Metadata module for vcenter-deploy
function Get-Metadata {
    return @{
        envExample = "env.example"
        envFile = ".env"
    }
}

Export-ModuleMember -Function Get-Metadata
