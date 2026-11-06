# ZJU undergraduate academics

[简体中文](../zh-CN/capabilities.md) · [Documentation](README.md)

Zdbk 0.4 consumes `zju.cas.account` and `zju.cas.service-ticket` from the [ZJU CAS account provider](https://github.com/Quantum-Zju-Plugin/Quantum.Zju.Plugin.Cas). Sign in through the account settings before invoking its business capabilities. The provider supplies [CAS](glossary.md#cas) authentication only; Zdbk owns academic requests, the private [site session](glossary.md#site-session) and response parsing.

## Session boundary

`AcademicSession` reads the current connection metadata, obtains a [service ticket](glossary.md#service-ticket) for the academic callback and consumes it using an independent framework cookie container. It preserves the supplied [User-Agent](glossary.md#user-agent), follows only approved academic destinations over [HTTPS](glossary.md#http), and never receives the provider's main-session cookies. The academic server selects the signed-in user; the plugin does not collect a student number or put one into business query parameters.

The session is reused while the provider's connection identifier remains unchanged. Each business read checks that identifier before and after the site operation. Reconnect, disconnect or an account change discards the old site client when next observed; a change during a read rejects the result instead of returning data from the previous account. Plugin disposal also releases the client. An expired site session can be renewed once through a fresh ticket; it never triggers a password form or unbounded retries. Provider disconnection does not perform university-wide logout.

## Business capabilities

The public [RPC](glossary.md#rpc) names and response fields remain unchanged:

| Capability | Input | Result |
| --- | --- | --- |
| `zdbk.list-courses` | `{"category":"MyCategory","start":1,"end":1}` | Course array |
| `zdbk.list-sections` | `{"course":"<selection code>"}` | Section array |
| `zdbk.list-selected` | `{}` | Selected section array |
| `zdbk.list-graduation` | `{}` | Graduation requirement/course array |
| `zdbk.get-introduction` | `{"course":"<course ID>"}` | Introduction text |

Category/page/course validation, academic form fields, year/semester discovery and response parsing live in `Application/AcademicReader.cs`; the handlers map those results to their published models. Academic form submissions in this reader only retrieve lists; no enrollment or withdrawal is performed. An introduction can contain upstream markup: render it as text unless the consumer sanitizes it.

`zdbk_sign_in_required` reports a missing account connection, `zdbk_account_changed` rejects a concurrent connection change, and `zdbk_session_unavailable` reports failed site-session verification. Invalid category/page/course requests and malformed upstream data produce business failures without returning partial lists. The old `zjuam.read-resource` capability is not used.
