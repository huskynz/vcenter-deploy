# ===============================
# metadata.psm1
# ===============================
# Simple metadata module for vCenter deployment configuration

function Get-Metadata {
    return @{
        envExample = "env.example"
        envFile = ".env"
        version = "1.0"
        author = "HuskyNZ"
        description = "vCenter Deployment Automation"
    }
}

Export-ModuleMember -Function Get-Metadata