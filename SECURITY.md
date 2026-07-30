# Security policy

## Supported versions

Before the first release, security fixes are applied to the default branch. After release, fixes are applied only to the latest published version.

| Version | Supported |
| --- | ---: |
| Unreleased changes on the default branch | Best effort |
| Latest release | Yes |
| Older releases | No |

## Reporting a vulnerability

Please report suspected vulnerabilities privately through [GitHub private vulnerability reporting](https://github.com/draconequus93/PromptlyHash/security/advisories/new).

Include:

- A clear description of the issue and its impact
- The affected version
- Reproduction steps or a proof of concept
- Any suggested mitigation

Do not include vulnerability details in a public issue. If private vulnerability reporting is unavailable, open a public issue requesting a private contact channel without including sensitive details.

The maintainer will try to acknowledge complete reports within seven days. Timelines for investigation and fixes depend on severity and complexity.

## Release integrity

Release checksums are published in `SHA256SUMS.txt`. Checksums detect file changes but do not authenticate the publisher. Release binaries are currently unsigned; this policy will be updated when Authenticode signing is configured.
