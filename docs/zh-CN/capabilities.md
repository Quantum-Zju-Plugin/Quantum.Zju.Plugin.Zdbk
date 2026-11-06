# 浙江大学本科教务

[English](../en/capabilities.md) · [文档入口](README.md)

Zdbk 0.4 使用[浙大 CAS 账号提供者](https://github.com/XmmShp/Quantum.Zju.Plugin.Cas)的 `zju.cas.account` 与 `zju.cas.service-ticket`。调用业务能力前，先通过账号设置登录。提供者只处理 [CAS](glossary.md#cas) 认证；教务业务请求、私有[站点会话](glossary.md#site-session)和响应解析均由 Zdbk 持有。

## 会话边界

`AcademicSession` 读取当前连接元数据，为教务回跳地址获取[服务票据](glossary.md#service-ticket)，再用独立的框架 Cookie 容器消费票据。它保留返回的 [User-Agent](glossary.md#user-agent)，仅通过 [HTTPS](glossary.md#http) 跟随批准的教务地址，不获取提供者的主会话 Cookie。教务服务器根据登录会话确定用户；插件不收集学号，也不在业务查询参数里指定学号。

提供者的连接标识不变时复用站点会话。每次读取都在业务请求前后检查连接标识；下次观察到重连、断开或账号变化时，丢弃旧站点客户端。读取过程中发生变化时拒绝返回旧账号的数据。插件释放时也会清理客户端。站点会话失效后最多通过新票据重建一次，不触发密码表单，也不无限重试。提供者断开不代表执行学校全局退出。

## 业务能力

对外 [RPC](glossary.md#rpc) 名称与响应字段保持不变：

| 能力 | 请求 | 响应 |
| --- | --- | --- |
| `zdbk.list-courses` | `{"category":"MyCategory","start":1,"end":1}` | 课程数组 |
| `zdbk.list-sections` | `{"course":"<选课代码>"}` | 教学班数组 |
| `zdbk.list-selected` | `{}` | 已选教学班数组 |
| `zdbk.list-graduation` | `{}` | 毕业要求及课程数组 |
| `zdbk.get-introduction` | `{"course":"<课程 ID>"}` | 简介文本 |

类别、页码和课程参数校验、教务表单、学年学期发现及响应解析位于 `Application/AcademicReader.cs`，处理器负责映射为已发布的模型。读取器中的表单提交只查询列表，不选课或退课。简介可能含上游标记，应按纯文本显示，除非消费方自行完成清理。

缺少账号连接返回 `zdbk_sign_in_required`；读取过程中连接变化返回 `zdbk_account_changed`；站点会话验证失败返回 `zdbk_session_unavailable`。无效类别、页码、课程参数或上游数据异常均返回业务失败，不返回不完整列表。不再调用 `zjuam.read-resource`。
