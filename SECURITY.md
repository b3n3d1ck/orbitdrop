# Security policy

## Reporting a vulnerability

Report privately. Open a GitHub Security Advisory on the repository
(`Security` tab → `Report a vulnerability`), or email the maintainer through the
address on https://github.com/b3n3d1ck. Please do not file a public issue for
something exploitable until there has been a chance to fix it.

Include what you did, what happened, and a sample input if one triggers it. A small
crafted file that reproduces the problem is worth more than a long description.

## What OrbitDrop promises

- **Offline by default.** The application makes no network calls of its own. It reads
  the local files you hand it and writes output locally. The only outbound action is
  opening a link in your browser when you click one in the About window.
- **No telemetry, no accounts, no background services.** Nothing is uploaded, and the
  app installs no hidden startup task or service.
- **Your originals are left alone.** Operations write to a temporary file and move the
  finished result into place with a collision-safe name; the input is never overwritten
  by default.

## Threat model (what the code treats as hostile)

Filenames, file contents, metadata, and archive entry names are all untrusted input.
The slice currently enforces:

- **Path traversal / "zip slip":** `PathSafety` resolves any untrusted entry name against
  its destination directory on fully-resolved paths and rejects anything that escapes.
  Covered by tests.
- **Output collisions:** `OutputNaming` never clobbers an existing file; it falls back to
  `name (2).ext`. The image operation additionally refuses to resolve onto its own input.
- **Malformed input:** a corrupt or non-image file produces a failed result with a message,
  not a crash, and any half-written temporary output is cleaned up.

Areas that arrive with later slices (archive extraction limits, symlink handling, FFmpeg
process invocation, HTML sanitization for document preview) are tracked in the README's
feature matrix and are **not** claimed as done until their code and tests exist.

## A clean scan is not a guarantee

If a release is ever scanned (for example with VirusTotal), the recorded result is a
point-in-time report for one exact file hash, shared with that service's partners. It is
evidence, not proof of safety, and it is never fabricated. No scan result is claimed here
unless the matching hash is published alongside it.
