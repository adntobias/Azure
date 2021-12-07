Get-Module -ListAvailable | <#where Name -like "Microsoft.Graph*" |#> ForEach-Object {
    $ModuleName = $_.Name;
    $Latest = Get-InstalledModule $ModuleName; 
    #Write-Host $ModuleName -ForegroundColor Green;
    Get-InstalledModule $ModuleName -AllVersions | where {$_.Version -ne $Latest.Version} | Uninstall-Module #-Force
}

#Update-Module