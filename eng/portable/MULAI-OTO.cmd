@echo off
setlocal
set "DOTNET_ROOT=%~dp0runtime"
set "DOTNET_ROOT_X64=%~dp0runtime"
set "DOTNET_MULTILEVEL_LOOKUP=0"
cd /d "%~dp0app"
start "" "%~dp0app\Pertamina.Oto.Desktop.App.exe"
endlocal
