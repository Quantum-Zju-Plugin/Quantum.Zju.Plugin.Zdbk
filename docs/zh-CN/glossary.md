# 术语表

[English](../en/glossary.md) · [文档入口](README.md)

文档首次出现行话或缩写时须解释并链接定义，遵循[研发规范术语表](../../.standards/docs/glossary.md)。

## CAS

Central Authentication Service / 中央认证服务：学校使用的统一认证协议，本插件消费外部 CAS 账号提供者。

## RPC

Remote Procedure Call / 远程过程调用：通过 Quantum 宿主按能力名分发的请求，使用可序列化的 JSON 契约。

## Service ticket

服务票据：绑定单一 CAS 服务、短期且只能使用一次的凭据。立即消费，禁止记录或持久化。

## Site session

站点会话：由本插件持有的教务站点 Cookie 会话，账号连接变化后失效。

## User-Agent

用户代理标识：随服务票据提供的浏览器身份，教务认证请求须沿用。

## HTTP

Hypertext Transfer Protocol / 超文本传输协议。HTTPS 增加传输加密和服务器证书认证，集成必须使用 HTTPS。
