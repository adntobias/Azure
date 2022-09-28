# Bitte die Befehle nacheinander ausführen 
# MS Doku
# https://docs.microsoft.com/en-us/graph/add-properties-profilecard#add-a-custom-attribute
# https://docs.microsoft.com/en-us/graph/api/organizationsettings-post-profilecardproperties?view=graph-rest-beta&tabs=powershell
# https://docs.microsoft.com/en-us/graph/api/profilecardproperty-get?view=graph-rest-beta&tabs=powershell
# MVP Blog
# https://nanddeepnachanblogs.com/posts/2021-12-23-customize-profile-cards-graph-api/


Install-Module Microsoft.Graph.Identity.DirectoryManagement
#-oder falls bereits vorhanden-
#Import-Module Microsoft.Graph.Identity.DirectoryManagement

# Hier die Tenant ID eintragen (Azure AD -> Overview)
$organizationId = "<Tenant ID>"

# Einloggen eines Berechtigten Admins (MFA möglich)
# "User.ReadWrite.All" -> Graph API erwartet mind. diese Berechtigung
Connect-MgGraph -TenantId $organizationId -Scopes "User.ReadWrite.All" 

# Explizites wählen des beta-Endpunktes
Select-MgProfile -Name "beta"

# Erstellen des Requests, hier können die folgenden Werte angepasst werden:
#   DirectoryPropertyName -> Hier kann eins der 15 extensionAttributes ausgewählt werden
#   DisplayName -> Wie soll der Titel lauten (englisch bzw. default Einstellung) 
#   LanguageTag -> Übersetzung in DE
#   Localizations:Displayname -> Titel der angezeigt wird in DE
$params = @{
	DirectoryPropertyName = "CustomAttribute1" 
	Annotations = @(
		@{
			DisplayName = "Sales Area" 
			Localizations = @(
				@{
					LanguageTag = "de" 
					DisplayName = "PLZ Gebiet" 
				}
			)
		}
	)
}

# Ändern der Einstellung, diese Einstellung gilt für den gesamten Tenant
New-MgOrganizationSettingProfileCardProperty -OrganizationId $organizationId -BodyParameter $params

#Zum Prüfen, es sollte mind. ein Eintrag zu sehen sein mit dem entspr. Attribut
Get-MgOrganizationSettingProfileCardProperty -OrganizationId $organizationId

#Zurücksetzen des Profils
Select-MgProfile -Name "v1.0"

#Logout
Disconnect-MgGraph

#####################################################
#####################################################
#####################################################

<#
Alternativ (Microsoft Tool)
https://developer.microsoft.com/en-us/graph/graph-explorer 
-> https://graph.microsoft.com/beta/organization/<TenantID hier eintragen>/settings/profileCardProperties
-> per POST
-> beta-Endpoint
-> als Body
{
    "directoryPropertyName": "customAttribute1",
    "annotations": [
        {
            "displayName": "Sales Area",
            "localizations": [
                {
                    "languageTag": "de",
                    "displayName": "PLZ Gebiet"
                }
            ]
        }
    ]
}
#>