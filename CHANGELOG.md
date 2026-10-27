# Changelog

All notable changes are recorded here. Format loosely follows Keep a Changelog;
versions follow SemVer.

## [Unreleased] — 0.1.0 vertical slice

First end-to-end path through the whole stack: drop an image, pick an action on the
radial wheel, get a converted file back. Built as the template every later feature copies.

### Added
- `OrbitDrop.Core`: operation abstraction (`IFileOperation`, `OperationContext`,
  `OperationResult`), a registry that only surfaces actions valid for a given file, a
  bounded-concurrency `JobQueue` with an honest lifecycle, collision-safe `OutputNaming`,
  and `PathSafety` traversal guards.
- `OrbitDrop.Operations.Images`: real image convert/resize on ImageSharp — format change,
  fit-to-max-dimension without upscaling, transparency flattening with a chosen background,
  temp-file-then-move writes.
- `OrbitDrop.App`: WPF shell with a keyboard-navigable radial menu, drag-and-drop, a file
  picker, a job history list, an accessible action list alongside the wheel, and an About
  window. Per-monitor-v2 DPI aware, runs as invoker (no admin).
- `OrbitDrop.Tests`: xUnit coverage for output naming, path safety, classification,
  parameter handling, the job queue (success/failure/throw/cancel/concurrency), and the
  image operation (convert, resize, no-upscale, collision, input preservation, bad input).
- GitHub Actions CI on `windows-latest`: restore, build, test, publish, and SHA-256 of the
  published executable.

### Known gaps (honest status)
- Authored in an environment without a .NET SDK or Windows, so nothing here has been
  compiled or run yet. CI is the first real build. See the README feature matrix.
- No installer, no code signing, no release artifact yet.
- PDF, media, OCR, archives, metadata, context-menu integration, and the global drag
  gesture are not implemented in this slice.
