param (
    [Parameter(Mandatory=$false)]
    [string] $Tag = "Environment",
    [Parameter(Mandatory=$false)]
    [string] $TagValue = "Workshop"
)

Disable-AzContextAutosave -Scope Process

$ctx = (Connect-AzAccount -Identity).Context
Set-AzContext -Subscription $ctx.Subscription

function Shutdown-AzVM {
    param (
        [Alias("These")]
        $vms
    )

    foreach ($vm in $vms) {
        Write-Output "Trying to shut down VM: $($vm.Name)"

        try {
            $vm | Stop-AzVM -Force -NoWait | Out-Null
        }
        catch {
            Write-Error "Exception shutting down vm - $($vm.Name): $($_.Exception.Message)"
        }
    }
}

try {
    $vms = Get-AzVM | where {$_.Tags[$Tag] -eq $TagValue}
}
catch {
    Write-Error $"Exception while getting VMs: $($_.Exception.Message)"
}

Shutdown-AzVM -These $vms