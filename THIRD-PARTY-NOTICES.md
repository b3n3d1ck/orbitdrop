# Third-party notices

OrbitDrop is distributed under the MIT License (see `LICENSE`). It redistributes the
following third-party components. This file records what they are and, where the terms are
not a plain permissive license, the determination that lets OrbitDrop ship them.

## SixLabors.ImageSharp 3.1.5

- Project: https://github.com/SixLabors/ImageSharp
- License: **Six Labors Split License, Version 1.0 (June 2022)**
- Bundled: yes (redistributed in the application output)

### Determination (recorded 2026-10-27)

The Six Labors Split License is a dual license. Its own text grants the component under
the **Apache License 2.0** when, among other cases:

> "You are consuming the Work for use in software licensed under an Open Source or Source
> Available license."

OrbitDrop is released under the MIT License, which is an Open Source license. OrbitDrop
therefore consumes ImageSharp under the Apache-2.0 grant of the Split License, royalty-free,
and may redistribute it in its binaries. No Six Labors commercial license is required for
this use.

This determination holds **only while OrbitDrop itself stays under an OSS/Source-Available
license**. If OrbitDrop is ever relicensed as closed-source, or distributed as part of a
proprietary product, this clause no longer applies and the Split License's revenue
threshold and commercial terms must be re-evaluated before shipping a binary. See
https://sixlabors.com/pricing/ and keep this file updated with the new determination.

Apache-2.0 requires preserving the copyright and license notices, which this file and the
bundled package metadata do.

## CommunityToolkit.Mvvm 8.3.2

- Project: https://github.com/CommunityToolkit/dotnet
- License: MIT
- Bundled: yes

No obligations beyond preserving the MIT license text, which ships in the package.

## Test-only dependencies (not redistributed)

xunit (Apache-2.0), xunit.runner.visualstudio (Apache-2.0), Microsoft.NET.Test.Sdk (MIT),
and coverlet.collector (MIT) are used only to build and run the test suite. They are never
part of the application output, so they carry no redistribution obligation here.
