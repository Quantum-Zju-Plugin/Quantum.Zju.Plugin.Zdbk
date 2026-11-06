# Glossary

[简体中文](../zh-CN/glossary.md) · [Documentation](README.md)

Explain jargon and abbreviations on first use and link to their definition, as required by the [standards glossary](../../.standards/docs/glossary.md).

## CAS

Central Authentication Service / 中央认证服务. The university sign-in protocol. This plugin consumes an external CAS account provider.

## RPC

Remote Procedure Call / 远程过程调用. A named request dispatched through the Quantum host, using a JSON-serializable contract.

## Service ticket

服务票据. A short-lived, single-use credential bound to one CAS service. Consume it immediately; never log or persist it.

## Site session

站点会话. The academic website's private cookie-based session, owned by this plugin and invalidated when the account connection changes.

## User-Agent

用户代理标识. Browser identity supplied with a ticket; academic authentication requests preserve it.

## HTTP

Hypertext Transfer Protocol / 超文本传输协议. HTTPS adds encrypted transport and authenticated server certificates; integrations require HTTPS.
