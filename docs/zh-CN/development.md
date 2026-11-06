# 开发指南

[English](../en/development.md) · [文档入口](README.md)

需要 .NET 10、Git、Python 3 和 PowerShell 7。初始化研发规范[子模块](../../.standards/docs/glossary.md#submodule)。生产项目和单元测试可独立构建，不依赖相邻仓库或私有包源：

```sh
git submodule update --init --recursive
dotnet restore Quantum.Zju.Plugin.Zdbk.slnx
dotnet format Quantum.Zju.Plugin.Zdbk.slnx
dotnet build Quantum.Zju.Plugin.Zdbk.slnx -c Release --no-restore
dotnet test Quantum.Zju.Plugin.Zdbk.slnx -c Release --no-build
pwsh .ci/Build.ps1
pwsh .ci/Verify-Host.ps1
```

维护者的 Windows 环境中所有 dotnet 命令须提权执行。`.ci/Build.ps1` 检查格式并生成 `artifacts/quantum.plugin.zdbk.zip`，不会自动发布。`.ci/Verify-Host.ps1` 按 `.ci/*-revision.txt` 从公开仓库获取固定 Desktop 和 CAS 提交，放入忽略的 `.ci-dependencies`；这些源码引用仅供宿主集成测试，生产代码使用已发布 SDK 包。不得隐式更新固定提交。

源码位于 `src/Quantum.ZdbkPlugin`，测试按根目录、Application、Contract 等技术职责对应组织。单元测试仅用虚构数据；宿主集成测试覆盖能力发现、缺失 CAS 依赖、未登录错误、真实宿主 JSON 边界上的业务读取和重新加载，不得将真实学生响应作为测试夹具提交。

真实只读检查时，先通过 CAS 插件在学校官方页面登录，再调用[能力契约](capabilities.md)中的接口，核对课程名、学分、已选教学班、上课时间地点、教学班列表及账号可用的毕业审核信息。空列表不能证明成功读取了非空课表。不得改变选课状态；凭据、站点会话、原始教务响应和每次运行结果均留在 Git 之外，测试后释放会话，禁止关闭证书验证。

发布前审核源码及产物，文档不含本机路径、工作流水账、单次任务证据或内部源码。构建统一映射源码路径，分发包不包含调试符号。

初始教务实现由产品研发团队作为 Quantum.OfficialPlugins 的 Zdbk 组件维护。合成夹具是按上游结构编写的虚构示例，不是学生记录；应保留原著作权声明和已发布的兼容性契约。
