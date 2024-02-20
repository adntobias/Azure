# Define the parameter for the webhook data
Param(
    [parameter (Mandatory=$false)]
    [object] $WebhookData
)

# Convert the webhook request body to JSON
$RequestBody = $WebhookData.RequestBody | ConvertFrom-Json
$Data = $RequestBody.data

# Disable Azure context autosave for the current process
Disable-AzContextAutosave -Scope Process

# Connect to the Azure account and set the context to the current subscription
$ctx = (Connect-AzAccount -Identity).Context
Set-AzContext -Subscription $ctx.Subscription

# Get the creator, resource ID, and current date
$createdBy = $Data.claims.name
$resourceID = $Data.resourceUri
$now = Get-Date -Format "dd/MM/yyyy"

# Create a hashtable with the tag information
$tagObj = @{
    "Creator"=$createdBy
    "CreatedAt"=$now
}

try {
    # Update the tags for the specified resource ID
    Write-Output "update tags now for id $($resourceID)"
    Update-AzTag -ResourceId $resourceID -Operation Merge -Tag $tagObj
}
catch {
    # Handle any exceptions that occur during tag update
    Write-Error $"Exception while writing Tags $($_.Exception.Message)"
}
