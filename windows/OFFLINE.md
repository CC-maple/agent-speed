# Windows 本地监控版本

`remove-network-function` 分支的 Windows 程序仅使用本地日志和本地数据。
macOS 源码不变。

本 fork 基于上游 1.3.0（提交 `688d8f9`），当前尚未发布正式离线版。
根目录 `VERSION` 保留上游版本，本地开发构建显示 0.0.0，当前内嵌 Helper 为 1.0.1。
版本含义与相对上游的改动见 [Fork 版本说明](../CHANGELOG.md)。

Windows 构建已移除自动和手动更新、模型价格在线同步、汇率在线同步、实时上报，
以及对应的设置页、网络总开关、菜单、排行榜和外部网站入口。
旧 `settings.json` 中的 `network`、`updates`、`report` 和 `cost.syncPrices` 不再读取或保存。
费用计算保留，模型价格使用应用内置价格表。

联网模块及其专用测试通过项目文件排除，不进入 Windows 构建；原文件保留在仓库中。
核心引擎仍保留本地计数用的数据结构，不包含 HTTP 传输和上报客户端。

面板网络流量区域的启用 agent 流量选项默认关闭，仅控制各进程流量统计，不影响整机网速。
勾选或取消后显示需重启，点击底部重启按钮后生效。未安装辅助服务时显示需安装并提供安装入口；
只有服务连接正常、已进入采样或待机状态时才显示已启用。安装服务需要 Windows 管理员授权。

从 Agent Speed 中手动安装或更新辅助服务成功后，主程序会在当前用户桌面创建
`Agent Speed Traffic Helper.lnk`，打开 Windows 服务管理器。服务名为 `AgentSpeedTraffic`。
该快捷方式不直接执行 Helper，不设置管理员运行标记，且不覆盖同名的无关文件。
快捷方式由未提权的主程序创建，使用当前用户的桌面，不写入提供 UAC 凭据的管理员桌面。
如果创建失败，界面会明确提示服务已安装但快捷方式创建失败。
快捷方式是服务管理入口，卸载服务后可按需要自行删除。

源码安全复核结果和验证边界见 [Traffic Helper 安全复核](TRAFFIC-HELPER-SECURITY.md)。

## agent 更新汇率

默认汇率文件：

```text
%LOCALAPPDATA%\AgentSpeed\rates\local-rates.json
```

第一次启动时，程序将内置汇率完整写入默认文件。也可以先用下面的脚本初始化文件。
这份初始数据是随源码提供的快照，日期保留为原始日期，不会宣称已获取最新汇率。

agent 可以使用 PowerShell 7 调用：

```powershell
.\windows\scripts\update-local-rate.ps1 -Currency CNY -Rate 7.25
```

这里的 `7.25` 只是用法示例，表示每 1 USD 对应 7.25 CNY，调用者应提供实际需要的汇率。
脚本只修改指定币种，保留其余币种和原来的日期。只有整张表已经更新到同一天时，
才应添加 `-Date 2026-10-24` 更新表日期。脚本不联网。

可使用 `-RatesFile C:\absolute\path\rates.json` 指定文件；程序使用环境变量选择该文件：

```powershell
$env:AGENT_SPEED_RATES_FILE = 'C:\absolute\path\rates.json'
& '.\dist\windows-offline-x64\AgentSpeed.exe'
```

环境变量必须在启动前设置，路径必须是绝对本地路径。显式指定的文件必须已经存在。
程序仅在每次启动时读取所选文件一次，不在运行中监视或重新读取文件。
agent 更新汇率后，必须完全退出 Agent Speed 并重新启动，新汇率才会用于费用显示。
关闭设置窗口或隐藏悬浮窗不会重新加载汇率。
可以点击面板底部设置与退出按钮之间的重启按钮；程序会先保存设置、退出当前会话，
释放单实例锁后重新启动，并读取更新后的汇率。
更新不改写已有历史中的美元费用，只影响显示货币的换算。

也可以直接维护 JSON。文件需要包含完整币种表，下面仅展示结构，省略号不能写入文件：

```text
{
  "base": "USD",
  "date": "2026-10-22",
  "rates": {
    "USD": 1,
    "CNY": 7.25,
    ...其余所有支持的币种...
  }
}
```

`base` 必须为 USD，USD 汇率必须为 1。币种代码大写，汇率是大于 0、
不超过 1000000000000 的 JSON 数值。缺少币种、重复键、未知字段、无效日期或格式错误会被拒绝。
运行中修改文件不影响本次启动已加载的汇率。下次启动遇到无效文件时，启动会报错，
不会静默换用其他数据。修复文件后需再次启动程序。

## 构建和测试

需要默认路径安装的 .NET 10 SDK。以下发布命令仅生成 Windows x64 主程序，不构建或嵌入 Traffic Helper：

```powershell
dotnet publish windows\src\AgentSpeed\AgentSpeed.csproj -c Release -r win-x64 --self-contained true -o dist\windows-offline-x64 -p:TrafficHelperDir= -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:DebugType=none -p:DebugSymbols=false
dotnet test windows\tests\AgentSpeed.Core.Tests -c Release
dotnet test windows\tests\AgentSpeed.Presentation.Tests -c Release
dotnet test windows\tests\AgentSpeed.Windows.Tests -c Release
```

NuGet 依赖还原仍可能联网；发布后的应用不发起网络请求。安装脚本及仓库 CI 属于开发分发工具，
不属于应用运行时的联网功能。

需要内嵌 Helper 时，先发布 Helper，再将输出根目录传给主程序：

```powershell
dotnet publish windows\src\AgentSpeed.TrafficHelper\AgentSpeed.TrafficHelper.csproj -c Release -r win-x64 -o dist\helper\win-x64
$helperRoot = (Resolve-Path -LiteralPath 'dist\helper').Path
dotnet publish windows\src\AgentSpeed\AgentSpeed.csproj -c Release -r win-x64 --self-contained true -o dist\windows-offline-with-helper-x64 "-p:TrafficHelperDir=$helperRoot" -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false -p:PublishReadyToRun=true -p:DebugType=none -p:DebugSymbols=false
```

Helper 的 NativeAOT 发布还需要 Visual Studio C++ x64 构建工具及 Windows SDK。
这里给出的是已验证的 Windows x64 路径；上游也提供 ARM64 工程配置，但本次未验证 ARM64 发布。

## 手动安装与管理 Helper

1. 完全退出旧主程序，再打开新生成的带 Helper 版本。同名程序的重启按钮只重启原路径，不会切换目录。
2. 在面板勾选启用 agent 流量，点击安装辅助服务，或进入设置的网络流量页安装。
3. 确认自己发起了安装，再手动完成 UAC 管理员授权；本程序未签名，可能显示未知发布者。
4. 安装成功后，桌面会出现 `Agent Speed Traffic Helper` 服务管理快捷方式。
5. 用主程序底部的重启按钮应用流量选项，连接正常后显示已启用。

不需要单独双击 Helper。停止监控可退出主程序；取消流量选项后重启则停止连接 Helper，整机网速可继续显示。
要卸载辅助服务，在带 Helper 的主程序中进入设置的网络流量页，使用移除服务操作并确认管理员授权。
此 fork 的主程序是本地发布产物，没有使用上游安装器注册主程序卸载项；上游 `install.ps1` 下载的是上游版本。
