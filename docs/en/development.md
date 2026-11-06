# Development

[简体中文](../zh-CN/development.md) · [Documentation](README.md)

Use .NET 10, Git, Python 3 and PowerShell 7. Initialize the standards [submodule](../../.standards/docs/glossary.md#submodule). Production and unit tests build independently without sibling checkouts or private package feeds:

```sh
git submodule update --init --recursive
dotnet restore Quantum.Zju.Plugin.Zdbk.slnx
dotnet format Quantum.Zju.Plugin.Zdbk.slnx
dotnet build Quantum.Zju.Plugin.Zdbk.slnx -c Release --no-restore
dotnet test Quantum.Zju.Plugin.Zdbk.slnx -c Release --no-build
pwsh .ci/Build.ps1
pwsh .ci/Verify-Host.ps1
```

Run dotnet elevated on the maintainer's Windows environment. `.ci/Build.ps1` verifies formatting and packages `artifacts/quantum.plugin.zdbk.zip`; it does not publish. `.ci/Verify-Host.ps1` fetches exact public Desktop and CAS commits from `.ci/*-revision.txt` into ignored `.ci-dependencies`. These source references exist only in host integration tests; production references the published SDK package. Do not update pins implicitly.

Source lives in `src/Quantum.ZdbkPlugin`. Tests mirror its root, Application and Contract features. Unit tests use fictional data. Host integration tests exercise capability discovery, missing CAS dependencies, anonymous failures, business reads through the real host JSON boundary and reload. Never add real student responses to fixtures.

For a live read-only check, sign in through the official university page using the CAS plugin, then invoke the [documented capabilities](capabilities.md). Check course names/credits, selected sections and meeting time/location, teaching sections and graduation data available to the account. An empty result is not proof that a nonempty timetable was read. Do not modify enrollment. Keep credentials, site sessions, raw academic responses and per-run results outside Git; dispose test sessions afterwards. Never disable certificate validation.

Do not publish documentation containing workstation paths, work logs, task evidence or internal source. Audit source and artifacts before upload. Builds use a deterministic path map and distribution packages omit debug symbols.

The initial academic implementation was maintained by the product engineering team as the Zdbk component of Quantum.OfficialPlugins. Synthetic fixtures are authored examples of the upstream schema, not copied student records. Preserve the original copyright notice and published compatibility contracts.
