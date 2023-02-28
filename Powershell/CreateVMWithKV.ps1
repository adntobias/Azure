#webhook https://b880b3c4-243d-4cff-b82b-776eb789bfbc.webhook.ne.azure-automation.net/webhooks?token=Tb%2fyPCTyqsoEPg0Dg1iEhGFKVJn75LR27fkJoDh4U4I%3d

param
(
    [Parameter(Mandatory = $false)]
    [string] $vmName = "vmyotesttp",
    [object] $WebhookData
)

Disable-AzContextAutosave -Scope Process

$ctx = (Connect-AzAccount -Identity).Context
Set-AzContext -Subscription $ctx.Subscription 

$rgName = "vmTest"
$location = "northeurope"

if ($WebhookData) {
    $WebhookDataJSON = $WebhookData | ConvertFrom-Json

    <#
    --- to demo ---
    URL - Webhook
    POST - Body/JSON
        {
            "vmName":"vmbywbhk",
            "rgName":"vmTestWbHk",
            "location":"centralus"
        }
    #>
    if ($WebhookDataJSON.RequestBody) 
    {
        $payload = (ConvertFrom-Json -InputObject $WebhookDataJSON.RequestBody)
        $vmName = $payload.vmName
        $rgName = $payload.rgName
        $location = $payload.location
    }


    <#
    --- to test ---
    URL - Webhook
    POST - Body/JSON
        [
	      {"name":"vm1", "id":21},
	      {"name":"vm2", "id":31},
	      {"name":"vm3", "id":41}
        ]

    write-output $WebhookDataJSON.WebhookName
    write-output $WebhookDataJSON.RequestBody
    write-output $WebhookDataJSON.RequestHeader
    
    if ($WebhookDataJSON.RequestBody) { 
        $names = (ConvertFrom-Json -InputObject $WebhookDataJSON.RequestBody)
       
        foreach ($x in $names) {
            $name = $x.Name
            Write-Output "Hello $name"
        }
    }
#>
}

try {
    Write-Output "calling secrets from Vault"
    $secretPW = Get-AzKeyVaultSecret -VaultName "supersecretcreds" -Name "adminPW" -AsPlainText
    $secretUser = Get-AzKeyVaultSecret -VaultName "supersecretcreds" -Name "adminUser" -AsPlainText
}
catch {
    Write-Output "Error reading secrets: $($_.Exception.Message)"
}

$secretPW = ConvertTo-SecureString $secretPW -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential ($secretUser, $secretPW)

if ((Get-AzResourceGroup -Name $rgName -ErrorAction SilentlyContinue) -eq $null) {
    try {
        Write-Output "Creating Test RG"
        New-AzResourceGroup -Name $rgName -Location $location
    }
    catch {
        Write-Output "Error creating RG: $($_.Exception.Message)"
    }
}

Write-Output "Checkign if vm already exists"
Get-AzVM -Name $vmName -ResourceGroupName $rgName -ErrorVariable notPresent -ErrorAction SilentlyContinue

if ($notPresent) {
    Write-Output "Creating Test VM"
    
    try {
        $SingleSubnet = New-AzVirtualNetworkSubnetConfig -Name "MySubnet" -AddressPrefix "10.0.0.0/24"
        $Vnet = New-AzVirtualNetwork -Name "MyNet" -ResourceGroupName $rgName -Location $location -AddressPrefix "10.0.0.0/16" -Subnet $SingleSubnet
        $PIP = New-AzPublicIpAddress -Name "MyPIP" -DomainNameLabel "mydnsnametp" -ResourceGroupName $rgName -Location $location -AllocationMethod Dynamic -Sku Basic
        $NIC = New-AzNetworkInterface -Name "MyNIC" -ResourceGroupName $rgName -Location $location -SubnetId $Vnet.Subnets[0].Id -PublicIpAddressId $PIP.Id

        #Images finden
        #Get-AzVMImagePublisher -Location $location | Select PublisherName
        #Get-AzVMImageOffer -Location $location -PublisherName "Canonical" | Select Offer
        #Get-AzVMImageSku -Location $location -PublisherName "Canonical" -Offer "UbuntuServer" | Select Skus

        $VirtualMachine = New-AzVMConfig -VMName $vmName -VMSize "Standard_B1ms"
        $VirtualMachine = Set-AzVMSourceImage -VM $VirtualMachine -PublisherName "Canonical" -Offer "UbuntuServer" -Skus "19.04" -Version "latest" 
        $VirtualMachine = Set-AzVMOperatingSystem -VM $VirtualMachine -ComputerName $vmName -Credential $cred -Linux               
        $VirtualMachine = Add-AzVMNetworkInterface -VM $VirtualMachine -Id $NIC.Id
        $VirtualMachine = Set-AzVMOSDisk -VM $VirtualMachine -Name "OsDisk12" -Caching "ReadWrite" -CreateOption "FromImage"

        New-AzVM -ResourceGroupName $rgName -VM $VirtualMachine -location $location
    }
    catch {
        Write-Output "Error creating VM: $($_.Exception.Message)"
    }
}
else {
    Write-Output "Test VM already exists"
}