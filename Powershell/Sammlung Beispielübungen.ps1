#�bungen mit Musterl�sung

# Alle Prozesse nur mit Namen und ID ausgeben lassen, (ABER nur die ersten und letzten 5 Einträge)
Get-Process | Select-Object -Property Name, Id -First 5 -Last 5

#5 Prozesse, die mehr als 5MB Speicher verbrauchen
Get-Process | where -Property WS -gt 5MB | select -First 5

#Alle Prozesse, die mehr als 50MB und weniger als 100MB Speicher verbrauchen
Get-Process | where {($_.WS -gt 50MB) -and ($_.WS -lt 100MB)}

#Alle laufenden Services, beginnend "a" ausgeben
Get-Service | where {$_.Status -eq "running" -and $_.Name -like "a*"}

#15 zufällige Services nach Status absteigend und Name aufsteigend sortiert
Get-Service | Get-Random -Count 15 | Sort-Object @{ expression = "Status"; Descending = $true}, Name 

#Die unten stehende ForEach-Schleife in Einzeiler umwandeln 
#Tipp: Mehrere Befehle in einer Zeile werden mit ; getrennt
New-Item -Path c:\dates -ItemType Directory #Zur Vorbereitung einen Ordner erstellen, da sonst New-Item einen Fehler wirft

foreach($i in 1..30)
{
   $logFolderItem = ("C:\dates\Log_{0:yyyyMMdd_HHmmss}.txt" -f (Get-Date))
   New-Item -Path $logFolderItem
   Start-Sleep -Seconds 1
}

#-----

1..30 | ForEach-Object { New-Item -Path ("C:\dates\Log_{0:yyyyMMdd_HHmmss}.txt" -f (Get-Date)); Start-Sleep -Seconds 1 }

#Alles bis auf die neuesten 10 Dateien l�schen
Get-ChildItem "C:\dates" | Sort-Object -Property CreationTime -Descending | Select-Object -Skip 10 | Remove-Item

# Alle Dateien in c:\dates grafisch ausgeben und ausgew�hlte l�schen
Get-ChildItem -Path C:\dates | Out-GridView -PassThru | Remove-Item #-WhatIf

# 5 Dateien in c:\dates mit je 5MB Dateigröße erzeugen
1..5 | foreach {Set-Content -Path c:\dates\dat$_.txt -Value ('.'*5MB)}

#Credential interaktiv abrufen
$cred = Get-Credential -Message "Bitte Nutzername und Passwort eingeben"

#Credential-Objekt automatisiert erstellen -- Default-User und Passwort
$user = "Max_Mustermann"
$pwd = "Passwort123" | ConvertTo-SecureString -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential -ArgumentList $user, $pwd

#H�ufige Kurzschreibweisen
Get-Member => gm
Where-Object => where => ?
ForEach-Object => foreach => %
Select-Object => select
Sort-Object => sort
Get-Command => gcm
<#
	Alle about_ Files ausgeben, hier finden sich Informationen zur PowerShell selbst
	zb. die operatoren about_operators
#>
Get-Help about_*
Get-Help about_* | Out-GridView -PassThru | Get-Help -ShowWindow #Ein wenig Übersichtlicher ...

# Alle Dateien in c:\dates grafisch ausgeben und ausgewählte löschen
Get-ChildItem -Path C:\dates | Out-GridView -PassThru | Remove-Item -WhatIf

#----------------------------------------------------------------------------------------------------------------
#----------------------------------------------------------------------------------------------------------------

#Credential interaktiv abrufen
$cred = Get-Credential -Message "Bitte Nutzername und Passwort eingeben"

#Credential-Objekt automatisiert erstellen -- Default-User und Passwort -- Kein MFA
$user = "Max_Mustermann"
$pwd = "Passwort123" | ConvertTo-SecureString -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential -ArgumentList $user, $pwd

#MFA-f�hig
Connect-AzAccount

#----------------------------------------------------------------------------------------------------------------
#----------------------------------------------------------------------------------------------------------------

#Abrufen der Regionen
$loc = Get-AzLocation | 
       select location, displayname | 
       sort location | 
       Out-GridView -PassThru  

#Neue ResourceGroup anlegen
$rg = New-AzResourceGroup -Name "MyRg" `
                          -Location $loc

#ResourceGroup abrufen
Get-AzResourceGroup -Name $rg.ResourceGroupName

#L�schen einer ResourceGroup
Remove-AzResourceGroup -Id $rg.ResourceId 

#----------------------------------------------------------------------------------------------------------------
#----------------------------------------------------------------------------------------------------------------

# Namen und Location als Variablen setzen
$resourceGroup = "myResourceGroup"
$location = "westeurope"
$vmName = "myVM"

# Userobjekt erstellen
$cred = Get-Credential -Message "Enter a username and password for the virtual machine."

# Ressourcengruppe erstellen
New-AzResourceGroup -Name $resourceGroup -Location $location

# Die VM erstellen
New-AzVM `
  -ResourceGroupName $resourceGroup `
  -Name $vmName `
  -Location $location `
  -ImageName "Win2016Datacenter" `
  -VirtualNetworkName "myVnet" `
  -SubnetName "mySubnet" `
  -SecurityGroupName "myNetworkSecurityGroup" `
  -PublicIpAddressName "myPublicIp" `
  -Credential $cred `
  -OpenPorts 3389 #RDP

#----------------------------------------------------------------------------------------------------------------
#----------------------------------------------------------------------------------------------------------------

#Arbeiten mit VMs
function Get-AzVMStatus
{
  Import-Module Az.Compute

  $RGs = Get-AzResourceGroup
  
  foreach($RG in $RGs)
  {
    $VMs = Get-AzVM -ResourceGroupName $RG.ResourceGroupName  
    
    foreach($VM in $VMs)
    {
      $VMDetail = Get-AzVM -ResourceGroupName $RG.ResourceGroupName -Name $VM.Name -Status
      $RGN = $VMDetail.ResourceGroupName  
     
      foreach ($VMStatus in $VMDetail.Statuses)
      { 
          $VMStatusDetail = $VMStatus.DisplayStatus
      }

      Write-Output "Resource Group: $RGN", ("VM Name: " + $VM.Name), "Status: $VMStatusDetail" `n
    }
  }
}

Get-AzVMStatus

#----------------------------------------------------------------------------------------------------------------
#----------------------------------------------------------------------------------------------------------------