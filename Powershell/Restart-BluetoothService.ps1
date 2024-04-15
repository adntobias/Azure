$seconds = 1
try {
    Get-Service *bluetooth* | Restart-Service -Force 
    Write-Output "restarted bluetooth service"
}
catch {
    write-error "error: " + $_.Exception.Message
    $seconds = 10
}
Start-Sleep -Seconds $seconds
