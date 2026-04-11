# Changelog

All notable changes to this project are documented in this file.

## [0.2.0] - 2026-04-11

### Added
- Endless-runner gameplay in DemoGame with jump, scoring, restart flow, and fair obstacle spawning.
- Build/run scripts for demo and showcase modes on PowerShell and POSIX shells.
- Test project `RenbokoEngine.Tests` with initial coverage for ServiceLocator, InputSystem, and AssetManifest.
- CI test execution step for the new test project.
- Repository `.editorconfig` for consistent formatting defaults.

### Changed
- Main menu/demo runtime reliability improvements (asset and font handling, content copy behavior).
- Obstacle spawning switched from timer-based to distance-based spacing with landing buffer.
- Player and obstacle baseline alignment in endless-runner scene.
- CI workflow expanded with formatting automation and test build/run steps.
- Script hardening with stricter error handling in PowerShell launch scripts.

### Fixed
- Asset manifest parsing now safely handles invalid JSON by returning a default manifest.
- Multiple nullability and safety issues in engine/demo flows addressed in prior work.

## [0.1.0] - 2026-04-10

### Added
- Initial Renboko engine modules and DemoGame sample.
