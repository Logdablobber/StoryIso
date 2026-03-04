
@echo off
set "Folder_Path=Your Path"
PowerShell "Get-ChildItem '%Folder_Path%' -Recurse -Directory | Measure-Object | ForEach-Object{$_.Count}"
pause