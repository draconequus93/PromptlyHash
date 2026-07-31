# PromptlyHash

<p align="center">
  <img src="assets/PromptlyHash-logo.svg" width="128" alt="PromptlyHash logo">
</p>

<p align="center">
  Generate and verify file checksums with confidence.
</p>

<p align="center">
  <a href="https://github.com/draconequus93/PromptlyHash/actions/workflows/build.yml"><img src="https://github.com/draconequus93/PromptlyHash/actions/workflows/build.yml/badge.svg" alt="Build status"></a>
  <a href="https://github.com/draconequus93/PromptlyHash/releases"><img src="https://img.shields.io/github/v/release/draconequus93/PromptlyHash?display_name=tag" alt="Latest release"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT license"></a>
  <img src="https://img.shields.io/badge/platform-Windows%2011-0078D4" alt="Windows 11">
</p>

PromptlyHash is an open-source Windows 11 desktop application for generating and checking file hashes. Hashing happens locally; selected files are never uploaded.

## Features

- Browse for any local file
- Generate MD5, SHA-1, SHA-256, SHA-384, or SHA-512 hashes
- Compare a supplied hash with the selected file
- Clear match and mismatch results
- Copy generated hashes to the clipboard
- Responsive and scrollable interface for different screen sizes and DPI settings
- Progress indicator while large files are processed
- Editable Markdown-based About window

## Security considerations

- Prefer SHA-256 or SHA-512 for new checksums.
- MD5 and SHA-1 are included only for compatibility with existing checksums. They are vulnerable to collision attacks and should not be used to establish trust in newly published files.
- A matching checksum confirms that file content matches an expected value; it does not identify or authenticate the file's publisher.
- Hashing operations do not make network requests. The About window opens the project page in the default browser only when its link is clicked.
- The application places a hash on the Windows clipboard only when the user selects **Copy**.

## Download

Download the latest installer or standalone executable from [GitHub Releases](https://github.com/draconequus93/PromptlyHash/releases).

- `PromptlyHash-Setup.msi` installs PromptlyHash and adds a versioned Start menu shortcut. If another PromptlyHash installation is detected, removing it is the default; users can instead keep both versions in separate installation folders.
- `PromptlyHash.exe` is a standalone, self-contained 64-bit executable.
- `SHA256SUMS.txt` contains checksums for both release files.

> [!IMPORTANT]
> Public release files are currently unsigned. Windows SmartScreen may display a warning until code signing is configured. Verify downloads against `SHA256SUMS.txt`.

## Build from source

Requirements:

- Windows
- .NET 10 SDK or newer
- Internet access for the initial .NET runtime and WiX SDK restore

Build and test the standalone executable:

```powershell
.\build.ps1
```

Build and test both the executable and MSI:

```powershell
.\build.ps1 -Msi
```

Outputs are written to:

```text
artifacts\publish\win-x64\PromptlyHash.exe
installer\bin\x64\Release\PromptlyHash-Setup.msi
```

## Development

```powershell
dotnet build PromptlyHash.sln
dotnet run --project src\PromptlyHash
dotnet run --project tests\PromptlyHash.Tests
```

The test application checks known hash vectors, validation, cancellation, file hashing, responsive layout, result visibility, scrolling, selector contrast, legacy-algorithm warnings, and About content.

## Customize the application

- Edit `src/PromptlyHash/About.md` to change the About window. It is embedded during the build.
- See [`BRANDING.md`](BRANDING.md) for logo, color, typography, and writing guidance.
- The primary vector logo is [`assets/PromptlyHash-logo.svg`](assets/PromptlyHash-logo.svg).
- Keep the vector logo and `tools\Generate-Icon.ps1` geometry synchronized when changing the mark, then run the script to regenerate the Windows icon.

## AI development disclosure

PromptlyHash was developed with assistance from **OpenAI Codex using GPT-5.6 Sol**. AI-assisted output was reviewed, tested, and integrated as part of the development process. The project maintainer remains responsible for the published source code and releases.

See [`AI_DISCLOSURE.md`](AI_DISCLOSURE.md) for more information.

## Contributing and security

Contributions are welcome. Read [`CONTRIBUTING.md`](CONTRIBUTING.md) before opening a pull request.

Please do not report security vulnerabilities in public issues. Follow the private process in [`SECURITY.md`](SECURITY.md).

## License

Copyright © 2026 Draconequus.

PromptlyHash is available under the [MIT License](LICENSE).
