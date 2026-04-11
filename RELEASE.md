# Release Guide

This repository uses a lightweight manual release process.

## Prerequisites
- .NET 8 SDK
- Clean working tree

## 1) Build and test

```powershell
dotnet restore
dotnet build MainEngine\RenbokoWorkspace.sln -c Release
dotnet test Tests\RenbokoEngine.Tests\RenbokoEngine.Tests.csproj -c Release
```

## 2) Package DemoGame

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\publish-demo.ps1 -Release
```

The output package is written to `artifacts/demo/`.

## 3) Update changelog
- Add release notes under a new version heading in `CHANGELOG.md`.

## 4) Create git tag

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\tag-release.ps1 -Version v0.2.0
git push origin v0.2.0
```
