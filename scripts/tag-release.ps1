<#
Creates an annotated git tag after validating working tree is clean.
Usage:
  .\tag-release.ps1 -Version v0.2.0
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$status = git status --porcelain
if ($status) {
    Write-Error "Working tree is not clean. Commit or stash changes before tagging."
    exit 1
}

$existing = git tag --list $Version
if ($existing) {
    Write-Error "Tag '$Version' already exists."
    exit 1
}

git tag -a $Version -m "Release $Version"
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create tag."
    exit $LASTEXITCODE
}

Write-Host "Created tag: $Version"
Write-Host "Push with: git push origin $Version"
