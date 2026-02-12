#!/usr/bin/env pwsh
# ============================================================================
# publish.ps1 - Build and package idas-cli for Windows and Linux
# 
# Usage:
#   .\publish.ps1 [version]
#   pwsh .\publish.ps1 [version]
#
# If version is not provided, defaults to "0.0.1"
# Can be run locally or from GitHub Actions
# ============================================================================

param(
    [string]$Version = "0.0.1"
)

$ErrorActionPreference = "Stop"

$ProjectDir = $PSScriptRoot
$DistDir = Join-Path $ProjectDir "dist"
$PublishBase = Join-Path $ProjectDir "bin\publish"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Building idas-cli version $Version" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# Clean previous build artifacts
if (Test-Path $DistDir) {
    Write-Host "Cleaning dist directory..."
    try {
        Remove-Item -Path $DistDir -Recurse -Force -ErrorAction Stop
    }
    catch {
        Write-Warning "Could not delete some files in dist (they may be in use). Attempting to clean what we can..."
        Get-ChildItem -Path $DistDir -Recurse | ForEach-Object {
            try {
                Remove-Item $_.FullName -Force -ErrorAction Stop
            }
            catch {
                Write-Warning "Skipping locked file: $($_.Name)"
            }
        }
    }
}
if (Test-Path $PublishBase) {
    Write-Host "Cleaning publish directory..."
    try {
        Remove-Item -Path $PublishBase -Recurse -Force -ErrorAction Stop
    }
    catch {
        Write-Warning "Could not delete some files in publish directory. They may be overwritten during build."
    }
}
New-Item -Path $DistDir -ItemType Directory -Force | Out-Null

# Define platforms to build
$Platforms = @("win-x64", "linux-x64")

foreach ($Platform in $Platforms) {
    Write-Host ""
    Write-Host "--------------------------------------------" -ForegroundColor Yellow
    Write-Host "Building for $Platform..." -ForegroundColor Yellow
    Write-Host "--------------------------------------------" -ForegroundColor Yellow
    
    $PublishDir = Join-Path $PublishBase $Platform
    
    # Publish as self-contained single file
    # Note: We skip separate restore/build and let publish handle everything
    # to ensure the correct self-contained binaries are produced
    # PublishTrimmed is disabled due to reflection usage in the codebase
    Write-Host "Publishing self-contained single-file executable..."
    
    $ProjectFile = Join-Path $ProjectDir "idas.csproj"
    dotnet publish $ProjectFile `
        -c Release `
        -r $Platform `
        -p:PublishSingleFile=true `
        -p:SelfContained=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:Version=$Version `
        -o $PublishDir
    
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $Platform"
    }
    
    # Copy README, .env.sample and .agents folder to publish directory
    $ReadmePath = Join-Path $ProjectDir "README.md"
    Copy-Item -Path $ReadmePath -Destination $PublishDir -Force
    
    $EnvSamplePath = Join-Path $ProjectDir ".env.sample"
    Copy-Item -Path $EnvSamplePath -Destination $PublishDir -Force
    
    $AgentsDir = Join-Path $ProjectDir ".agents"
    $AgentsDest = Join-Path $PublishDir ".agents"
    Copy-Item -Path $AgentsDir -Destination $AgentsDest -Recurse -Force
    
    # Package based on platform
    Write-Host "Packaging..."
    $ArchiveName = "idas-$Version-$Platform"
    
    if ($Platform -like "win-*") {
        # Create zip for Windows
        $ZipPath = Join-Path $DistDir "$ArchiveName.zip"
        Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipPath -Force
        Write-Host "Created: $ZipPath" -ForegroundColor Green
        
        # Copy artifacts to dist folder (Windows)
        Write-Host "Copying Windows artifacts to dist/..."
        Copy-Item -Path "$PublishDir\*" -Destination $DistDir -Recurse -Force
    }
    else {
        # Create tar.gz for Linux/macOS
        $TarGzPath = Join-Path $DistDir "$ArchiveName.tar.gz"
        
        # Use tar command if available (Windows 10+ has built-in tar)
        Push-Location $PublishDir
        try {
            tar -czvf $TarGzPath *
            if ($LASTEXITCODE -ne 0) {
                throw "tar command failed"
            }
            Write-Host "Created: $TarGzPath" -ForegroundColor Green
        }
        finally {
            Pop-Location
        }
        
        # Copy artifacts to dist folder (Linux)
        Write-Host "Copying Linux artifacts to dist/..."
        $LinuxBinary = Join-Path $PublishDir "idas"
        $LinuxDest = Join-Path $DistDir "idas-linux-x64"
        Copy-Item -Path $LinuxBinary -Destination $LinuxDest -Force
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Build complete! Artifacts in ${DistDir}:" -ForegroundColor Cyan
Get-ChildItem -Path $DistDir | Format-Table Name, Length, LastWriteTime
Write-Host "============================================" -ForegroundColor Cyan
