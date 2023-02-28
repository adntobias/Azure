$date = (Get-Date).AddMonths(-1) # date one month ago
$csvFile = "InactiveApps.csv"

# Connect to Azure
Connect-AzureAD

# Create an array to store inactive apps
$inactiveApps = @()

# Get all Enterprise Applications in Azure AD
$enterpriseApps = Get-AzureADServicePrincipal

# Iterate through each enterprise application
foreach ($app in $enterpriseApps) {
    $appName = $app.DisplayName
    $signInLogs = Get-AzureADAuditSignInLogs -Filter "appId eq '$($app.AppId)'" -Top 1
    if ($signInLogs) {
        $lastSignIn = $signInLogs[0].createdDateTime
    }
    else {
        $lastSignIn = $app.CreatedDateTime
    }
    if ($lastSignIn -le $date) {
        # App has not been accessed for a month or longer
        # Add the app to the inactive apps array
        $creator = get-AzureADServicePrincipalOwner -ObjectId $app.ObjectId
        $inactiveApps += New-Object PSObject -Property @{
            'Name'             = $app.DisplayName
            'AppId'            = $app.AppId
            'Created Date'     = $app.CreatedDateTime
            'Last SignIn Date' = $lastSignIn
            'Creator Name'     = $creator.DisplayName
            'Creator Email'    = $creator.UserPrincipalName
            'Type'             = 'Enterprise Application'
        }
    }
}

# Get all App registrations in Azure AD
$appRegistrations = Get-AzureADApplication

# Iterate through each app registration
foreach ($app in $appRegistrations) {
    $appName = $app.DisplayName
    $signInLogs = Get-AzureADAuditSignInLogs -Filter "appId eq '$($app.AppId)'" -Top 1
    if ($signInLogs) {
        $lastSignIn = $signInLogs[0].createdDateTime
    }
    else {
        $lastSignIn = $app.CreatedDateTime
    }
    $secret = Get-AzureADApplicationPasswordCredential -ObjectId $app.ObjectId
    if ($secret) {
        $secretExp = $secret.EndDate
    }
    if (($lastSignIn -le $date) -or ($secretExp -le (Get-Date))) {
        # App has not been accessed for a month or longer or the secret has expired
        # Add the app to the inactive apps array
        $creator = Get-AzureADApplicationOwner -ObjectId $app.ObjectId
        $inactiveApps += New-Object PSObject -Property @{
            'Name'             = $app.DisplayName
            'AppId'            = $app.AppId
            'Created Date'     = $app.CreatedDateTime
            'Last SignIn Date' = $lastSignIn
            'Secret Expired'   = $secretExp
            'Creator Name'     = $creator.DisplayName
            'Creator Email'    = $creator.UserPrincipalName
            'Type'             = 'App Registration'
        }
    }
}
#Export the inactive apps array to a CSV file
$inactiveApps | Export-Csv $csvFile -NoTypeInformation -Encoding UTF8 -Delimiter ","
Write-Output "Inactive App details exported to $csvFile"
