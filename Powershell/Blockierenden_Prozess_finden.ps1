$lockedFile="*File oder Folder*" 

#Wichtig: Folder mit "\" trennen (nicht "/")
#$lockedFile = $lockedFile.Replace("/", "\")

Get-Process | foreach {
    $processVar = $_;
    $_.Modules | foreach {
        if($_.FileName -like "*$lockedFile*") {
            $processVar.Name + " PID:" + $processVar.id
        }
    }
}