@echo off
title PERTAMINA OTO - Pratinjau Web MVC
pwsh -NoProfile -File "%~dp0Start-Preview.ps1"
if errorlevel 1 pause
