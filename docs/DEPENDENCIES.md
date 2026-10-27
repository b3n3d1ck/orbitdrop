# Dependency inventory

Everything OrbitDrop pulls in, why it is here, and what its license obligates. This
list covers the current vertical slice (image convert/resize). It grows as engines
are added; new dependencies get a row here in the same pull request that introduces them.

| Package | Version | Purpose | License | Bundled? | Notes |
|---------|---------|---------|---------|----------|-------|
| SixLabors.ImageSharp | 3.1.5 | Decode/encode and resize images in `OrbitDrop.Operations.Images` | Six Labors Split License (dual: Apache-2.0-style terms for qualifying open-source use, otherwise a paid commercial license) | Yes (NuGet, redistributed in the app output) | See the licensing note below — this is the one that needs a real decision before any commercial release. |
| CommunityToolkit.Mvvm | 8.3.2 | `ObservableObject`, `RelayCommand`, source generators for the WPF view models | MIT | Yes (NuGet) | No obligations beyond preserving the license text. |
| Microsoft.NET.Test.Sdk | 17.11.1 | Test host | MIT | No (test-only, never shipped) | |
| xunit | 2.9.2 | Test framework | Apache-2.0 | No (test-only) | |
| xunit.runner.visualstudio | 2.8.2 | Test runner adapter | Apache-2.0 | No (test-only) | |
| coverlet.collector | 6.0.2 | Code coverage | MIT | No (test-only) | |

## The ImageSharp licensing note — resolved

ImageSharp v2 was Apache-2.0. Starting with v3 (what this project uses), Six Labors ships
it under the **Six Labors Split License v1.0**, a dual license. Its text grants the
component under Apache-2.0 when the consuming software is "licensed under an Open Source or
Source Available license," and falls back to a paid commercial license otherwise (with
revenue-threshold and nonprofit carve-outs).

**Determination (2026-10-27):** OrbitDrop is MIT-licensed, which is an Open Source license,
so it consumes ImageSharp under the Split License's Apache-2.0 grant, royalty-free, and may
redistribute it. Binary releases are **not** blocked on this. The full determination,
including the condition under which it stops holding (OrbitDrop going closed-source), is
recorded in [`THIRD-PARTY-NOTICES.md`](../THIRD-PARTY-NOTICES.md).

The one standing obligation: keep that determination accurate. If OrbitDrop's own license
ever changes, re-check the Split License terms before shipping a binary.

## PDF generation is first-party (no dependency)

Image → PDF is produced by our own code (`OrbitDrop.Operations.Pdf/MinimalPdf.cs`), which writes
a minimal PDF embedding the image as a JPEG via the standard `DCTDecode` filter. This was a
deliberate choice: the common managed PDF libraries are not clean fits for an MIT project —
PDFsharp 6.x is AGPL-3.0-only (strong copyleft) and QuestPDF's community license is
revenue-gated. Writing the one-image PDF ourselves adds no runtime dependency and no new license
obligation. ImageSharp is used only to encode the embedded JPEG (already a dependency).

## Planned dependencies (not yet added)

These belong to later slices and are listed so the licensing homework is visible early:

- **FFmpeg** for media — GPL or LGPL depending on the build; redistribution and the
  "built with" notice obligations differ between them. Will be detected at runtime, not
  silently bundled, until the license path is settled.
- **PDFium / PdfPig / PDFsharp** for PDF — Apache-2.0 (PdfPig), MIT (PDFsharp), BSD-3
  (PDFium) respectively; the choice affects what rendering is possible.
- **Tesseract** for OCR — Apache-2.0, plus language data packs under their own terms.
- **SharpCompress / 7-Zip** for archives — MIT (SharpCompress); 7-Zip is LGPL + unRAR
  restriction.
