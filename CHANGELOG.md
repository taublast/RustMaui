# Changelog

## Unreleased

## 1.0.0.3 - 2026-04-26

- Fixed iOS template validation for generated apps by selecting exactly one Rust static archive for Apple linking instead of missing the archive or linking duplicate Cargo outputs.
- Updated the template default generator version and local sample package references to `1.0.0.3` so local validation and package testing restore the corrected package set.

## 1.0.0.2 - 2026-04-26

- Fixed Apple packaging/linking in `RustMaui.Generators` so iOS device and iOS simulator use static Rust archives imported via `__Internal`, while MacCatalyst continues to use bundled dynamic libraries.
- Fixed NativeReference wiring to pick up the real Cargo Apple outputs after build instead of relying on pre-build file evaluation.
- Updated package and template documentation to describe the shared Apple platform contract for RustMaui projects.

## 1.0.0.1 - 2026-04-26

- First release of the `RustMaui`, `RustMaui.Generators`, and `RustMaui.Templates` package family.