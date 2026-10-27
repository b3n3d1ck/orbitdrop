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

## The ImageSharp licensing note (read before shipping)

ImageSharp v2 was Apache-2.0. Starting with v3 (what this project uses), Six Labors
ships it under the **Six Labors Split License**. In short: it is free to use for
open-source projects and for organizations under a revenue threshold, but a commercial
license is required otherwise. OrbitDrop is MIT-licensed open source, which is the case
the split license is meant to allow — but "open source" is not an automatic pass, and
the terms are the authority, not this paragraph.

Before cutting a public binary release, do one of:

1. Confirm the project qualifies under the current Six Labors Split License, and keep a
   dated note of that determination in the release checklist; or
2. Buy a Six Labors commercial license; or
3. Replace ImageSharp with an engine whose redistribution terms are unambiguous for this
   use (for example a native library wrapper under a permissive or LGPL license, with the
   LGPL dynamic-linking obligations handled).

Until that decision is recorded, treat binary releases as blocked. The source builds and
tests regardless.

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
