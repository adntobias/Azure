#check if theres an attendee from the previous tenant.

# $NewDoamin = 'alexholmeset.onmicrosoft.com'

#Cancelation message
$CancelationMessage = "We are moving over to a new system, so this meeting will be canceled. You will receive new invite from our new domain."

#From line 150, you find where to update the Client ID, Tenant ID and App secret.

function GetStringBetweenTwoStrings($text) {

    #Regex pattern to compare two strings
    $pattern = "(?s)(?<=________________________________________________________________________________)(.*?)(?=________________________________________________________________________________)"

    #Perform the opperation
    $result = [regex]::Match($text, $pattern).value

    #Return result
    return $result

}


function Html-ToText {
    param([System.String] $html)

    # remove line breaks, replace with spaces
    $html = $html -replace "(`r|`n|`t)", " "
    # write-verbose "removed line breaks: `n`n$html`n"

    # remove invisible content
    @('head', 'style', 'script', 'object', 'embed', 'applet', 'noframes', 'noscript', 'noembed') | % {
        $html = $html -replace "<$_[^>]*?>.*?</$_>", ""
    }
    # write-verbose "removed invisible blocks: `n`n$html`n"

    # Condense extra whitespace
    $html = $html -replace "( )+", " "
    # write-verbose "condensed whitespace: `n`n$html`n"

    # Add line breaks
    @('div', 'p', 'blockquote', 'h[1-9]') | % { $html = $html -replace "</?$_[^>]*?>.*?</$_>", ("`n" + '$0' ) }
    # Add line breaks for self-closing tags
    @('div', 'p', 'blockquote', 'h[1-9]', 'br') | % { $html = $html -replace "<$_[^>]*?/>", ('$0' + "`n") }
    # write-verbose "added line breaks: `n`n$html`n"

    #strip tags
    $html = $html -replace "<[^>]*?>", ""
    # write-verbose "removed tags: `n`n$html`n"

    # replace common entities
    @(
        @("&amp;bull;", " * "),
        @("&amp;lsaquo;", "<"),
        @("&amp;rsaquo;", ">"),
        @("&amp;(rsquo|lsquo);", "'"),
        @("&amp;(quot|ldquo|rdquo);", '"'),
        @("&amp;trade;", "(tm)"),
        @("&amp;frasl;", "/"),
        @("&amp;(quot|#34|#034|#x22);", '"'),
        @('&amp;(amp|#38|#038|#x26);', "&amp;"),
        @("&amp;(lt|#60|#060|#x3c);", "<"),
        @("&amp;(gt|#62|#062|#x3e);", ">"),
        @('&amp;(copy|#169);', "(c)"),
        @("&amp;(reg|#174);", "(r)"),
        @("&amp;nbsp;", " "),
        @("&amp;(.{2,6});", "")
    ) | % { $html = $html -replace $_[0], $_[1] }
    # write-verbose "replaced entities: `n`n$html`n"

    return $html

}


function Get-MSGraphAppToken {
    <# .SYNOPSIS
Get an app based authentication token required for interacting with Microsoft Graph API
.PARAMETER TenantID
A tenant ID should be provided.

.PARAMETER ClientID
Application ID for an Azure AD application. Uses by default the Microsoft Intune PowerShell application ID.

.PARAMETER ClientSecret
Web application client secret.

.EXAMPLE
# Manually specify username and password to acquire an authentication token:
Get-MSGraphAppToken -TenantID $TenantID -ClientID $ClientID -ClientSecert = $ClientSecret
.NOTES
Author: Jan Ketil Skanke
Contact: @JankeSkanke
Created: 2020-15-03
Updated: 2020-15-03

Version history:
1.0.0 - (2020-03-15) Function created
#>
    [CmdletBinding()]
    param (
        [parameter(Mandatory = $true, HelpMessage = "Your Azure AD Directory ID should be provided")]
        [ValidateNotNullOrEmpty()]
        [string]$TenantID,
        [parameter(Mandatory = $true, HelpMessage = "Application ID for an Azure AD application")]
        [ValidateNotNullOrEmpty()]
        [string]$ClientID,
        [parameter(Mandatory = $true, HelpMessage = "Azure AD Application Client Secret.")]
        [ValidateNotNullOrEmpty()]
        [string]$ClientSecret
    )
    Process {
        $ErrorActionPreference = "Stop"

        # Construct URI
        $uri = "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token"
        # Construct Body
        $body = @{
            client_id     = $clientId
            scope         = "https://graph.microsoft.com/.default"
            client_secret = $clientSecret
            grant_type    = "client_credentials"
        }

        try {
            $MyTokenRequest = Invoke-WebRequest -Method Post -Uri $uri -ContentType "application/x-www-form-urlencoded" -Body $body -UseBasicParsing
            $MyToken = ($MyTokenRequest.Content | ConvertFrom-Json).access_token
            If (!$MyToken) {
                Write-Warning "Failed to get Graph API access token!"
                Exit 1
            }
            $MyHeader = @{"Authorization" = "Bearer $MyToken" }
        }
        catch [System.Exception] {
            Write-Warning "Failed to get Access Token, Error message: $($_.Exception.Message)"; break
        }
        return $MyHeader
    }
}


$NewTenantId = 'xxx'
$NewClientID = 'xxx'
$NewClientSecret = "xxx"
$global:NewHeader = Get-MSGraphAppToken -TenantID $NewTenantId -ClientID $NewClientID -ClientSecret $NewClientSecret

#Cancelation message
$CancelationMessage = "We are moving over to a new system, so this meeting will be canceled. You will receive new invite from our new domain."


#Gets all internal users in the new tenant.

$currentUri = "https://graph.microsoft.com/beta/users?`$filter=userType eq 'Member'"

$UsersNewTenant = while (-not [string]::IsNullOrEmpty($currentUri)) {

    # API Call
    Write-Host "`r`nQuerying $currentUri..." -ForegroundColor Yellow
    $apiCall = Invoke-WebRequest -Method "GET" -Uri $currentUri -ContentType "application/json" -Headers $global:NewHeader -ErrorAction Stop

    $nextLink = $null
    $currentUri = $null

    if ($apiCall.Content) {
        # Check if any data is left
        $nextLink = $apiCall.Content | ConvertFrom-Json | Select-Object '@odata.nextLink'
        $currentUri = $nextLink.'@odata.nextLink'

        $apiCall.Content | ConvertFrom-Json
    }

}


foreach ($UserNewTenant in $UsersNewTenant.value) {

    $UserNewTenantUPN = $UserNewTenant.userprincipalname
    #Gets all events for the current user in the new tenant.

    $currentUri = "https://graph.microsoft.com/beta/users/$UserNewTenantUPN/events"

    $NewTenantTeamsMeetingsBulk = while (-not [string]::IsNullOrEmpty($currentUri)) {

        # API Call
        Write-Host "`r`nQuerying $currentUri..." -ForegroundColor Yellow
        $apiCall = Invoke-WebRequest -Method "GET" -Uri $currentUri -ContentType "application/json" -Headers $global:NewHeader -ErrorAction Stop

        $nextLink = $null
        $currentUri = $null

        if ($apiCall.Content) {
            # Check if any data is left
            $nextLink = $apiCall.Content | ConvertFrom-Json | Select-Object '@odata.nextLink'
            $currentUri = $nextLink.'@odata.nextLink'

            $apiCall.Content | ConvertFrom-Json
        }
    }

    $NewTenantTeamsMeetings = $NewTenantTeamsMeetingsBulk.value | Where-Object { (get-date $($_.start).datetime -Format yyyy-MM-ddTHH:MM) -ge (get-date -Format yyyy-MM-ddTHH:MM) }
    # $NewTenantTeamsMeetingsSeriesPastStartDate = $NewTenantTeamsMeetingsBulk.value | Where-Object{(get-date $($_.start).datetime -Format yyyy-MM-ddTHH:MM) -lt (get-date -Format yyyy-MM-ddTHH:MM)} | Where-Object{$_.type -like "seriesMaster"}

    $Result = @()
    ForEach ($meeting in $NewTenantTeamsMeetings) {
        $Result += New-Object PSObject -property $([ordered]@{
                Subject        = $meeting.subject
                Organizer      = $meeting.organizer.emailAddress.name
                Attendees      = (($meeting.attendees | select -expand emailAddress) | Select -expand name) -join ','
                StareTime      = [DateTime]$meeting.start.dateTime
                EndTime        = [DateTime]$meeting.end.dateTime
                IsTeamsMeeting = ($meeting.onlineMeetingProvider -eq 'teamsForBusiness')
                Location       = $meeting.location.displayName
                IsCancelled    = $meeting.isCancelled
            })
    }

    $Result | Export-CSV "D:\CalendarEvents.CSV" -NoTypeInformation -Encoding UTF8 -Append
}