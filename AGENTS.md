# ZJU undergraduate academics plugin instructions

- Follow the [English standards](.standards/docs/en/README.md) and [Git branching rules](.standards/docs/en/02-development/git-branching.md). Inspect the branch and working tree, refresh the approved target, use a topic branch, and merge through a pull request. Never directly edit, commit or push protected branches.
- Start with the [English documentation](docs/en/README.md). Keep English and Simplified Chinese documentation in mutually linked docs/en and docs/zh-CN files. AGENTS.md remains English only.
- This repository is public under Apache-2.0. Audit source and history for credentials, private paths and internal information before publication.
- Keep temporary scripts, one-off evidence, acceptance logs, histories and real university responses out of Git under [MNT-09](.standards/docs/en/05-maintenance/maintenance-standards.md#keep-acceptance-records-outside-source-repositories). Only synthetic maintained fixtures belong in tests.
- Consume the published SDK package. ZJU CAS owns sign-in and service tickets; this plugin owns academic requests, its private site session, business parsing and academic RPC handlers. Never collect passwords or copy CAS browser cookies.
- Preserve the published plugin ID, assembly name, CLR namespaces, RPC names and JSON fields. Tests mirror src feature paths. Keep one production type per file and reuse NOF generated RPC registration.
- Follow .editorconfig, including var, braces for control flow, PascalCase static fields and single-line expression bodies. Run every dotnet command elevated on this Windows machine. Restore, format, build and test affected projects.
- main uses pull requests, including administrator changes. Independent approving reviews and required CI gates are not configured in this personal repository profile; run relevant checks and report limitations honestly.
