# OrbitDrop

OrbitDrop is a Windows file utility built around a radial action menu. You drop a file,
a wheel of actions appears, you pick one, and the converted result lands next to the
original without touching it. This repository is the working source, authored by
[b3n3d1ck](https://github.com/b3n3d1ck).

> **Status: early, but building and passing tests on Windows.**
>
> What exists today is one complete vertical slice: image convert and resize, driven
> through the radial menu, with the whole supporting stack (operation model, job queue,
> safe output, tests, Windows CI) wired up as the template every later feature copies.
>
> The GitHub Actions workflow on `windows-latest` restores, builds the full solution
> (WPF app included), runs the test suite, publishes the app, and checksums the executable.
> All of that is passing. There is **no installer and no release binary yet**, so there is
> nothing to download and nothing to scan. When those exist, their SHA-256 hashes will be
> published here and any scan result will carry the matching hash. No virus-free claim is
> made, because none can be made honestly yet.

## What works in this slice

Drop a PNG (or any supported image) onto the window. The wheel offers convert-to-JPEG,
convert-to-PNG, convert-to-WebP, and resize-to-1080px. Pick one with the mouse or the
keyboard. The file is written with a collision-safe name in the same folder, and the job
shows up in the history panel with its progress and outcome.

The same actions appear as a plain button list beside the wheel, so the feature is usable
without the radial interaction and reachable by keyboard and screen readers.

## Feature matrix

Honest status. "Builds on CI" means it compiles in the Windows build but has no automated
test exercising it yet (the UI). "Tested on CI" means a unit/integration test covers it and
passes on `windows-latest`.

| Area | Status |
|------|--------|
| Pie-wheel menu placement + pointer hit-testing | Tested on CI (geometry unit-tested) |
| Radial wheel rendering, keyboard nav, centre preview | Builds on CI; visuals pending manual retest |
| Drag-and-drop into the window | Builds on CI; no UI test yet |
| File picker | Builds on CI; no UI test yet |
| Image convert (JPEG / PNG / WebP / BMP / GIF / TIFF) | Tested on CI |
| Image → PDF (single page, A4 fit) | Tested on CI |
| Transparency flatten onto chosen background | Tested on CI |
| Collision-safe output, originals preserved | Tested on CI |
| Path-traversal guards | Tested on CI |
| Bounded-concurrency job queue | Tested on CI |
| Image crop (visual editor) | Not started |
| PDF as input (PDF → image, merge, split, text) | Not started |
| Video / audio (FFmpeg) | Not started |
| OCR | Not started |
| Archives | Not started |
| Metadata viewer / stripping | Not started |
| Global drag gesture, context-menu entry, tray | Not started |
| Installer (WiX) | Not started |
| Code signing, release pipeline | Not started |

## Supported image formats (this slice)

Read: JPEG, PNG, WebP, BMP, TIFF, GIF, ICO, JFIF (whatever ImageSharp decodes).
Write: JPEG, PNG, WebP, BMP, GIF, TIFF, and PDF (one image per page).

HEIC, AVIF, RAW, and friends are deliberately absent until an engine that genuinely
supports them is added and tested. The wheel never offers a conversion it cannot perform.

## Project layout

```
src/
  OrbitDrop.Core/              operation model, job queue, safe IO, wheel geometry
  OrbitDrop.Operations.Images/ the ImageSharp-backed convert/resize operation
  OrbitDrop.Operations.Pdf/    image → PDF (first-party minimal PDF writer, no PDF library)
  OrbitDrop.App/               WPF shell, pie-wheel control, view models
tests/
  OrbitDrop.Tests/             xUnit suite for Core + Images + Pdf
.github/workflows/ci.yml       Windows build + test + publish
docs/DEPENDENCIES.md           dependency and license inventory
```

`Core` and `Operations.Images` target `net8.0` on purpose, so their logic builds and tests
on any runner. Only `OrbitDrop.App` is Windows-only, because WPF is.

## Build from source

You need the .NET 8 SDK. On Windows:

```powershell
dotnet restore OrbitDrop.sln
dotnet build OrbitDrop.sln -c Release
dotnet test tests/OrbitDrop.Tests/OrbitDrop.Tests.csproj -c Release
dotnet run --project src/OrbitDrop.App/OrbitDrop.App.csproj
```

On Linux or macOS you can still build and test the core libraries (the `OrbitDrop.App`
project will not build off Windows, which is expected):

```bash
dotnet build src/OrbitDrop.Core/OrbitDrop.Core.csproj
dotnet test tests/OrbitDrop.Tests/OrbitDrop.Tests.csproj
```

## How a conversion flows through the code

1. A dropped path becomes a `FileSubject`, classified by extension.
2. `OperationRegistry.GetApplicable` returns only the operations whose `CanApplyTo` is true.
3. `ActionCatalog` turns those into concrete wheel choices with preset parameters.
4. Picking one builds a `Job` (via `JobFactory`, which wires progress) and hands it to the
   `JobQueue`.
5. `ImageConvertOperation` loads the image, resizes if asked (never upscaling), flattens
   transparency when the target has no alpha, writes to a temp file, then moves it into
   place under a non-colliding name.
6. The job reaches `Succeeded` or `Failed`, and the UI reports it plainly.

## Privacy and security

No telemetry. No accounts. No network calls from the app itself, except opening a link in
your browser when you click one. Your originals are never overwritten by default. Untrusted
input (filenames, file bytes, future archive entries) is treated as hostile; see
[SECURITY.md](SECURITY.md) for the threat model and how to report an issue.

## Dependencies and licensing

The application is MIT-licensed. It depends on ImageSharp, whose v3 ships under the Six
Labors Split License rather than a plain permissive one. Because OrbitDrop is itself
open source (MIT), that license grants ImageSharp under Apache-2.0 terms, so binaries can
ship. The determination and the condition under which it would stop holding are recorded in
[THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md); the full inventory is in
[docs/DEPENDENCIES.md](docs/DEPENDENCIES.md).

## Known limitations

- The UI (radial menu, drag-and-drop) compiles on CI but has no automated test yet; it has
  not been click-tested on a desktop. The image engine and core are covered by passing tests.
- Only the first dropped file is processed in this slice; batch comes with a later one.
- No installer, no signed binary, no release.

## Attribution

OrbitDrop by [b3n3d1ck](https://github.com/b3n3d1ck). Contributions and bug reports are
welcome through the repository.
