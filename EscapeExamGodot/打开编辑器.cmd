@echo off
setlocal
set "APPDATA=%~dp0local_data"
start "Escape Exam Editor" "%~dp0tools\runtime\Godot_v4.7.2-stable_win64.exe" --path "%~dp0." --editor --log-file "%~dp0evidence\logs\editor.log"
