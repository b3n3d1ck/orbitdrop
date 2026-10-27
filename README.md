<h1 align="center">Flickwheel</h1>
<p align="center"><b>Your files. Your flow.</b><br>Convert, edit, and manage files from a radial menu, without opening a separate app. Windows 10 (19041+) and 11.</p>

<p align="center"><sub>Flickwheel is a fork of <a href="https://github.com/i3sam/OrbitDrop">OrbitDrop</a> by Mohammed Essam, used under the MIT License. Maintained by <a href="https://github.com/b3n3d1ck">b3n3d1ck</a>. Not affiliated with or endorsed by the original project.</sub></p>

---

> **Status: adopting the upstream codebase. Building on CI; not yet released.**
>
> This repository was just re-based onto the OrbitDrop source as its technical foundation. The
> product is being rebranded to Flickwheel with its own identity. There is **no release yet**, so
> there is nothing to download and no installer to run. Download links will appear here only when a
> real GitHub Release exists, with SHA-256 checksums. No "virus-free" or VirusTotal claim is made,
> because none can be made honestly yet.

## What it is

Flickwheel puts a context-aware radial wheel at your pointer. Drag a file and tap a key, or select
files and press a shortcut: the wheel opens with the actions that actually apply to those files.
The result is written **next to the original**, which is never changed, and recent results can be
undone. There is no main window — it lives in the notification area and appears when you summon it.

## Features (inherited from upstream, being verified on this fork)

These come from the OrbitDrop codebase and build on CI. Each is marked honestly as it is verified on
Flickwheel's own pipeline; nothing here is claimed as hand-tested until it has been run on Windows.

- Image convert between PNG, JPEG, WebP, BMP, TIFF, GIF (via Windows Imaging Component + SkiaSharp), SVG rasterisation.
- Resize, crop, rotate, flip, compress, and target-size reduction, with transparency handling.
- Image → PDF, plus PDF merge/split/rotate/compress (PDFsharp 6.2.4, the last MIT release).
- Video/audio transcode via Windows Media Foundation.
- CorelDRAW (.cdr/.cmx) reading via a bundled native reader (libcdr).
- Batch processing with a job queue, progress, cancellation, and completion notifications.
- File actions, right-click Explorer integration, tray menu, onboarding guide, light/dark themes.

## Architecture

.NET 10 + WPF, Windows-only (`net10.0-windows10.0.19041.0`). Managed projects under `src/`
(`OrbitDrop.Core`, `.Jobs`, `.Conversion`, `.Tools`, `.Infrastructure`, `.UI`, `.Windows`, `.App`),
a native C++ CorelDRAW reader under `src/OrbitDrop.CdrReader`, tests under `tests/`. Internal
assembly and namespace names remain `OrbitDrop.*` to preserve attribution and keep WPF resource URIs
intact; the user-facing product is Flickwheel.

## Build and test

Requires the .NET 10 SDK on Windows.

```powershell
dotnet build OrbitDrop.slnx -c Release
dotnet test tests/OrbitDrop.Tests -c Release
```

The native CorelDRAW reader and the installer are produced by the scripts in `build/` and are only
needed for a packaged release, not for building or testing the app.

## Privacy and security

Processing is local. No telemetry, no accounts, no file uploads. See [SECURITY.md](SECURITY.md) once
added for the reporting process. One upstream behaviour is being reviewed for this fork: the update
checker must be retargeted or disabled so it never contacts the original project's release feed — this
is tracked as a release gate below.

## Attribution and licensing

Flickwheel is MIT-licensed. It is based on **OrbitDrop by Mohammed Essam** (MIT, © 2026), whose
copyright and license are preserved in [LICENSE](LICENSE). The full dependency inventory and the
upstream acknowledgement are in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md). Modifications for
Flickwheel are © 2026 b3n3d1ck.

## Known limitations / release gates

- No release, installer, or portable package yet.
- UI still shows "OrbitDrop" strings in places; the rebrand to Flickwheel is in progress.
- Update checker must be retargeted/disabled before any release.
- Microsoft Store packaging (the original project's identity) has been removed; Flickwheel is
  GitHub-distributed only for now.
- Nothing is code-signed; SmartScreen will warn on first run until provenance/signing is addressed.
