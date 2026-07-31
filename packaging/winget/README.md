# WinGet publishing

The versioned folders in this directory contain submission-ready manifests for
the [Windows Package Manager Community Repository](https://github.com/microsoft/winget-pkgs).

## Submit version 0.1.0

1. Install Microsoft's WinGetCreate utility:

   ```powershell
   winget install wingetcreate
   ```

2. Validate the manifests:

   ```powershell
   winget validate .\packaging\winget\0.1.0
   ```

3. Test installation and removal on a clean Windows Sandbox or virtual machine:

   ```powershell
   winget install --manifest .\packaging\winget\0.1.0 --accept-package-agreements
   winget uninstall --id Draconequus.PromptlyHash
   ```

4. Authenticate WinGetCreate with GitHub, then submit:

   ```powershell
   wingetcreate token --store
   wingetcreate submit .\packaging\winget\0.1.0
   ```

WinGetCreate opens a pull request against `microsoft/winget-pkgs`. Microsoft
automation and a repository moderator validate the installer before it enters
the public WinGet catalog.

## Prepare a later release

After publishing the GitHub release, use WinGetCreate to copy the preceding
manifest and populate the new installer URL, hash, and MSI product code:

```powershell
wingetcreate update Draconequus.PromptlyHash `
  --version 0.2.0 `
  --urls https://github.com/draconequus93/PromptlyHash/releases/download/v0.2.0/PromptlyHash-Setup.msi `
  --out .\packaging\winget
```

Always validate and test the generated manifest before submission. Keep the
package identifier and MSI upgrade code stable between releases.
