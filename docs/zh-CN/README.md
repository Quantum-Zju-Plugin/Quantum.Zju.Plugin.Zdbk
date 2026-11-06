# Quantum.Zju.Plugin.Zdbk

[English](../en/README.md)

Quantum 的浙江大学本科教务只读集成，采用 [Apache-2.0](../../LICENSE)。遵循固定提交的[简体中文研发规范](../../.standards/docs/zh-CN/README.md)，参见[开发指南](development.md)、[能力契约](capabilities.md)和[术语表](glossary.md)。

先在设置的账号页通过独立的[浙大 CAS 账号插件](https://github.com/XmmShp/Quantum.Zju.Plugin.Cas)登录。本插件负责教务站点请求、私有站点会话和数据解析，不收集密码、不导出 CAS Cookie，也不执行选课或退课。

为兼容既有消费方，插件标识保留 `quantum.plugin.zdbk`，程序集保留 `Quantum.ZdbkPlugin.dll`，CLR 命名空间保留 `Quantum.OfficialPlugins.Zdbk`。仓库更名不改变现有能力名和传输契约。依赖已发布 SDK `0.3.0-rc.3` 及 `zju.cas.account`、`zju.cas.service-ticket` 能力。

已选教学班列表提供上游的上课时间、地点字符串，供课表消费方使用。学年、学期由教务站点当前页面决定，不支持任意指定学期；上游未提供的排课信息不会被补造。日历转换与同步由消费方负责。

## 仓库工作流

使用短期[主题分支](../../.standards/docs/glossary.md#topic-branch)，通过[合并请求](../../.standards/docs/glossary.md#change-request)进入 main，管理员也遵守。个人仓库配置不强制另一位维护者批准或托管 [CI](../../.standards/docs/glossary.md#ci-cd) 门禁；必须运行相关检查并如实说明托管任务的限制。
