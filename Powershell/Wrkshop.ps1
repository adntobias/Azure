#Run this script in Azure PowerShell - Script Under Development 

#Variables
$Suffix = $(Get-Random -Maximum 99999)
$RGName = "AzureDevWorkshop" + $Suffix
$Location = "germanywestcentral"
$WebAppPlanName = "demoPlanDev" + $Suffix
$WebAppName = "wrkshpwebfrontend" + $Suffix
$DataStorename = "wrkshpdatastore" + $Suffix
$ImagesContainerName = "images"
$FunctionStorage = "wrkshpfunctionstore" + $Suffix
$FunctionName = "wrkshptodofunction" + $Suffix
$KeyVaultName = "wrkshpkeyvault" + $Suffix
$SecretName = "StorageConnectionString"
$AppInsightsName = "wrkshpmonitoring" + $Suffix
$WorkspaceName = "wrkshpmonitoringworspace" + $Suffix
#--------------------------------------------
$RegistryName = "myacr" + $Suffix
$ImageTag = "frontend:v1"
$AKSName = "myaks" + $Suffix

#WebApp
New-AzResourceGroup -Name $RGName -Location $Location
New-AzAppServicePlan -Name $WebAppPlanName -ResourceGroupName $RGName -Sku S1 -Location $Location
New-AzWebApp -Name $WebAppName -ResourceGroupName $RGName -AppServicePlan $WebAppPlanName
$FrontendUri = (Get-AzWebApp -Name $WebAppName -ResourceGroupName $RGName).DefaultHostName

#Deploy WebApp
#dotnet clean
#dotnet build
#dotnet publish -c Release -o .\myapp
#Compress-Archive .\myapp* deploy.zip -force

#Publish-AzWebApp -ResourceGroupName $RGName -Name $WebAppName -ArchivePath "./deploy.zip"

#StorageAccount
New-AzStorageAccount -Name $DataStorename -ResourceGroupName $RGName -Location $Location -SkuName Standard_LRS
$Key1 = (Get-AzStorageAccountKey -ResourceGroupName $RGName -Name $DataStorename)[0].Value
$ConnString = (Get-AzStorageAccount -ResourceGroupName $RGName -Name $DataStorename).Context.ConnectionString
New-AzStorageContainer -Name $ImagesContainerName -PublicAccess blob -Context $ConnString

#Function
New-AzStorageAccount -Name $FunctionStorage -ResourceGroupName $RGName -Location $Location -SkuName Standard_LRS
New-AzFunctionApp -Name $FunctionName -ResourceGroupName $RGName -StorageAccount $FunctionStorage -Location $Location -OsType Linux -Runtime dotnet -DisableAppInsights -FunctionsVersion 4
$FunctionKey = (Get-AzFunctionAppKey -ResourceGroupName $RGName -Name $FunctionName).Value
$FunctionUri = "https://" + (Get-AzFunctionApp -ResourceGroupName $RGName -Name $FunctionName).HostNames[0] + "/api/"

#Function Deploy
#dotnet clean
#dotnet build
#dotnet publish -c Release -o .\myapp
#Compress-Archive .\myapp* deploy.zip -force
#Publish-AzFunctionApp -ResourceGroupName $RGName -Name $FunctionName -ArchivePath "./deploy.zip"

#KeyVault
New-AzKeyVault -Name $KeyVaultName -ResourceGroupName $RGName -Location $Location
Set-AzKeyVaultSecret -VaultName $KeyVaultName -Name $SecretName -Value $ConnString
$PrincipalID = (Get-AzFunctionAppIdentity -ResourceGroupName $RGName -Name $FunctionName).PrincipalId
Set-AzKeyVaultAccessPolicy -VaultName $KeyVaultName -ObjectId $PrincipalID -PermissionsToSecrets get
$KVSecretUri = "https://" + $KeyVaultName + ".vault.azure.net/secrets/" + $SecretName
$KVUri = "https://" + $KeyVaultName + ".vault.azure.net"

#AppInsights
New-AzResource -ResourceType "Microsoft.Insights/components" -Name $AppInsightsName -ResourceGroupName $RGName -Location $Location -Properties @{Application_Type = "web"}
New-AzOperationalInsightsWorkspace -Name $WorkspaceName -ResourceGroupName $RGName -Location $Location -ApplicationId $AppInsightsName
$WorkspaceKey = (Get-AzOperationalInsightsWorkspaceSharedKey -ResourceGroupName $RGName -Name $WorkspaceName).PrimarySharedKey
$InstrumentationKey = (Get-AzResource -ResourceType "Microsoft.Insights/components" -Name $AppInsightsName -ResourceGroupName $RGName).InstrumentationKey

#AKS
New-AzAksCluster -Name $AKSName -ResourceGroupName $RGName -Location $Location -KubernetesVersion 1.17 -NodeCount 1 -EnableRBAC -EnableMonitoring
$AKSCluster = Get-AzAksCluster -Name $AKSName -ResourceGroupName $RGName
$AADClientId = $AKSCluster.AadProfile.ClientAppId
$AADClientSecret = (Get-AzAksClusterAdminCredential -Name $AKSName -ResourceGroupName $RGName).Password

#ACR
New-AzAcr -Name $RegistryName -ResourceGroupName $RGName -Sku Basic -Location $Location
$ACRUsername = (Get-AzAcrCredential -Name $RegistryName -ResourceGroupName $RGName).Username
$ACRPassword = (Get-AzAcrCredential -Name $RegistryName -ResourceGroupName $RGName).Passwords[0].Value

#ACR Push
#docker build -t $RegistryName.azurecr.io/$ImageTag .
#docker login $RegistryName.azurecr.io -u $ACRUsername -p $ACRPassword
#docker push $RegistryName.azurecr.io/$ImageTag

#AKS Deploy
#kubectl apply -f deployment.yaml
#kubectl apply -f service.yaml

#Outputs
Write-Output "FrontendUri: $FrontendUri"
Write-Output "FunctionUri: $FunctionUri"
Write-Output "FunctionKey: $FunctionKey"
Write-Output "KVSecretUri: $KVSecretUri"
Write-Output "KVUri: $KVUri"
Write-Output "WorkspaceKey: $WorkspaceKey"
Write-Output "InstrumentationKey: $InstrumentationKey"
Write-Output "AADClientId: $AADClientId"
Write-Output "AADClientSecret: $AADClientSecret"
Write-Output "ACRUsername: $ACRUsername"
Write-Output "ACRPassword: $ACRPassword"