#connect-azaccount -TenantId xxxxx-xx-xxxx-xx

#1. Ressourcen Gruppen auslesen
#2. VMs aus Ressource Gruppe auslesen
#3. Status der VM auslesen
#4. {Deallocated | Running} Starten oder Stoppen der VM


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
            Write-Host "Resource Group: $RGN", ('VM Name: ' + $VM.Name), "Status: $VMStatusDetail" 
          
            if($VMStatusDetail -eq 'VM running')
            {
                Write-Host 'Stopping all VMs'
                Stop-AzVM -ResourceGroupName $RGN -Name $VMDetail.Name -Force -NoWait 
            }
            elseif ($VMStatusDetail -eq 'VM deallocated')
            {
                Write-Host 'Starting all VMs'
                Start-AzVM -ResourceGroupName $RGN -Name $VMDetail.Name -NoWait 
            }
        }
    }
}

  #Endpoint Manager Module 
  #Folien an TLN  senden