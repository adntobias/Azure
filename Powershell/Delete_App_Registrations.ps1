# suchen nach namen
connect-mggraph -scopes @('Application.ReadWrite.All')
Get-MgApplication | where DisplayName -like '*test*' | foreach { Remove-MgApplication -ApplicationId $_.Id }