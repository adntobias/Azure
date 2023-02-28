$date = (Get-Date).AddMonths(-1) # date one month ago
$csvFile = "InactiveUsers.csv"

# Connect to Azure AD
Connect-AzAccount

# Get all user accounts in Azure AD
$users = Get-AzADUser

# Create an array to store inactive users
$inactiveUsers = @()

# Iterate through each user account and check their last sign-in date
foreach ($user in $users) {
    $userName = $user.UserPrincipalName
    $lastSignIn = $user.LastSignInDateTime
    if ($lastSignIn -le $date) {
        # User has not signed in for a month or longer
        # Add the user to the inactive users array
        $isGuest = $user.AccountEnabled -and $user.UserType -eq 'Guest'
        $inactiveUsers += New-Object PSObject -Property @{
            'Name' = $user.DisplayName
            'Email' = $user.UserPrincipalName
            'Last Sign-In' = $user.LastSignInDateTime
            'IsGuestUser' = $isGuest
        }
    }
}

# Export the inactive users array to a CSV file
$inactiveUsers | Export-Csv $csvFile -Encoding 'utf8' -Delimiter ","

Write-Output "Inactive user account details exported to $csvFile"
