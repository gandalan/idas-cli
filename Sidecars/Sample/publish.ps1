#!/usr/bin/env pwsh

param(
    [string]$Version = "0.1.0"
)

$ErrorActionPreference = "Stop"

$ScriptDir = $PSScriptRoot
$RootDir = Split-Path (Split-Path $ScriptDir -Parent) -Parent
$DistDir = Join-Path $RootDir "dist"
$PublishBase = Join-Path $ScriptDir "bin\publish"
$ProjectFile = Join-Path $ScriptDir "IdasSidecarSample.csproj"
$Platforms = @("win-x64", "linux-x64")

New-Item -Path $DistDir -ItemType Directory -Force | Out-Null
if (Test-Path $PublishBase) {
    Remove-Item -Path $PublishBase -Recurse -Force
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Building sidecar sample version $Version" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

foreach ($Platform in $Platforms) {
    Write-Host ""
    Write-Host "--------------------------------------------" -ForegroundColor Yellow
    Write-Host "Building sidecar for $Platform..." -ForegroundColor Yellow
    Write-Host "--------------------------------------------" -ForegroundColor Yellow

    $PublishDir = Join-Path $PublishBase $Platform

    dotnet publish $ProjectFile `
        -c Release `
        -r $Platform `
        -p:PublishSingleFile=true `
        -p:SelfContained=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:Version=$Version `
        -o $PublishDir

    if ($LASTEXITCODE -ne 0) {
        throw "sidecar publish failed for $Platform"
    }

    if ($Platform -like "win-*") {
        Copy-Item -Path (Join-Path $PublishDir "idas-beispiel.exe") -Destination (Join-Path $DistDir "idas-beispiel.exe") -Force
    }
    else {
        Copy-Item -Path (Join-Path $PublishDir "idas-beispiel") -Destination (Join-Path $DistDir "idas-beispiel") -Force
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Sidecar sample copied to ${DistDir}:" -ForegroundColor Cyan
Get-ChildItem -Path $DistDir | Where-Object { $_.Name -like 'idas-beispiel*' } | Format-Table Name, Length, LastWriteTime
Write-Host "============================================" -ForegroundColor Cyan
