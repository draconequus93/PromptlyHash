# Contributing to PromptlyHash

Thank you for considering a contribution.

## Before you start

- Search existing issues and pull requests before opening a duplicate.
- Use a GitHub issue to discuss substantial features or behavior changes first.
- Keep changes focused on one concern.
- Never include private files, credentials, certificates, signing keys, or personal data.

## Development setup

PromptlyHash requires Windows and the .NET 10 SDK or newer.

```powershell
dotnet build PromptlyHash.sln
dotnet run --project tests\PromptlyHash.Tests
dotnet run --project src\PromptlyHash
```

To validate the distributable packages:

```powershell
.\build.ps1 -Msi
```

## Pull requests

A pull request should:

- Explain the problem and the chosen solution.
- Reference related issues.
- Include tests for behavior changes where practical.
- Pass the existing build and test workflow.
- Update user-facing documentation when applicable.
- Avoid unrelated formatting or refactoring.

By submitting a contribution, you agree that it may be distributed under the repository's MIT License and that you have the right to submit it.

## AI-assisted contributions

AI-assisted contributions are welcome when the contributor understands, reviews, and tests the submitted changes. Disclose substantial use of generative AI in the pull-request description, including the tool used. Do not submit generated code you cannot explain or code whose licensing origin is uncertain.

## Security reports

Do not disclose suspected vulnerabilities in public issues or pull requests. Follow [`SECURITY.md`](SECURITY.md).

## Conduct

Participation is subject to [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).
