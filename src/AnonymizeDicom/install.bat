@echo off
set "TOOL=%~dp0"
set "TOOL=%TOOL:~0,-1%"
powershell -NoProfile -ExecutionPolicy Bypass -Command "[Environment]::SetEnvironmentVariable('Path', [Environment]::GetEnvironmentVariable('Path','User') + ';' + '%TOOL%', 'User')"
if %errorlevel% neq 0 (
  echo Failed to update PATH.
  pause
  exit /b 1
)
echo Added to user PATH: %TOOL%
echo Open a NEW Command Prompt and run: anonymizeDicom --help
pause
