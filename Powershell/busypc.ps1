#Schreibt ein Script, das eine Webseite in einem Browser öffnet und die Seite automatisch nach unten scrollt.
#Das Skript soll eine angegebene Anzahl von Minuten lang laufen.
#Die Webseite https://scrollmagic.io/examples/advanced/infinite_scrolling.html soll geöffnet werden.

$minutes = 8

Start-Process chrome '"https://scrollmagic.io/examples/advanced/infinite_scrolling.html"'
$shl = New-Object -com "WScript.Shell"

($minutes * 60)..0 | foreach {
    Write-Output "$_ .."

    if ($_ -gt 0)
    {
        Start-Sleep 1
        $shl.sendKeys('{DOWN}')
    }
    else
    {
        Write-Output "end"
        Stop-Computer -ComputerName localhost
    }
}
