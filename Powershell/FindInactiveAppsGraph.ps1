# Import the Microsoft Graph PowerShell module
Import-Module Microsoft.Graph

# Connect to Microsoft Graph
Connect-MgGraph -Scopes "Application.Read.All"

$date = (Get-Date).AddMonths(-1) # date one month ago
$csvFile = "InactiveApps.csv"

# Create an array to store inactive apps
$inactiveApps = @()

# Get all applications from Azure AD
$applications = Get-MgApplication

# Iterate through each application
foreach ($app in $applications) {
    $appName = $app.displayName
    $signInLogs = Get-MgAuditSignInLogs -ApplicationId $app.appId
    if (!$signInLogs.value) {
        $lastSignIn = $app.createdDateTime
    }
    else {
        $lastSignIn = ($signInLogs.value | sort-object -property createdDateTime -descending | select-object -first 1).createdDateTime
    }
    if ($lastSignIn -le $date) {
        # App has not been accessed for a month or longer
        # Add the app to the inactive apps array
        $creator = Get-MgUser -Id $app.createdBy.user.id
        $inactiveApps += New-Object PSObject -Property @{
            'Name'          = $appName
            'AppId'         = $app.appId
            'Created'       = $app.createdDateTime
            'Creator'       = $creator.displayName
            'Creator Email' = $creator.mail
        }
    }
}
#Check if there are any inactive apps
if ($inactiveApps) {
    # Export the inactive apps to a CSV file
    $inactiveApps | Export-Csv -Path $csvFile -Encoding UTF8 -NoTypeInformation -Delimiter ","
    Write-Host "Inactive apps have been exported to $csvFile"
}
else {
    Write-Host "No inactive apps found."
}
