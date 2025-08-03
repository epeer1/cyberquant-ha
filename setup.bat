@echo off
echo.
echo FSScore Setup - Restoring NuGet packages for .NET Framework project
echo.

REM Check if PowerShell is available
powershell -Command "Write-Host 'PowerShell detected'" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: PowerShell is required but not found!
    echo Please install PowerShell or run setup.ps1 manually.
    pause
    exit /b 1
)

REM Run the PowerShell setup script
echo Running setup.ps1...
powershell -ExecutionPolicy Bypass -File .\setup.ps1

REM Check if setup was successful
if %ERRORLEVEL% EQU 0 (
    echo.
    echo SUCCESS! Setup completed successfully.
    echo You can now open FSScoreAssignment.sln in Visual Studio.
) else (
    echo.
    echo FAILED! Setup encountered an error.
    echo Try opening FSScoreAssignment.sln in Visual Studio and manually restoring packages.
)

echo.
pause