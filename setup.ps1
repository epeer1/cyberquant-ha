# FSScore Setup Script
# Ensures proper package restoration for .NET Framework project with packages.config

Write-Host "Setting up FSScore project..." -ForegroundColor Green
Write-Host "This .NET Framework project requires NuGet.exe for proper package restoration." -ForegroundColor Yellow

# Function to download NuGet.exe if not found
function Download-NuGet {
    $nugetPath = ".\nuget.exe"
    if (-not (Test-Path $nugetPath)) {
        Write-Host "Downloading NuGet.exe..." -ForegroundColor Yellow
        try {
            Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $nugetPath
            Write-Host "NuGet.exe downloaded successfully!" -ForegroundColor Green
        } catch {
            Write-Host "Failed to download NuGet.exe: $($_.Exception.Message)" -ForegroundColor Red
            return $null
        }
    }
    return $nugetPath
}

# Try to find NuGet.exe in PATH first
$nugetExe = Get-Command "nuget.exe" -ErrorAction SilentlyContinue
if ($nugetExe) {
    $nugetPath = $nugetExe.Source
    Write-Host "Found NuGet.exe in PATH: $nugetPath" -ForegroundColor Green
} else {
    Write-Host "NuGet.exe not found in PATH. Attempting to download..." -ForegroundColor Yellow
    $nugetPath = Download-NuGet
    if (-not $nugetPath) {
        Write-Host "ERROR: Could not obtain NuGet.exe!" -ForegroundColor Red
        Write-Host "Please install Visual Studio or manually download NuGet.exe" -ForegroundColor Yellow
        exit 1
    }
}

# Restore packages using NuGet.exe
Write-Host "Restoring packages using NuGet.exe..." -ForegroundColor Yellow
try {
    & $nugetPath restore FSScoreAssignment.sln
    $restoreExitCode = $LASTEXITCODE
    
    if ($restoreExitCode -eq 0) {
        Write-Host "Package restore completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Package restore completed with warnings (exit code: $restoreExitCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Package restore failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Verify the critical package was restored
if (Test-Path "packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.1") {
    Write-Host "SUCCESS: Roslyn compiler package restored!" -ForegroundColor Green
    
    # Copy Roslyn compiler files to bin\roslyn (build targets don't always work properly)
    Write-Host "Copying Roslyn compiler files to bin\roslyn..." -ForegroundColor Yellow
    try {
        New-Item -ItemType Directory -Path "FSScore.WebApi\bin\roslyn" -Force | Out-Null
        Copy-Item "packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.1\tools\RoslynLatest\*" "FSScore.WebApi\bin\roslyn\" -Recurse -Force
        
        if (Test-Path "FSScore.WebApi\bin\roslyn\csc.exe") {
            Write-Host "SUCCESS: Roslyn compiler files copied to bin\roslyn!" -ForegroundColor Green
        } else {
            Write-Host "WARNING: Failed to copy Roslyn compiler files!" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "WARNING: Error copying Roslyn files: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    
    Write-Host "You can now open FSScoreAssignment.sln in Visual Studio and run the project!" -ForegroundColor Green
    Write-Host "" 
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "   1. Open FSScoreAssignment.sln in Visual Studio" -ForegroundColor White
    Write-Host "   2. Set FSScore.WebApi as startup project" -ForegroundColor White
    Write-Host "   3. Press F5 to run" -ForegroundColor White
    Write-Host "   4. Test API at https://localhost:44355/api/values/test-db" -ForegroundColor White
} else {
    Write-Host "ERROR: Roslyn compiler package not restored!" -ForegroundColor Red
    Write-Host "Try opening the solution in Visual Studio and using Restore NuGet Packages" -ForegroundColor Yellow
    exit 1
}