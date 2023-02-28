
param($eventGridEvent, $TriggerMetadata)

import-module "Az.Accounts"
import-module "Az.MSGraph"
import-module "Az.Authorization"

#$event = $eventGridEvent | ConvertTo-Json -Depth 2
$createdBy = $eventGridEvent.data.claims.name
$resourceID = $eventGridEvent.data.resourceUri
$now = Get-Date -Format "dd/MM/yyyy"

$tagObj = @{
    "Creator"=$createdBy
    "CreatedAt"=$now
}

try {
    Write-Host $"update tags now for id $($resourceID)"
    Update-AzTag -ResourceId $resourceID -Operation Merge -Tag $tagObj    
}
catch {
    Write-Error $"Exception while writing Tags $($_.Exception.Message)" 
}
