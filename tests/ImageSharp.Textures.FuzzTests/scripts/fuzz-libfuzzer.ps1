<#
    Drives a libFuzzer fuzz session against the ASTC fuzz harness. Modelled on
    https://github.com/Metalnem/sharpfuzz/blob/master/scripts/fuzz-libfuzzer.ps1.

    On first run, downloads the pinned libfuzzer-dotnet driver into .tools/ (gitignored).
    Pass -Driver <path> to use a different binary (e.g. self-built on macOS).
    Findings (crashes / timeouts) are written to findings/<target>/.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('block-mode', 'decompress-block', 'decompress-hdr-block', 'decompress-image')]
    [string]$Target,

    [string]$Driver,

    [int]$MaxTotalTime = 300
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$projectDir = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $projectDir 'ImageSharp.Textures.FuzzTests.csproj'
$publishDir = Join-Path $projectDir 'publish'
$findingsDir = Join-Path $projectDir "findings/$Target"
$seedDataDir = Join-Path $projectDir "SeedData/$Target"
# libFuzzer writes mutated inputs back to its first positional corpus arg. Keep that
# pointed at a gitignored cache so the curated seed data isn't polluted by new findings.
$workingCorpusDir = Join-Path $projectDir "corpus/$Target"
$harnessDll = Join-Path $publishDir 'SixLabors.ImageSharp.Textures.FuzzTests.dll'

if (-not (Test-Path $seedDataDir)) {
    Write-Host "Seed data directory not found: $seedDataDir" -ForegroundColor Red
    exit 1
}

# Resolve libfuzzer-dotnet: caller-supplied or pinned download cached under .tools/.
if (-not $Driver) {
    $libFuzzerVersion = 'v2025.05.02.0904'
    $toolsDir = Join-Path $projectDir '.tools'
    $isWindowsHost = $IsWindows -or $PSVersionTable.PSEdition -eq 'Desktop'

    if ($isWindowsHost) {
        $assetName = 'libfuzzer-dotnet-windows.exe'
        $cachedName = 'libfuzzer-dotnet.exe'
    }
    elseif ($IsLinux) {
        $assetName = 'libfuzzer-dotnet-ubuntu'
        $cachedName = 'libfuzzer-dotnet'
    }
    else {
        Write-Host 'macOS is not covered by upstream prebuilt binaries.' -ForegroundColor Red
        Write-Host 'Build from source: clang -fsanitize=fuzzer libfuzzer-dotnet.cc -o libfuzzer-dotnet'
        Write-Host 'and pass -Driver <path>.'
        exit 1
    }

    $Driver = Join-Path $toolsDir $cachedName
    if (-not (Test-Path $Driver)) {
        New-Item -ItemType Directory -Force -Path $toolsDir | Out-Null
        $url = "https://github.com/Metalnem/libfuzzer-dotnet/releases/download/$libFuzzerVersion/$assetName"
        Write-Host "==> Downloading libfuzzer-dotnet $libFuzzerVersion"
        Invoke-WebRequest -Uri $url -OutFile $Driver -UseBasicParsing
        if (-not $isWindowsHost) { chmod +x $Driver }
    }
}

Write-Host '==> Restoring SharpFuzz CLI tool'
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed' }

Write-Host '==> Publishing fuzz harness'
# Wipe the publish dir so the SharpFuzz instrumentation step always sees fresh DLLs
# (re-instrumenting an already-instrumented assembly is rejected).
if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }
dotnet publish $projectFile -c Release -f net8.0 -o $publishDir --nologo
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

$exclusions = @('dnlib.dll', 'SharpFuzz.dll', 'SharpFuzz.Common.dll', 'SixLabors.ImageSharp.Textures.FuzzTests.dll')
Get-ChildItem $publishDir -Filter *.dll |
    Where-Object { $_.Name -notin $exclusions -and $_.Name -notlike 'System.*.dll' } |
    ForEach-Object {
        Write-Host "==> Instrumenting $($_.Name)"
        dotnet sharpfuzz $_.FullName
        if ($LASTEXITCODE -ne 0) { throw "sharpfuzz instrumentation failed for $($_.Name)" }
    }

New-Item -ItemType Directory -Force -Path $findingsDir | Out-Null
New-Item -ItemType Directory -Force -Path $workingCorpusDir | Out-Null
$artifactPrefix = $findingsDir + [System.IO.Path]::DirectorySeparatorChar

Write-Host "==> Running libfuzzer-dotnet (target=$Target, max_total_time=${MaxTotalTime}s)"
$env:FUZZ_TARGET = $Target
# First corpus arg is libFuzzer's writable working dir; the curated SeedData dir is passed
# additionally as a read-only seed source.
& $Driver "--target_path=dotnet" "--target_arg=$harnessDll" `
    "-artifact_prefix=$artifactPrefix" "-max_total_time=$MaxTotalTime" "-timeout=10" `
    $workingCorpusDir $seedDataDir
$exitCode = $LASTEXITCODE
Remove-Item Env:FUZZ_TARGET
exit $exitCode
