# Changelog

Notable changes to PromptlyHash are documented here.

The project follows [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.1.0] - 2026-07-31

### Added

- Windows 11 desktop interface for generating and checking file hashes
- MD5, SHA-1, SHA-256, SHA-384, and SHA-512 support
- Responsive, scrollable layout with progress feedback
- Copy-to-clipboard support
- Markdown-backed About window
- PromptlyHash logo and branding guide
- Self-contained executable and MSI packaging
- Automated hashing and responsive-layout checks

### Security

- Added fixed-time hash comparison and cancellation-aware file hashing
- Added explicit warnings for legacy MD5 and SHA-1 usage
- Enabled transitive NuGet vulnerability auditing
- Pinned GitHub Actions to immutable release commits

[Unreleased]: https://github.com/draconequus93/PromptlyHash/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/draconequus93/PromptlyHash/releases/tag/v0.1.0
