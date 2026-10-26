@echo off
setlocal
set "APPDATA=%~dp0local_data"
if not exist "%~dp0tools\runtime\Godot_v4.7.2-stable_win64.exe" (
  echo Missing isolated runtime. Run tools\setup_runtime.ps1 first.
  pause
  exit /b 1
)
start "Escape Exam" "%~dp0tools\runtime\Godot_v4.7.2-stable_win64.exe" --path "%~dp0." --log-file "%~dp0evidence\logs\play.log"
