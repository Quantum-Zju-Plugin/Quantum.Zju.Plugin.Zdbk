# Quantum.Zju.Plugin.Zdbk

[简体中文](../zh-CN/README.md)

Read-only Zhejiang University undergraduate academic integration for Quantum, maintained under [Apache-2.0](../../LICENSE). Follow the pinned [English standards](../../.standards/docs/en/README.md), [development guide](development.md), [capability contract](capabilities.md), and [glossary](glossary.md).

Sign in through the independent [ZJU CAS account plugin](https://github.com/Quantum-Zju-Plugin/Quantum.Zju.Plugin.Cas) under Settings → Accounts. This plugin owns academic website requests, private site sessions and data parsing. It does not collect passwords, export CAS cookies, enroll in courses or withdraw from courses.

The published plugin ID remains `quantum.plugin.zdbk`, the assembly remains `Quantum.ZdbkPlugin.dll`, and CLR namespaces remain `Quantum.OfficialPlugins.Zdbk` for compatibility. The repository name does not rename existing capabilities or consumer contracts. Requires the published SDK `0.3.0-rc.3` and the `zju.cas.account` / `zju.cas.service-ticket` capabilities.

The selected-section list supplies upstream class-time and location strings for timetable consumers. The academic website determines the active academic year/term; there is no arbitrary term override. Missing meeting data is returned as missing data, not an invented schedule. Consumers own calendar conversion and synchronization.

## Repository workflow

Use short-lived [topic branches](../../.standards/docs/glossary.md#topic-branch) and [pull requests](../../.standards/docs/glossary.md#change-request) into main, including administrator changes. The personal repository profile does not require an independent approving reviewer or mandatory hosted [CI](../../.standards/docs/glossary.md#ci-cd) gates. Run relevant checks and report hosted-run restrictions honestly.
