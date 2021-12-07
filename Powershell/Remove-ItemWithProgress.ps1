#https://www.reddit.com/r/PowerShell/comments/bdg61e/removeitem_with_progres_bar/

function Remove-ItemWithProgress {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.DirectoryInfo] $Path
    )

    try {
        [string] $_Path = (Resolve-Path $Path).Path;
    }
    catch { throw 'Invalid Path.'; }

    $RegEx = '"([^"]+)"\.';
    $Activity = "Deleting $_Path"
    Write-Progress -Id 0 -Activity $Activity -Status "Reading files..." -PercentComplete 0;

    $Directory = Get-Location
    $ItemCount = (Get-ChildItem -Path $_Path -Recurse -Force).Count + 1; # Needs to include -Force for hidden directories
    $Counter = 0;

    $Start = (Get-Date)
    Remove-Item -Path $_Path -Recurse -Force -Verbose 4>&1 | ForEach-Object { 
        $Counter++;

        $Elapsed = ((Get-Date) - $Start).TotalSeconds
        $Remaining = ($Elapsed / ($Counter / $ItemCount)) - $Elapsed
        
        $Text = [regex]::Match($_, $RegEx).Captures.Groups[1].Value.Replace($Directory, ".");
        $Progress = $Counter * 100 / $ItemCount
        Write-Progress -Id 0 -Activity $Activity -Status $Text -PercentComplete $Progress -SecondsRemaining $Remaining
    };
    Write-Progress -Id 0 -Activity $Activity -PercentComplete 100 -Completed;

    Write-Output "Deleted $Counter items."
}

Export-ModuleMember -Function Remove-ItemWithProgress

#Remove-ItemWithProgress -Path "*Ein Path*"
