# Fork 版本说明

本文件记录 CC-maple/agent-speed 相对上游 zoefix/agent-speed 的改动。
上游的功能介绍、MIT 许可证和原作者版权声明继续保留；本 fork 的主要修改范围是 Windows。
macOS 源码未修改，不应将下列 Windows 改动理解为上游或 macOS 的发布说明。

## 尚未发布：Windows 离线 fork（2026-10-24）

| 版本项 | 值与含义 |
| --- | --- |
| 上游基线 | `zoefix/agent-speed` 1.3.0，基线提交 `688d8f931d062800cdbfabbdfe73b00efe4f4123` |
| 根目录 `VERSION` | 保留上游的 `1.3.0`，不表示本 fork 已发布同名离线版 |
| Windows 本地开发构建 | `0.0.0`，来自 `windows/Directory.Build.props`；正式发布脚本可以另外写入版本 |
| Traffic Helper | `1.0.1`，文件版本 `1.0.1.0`；管道协议仍为 1，最低兼容 Helper 版本仍为 1.0.0 |
| 发布状态 | 尚未为本 fork 指定正式发布版本或发布标签 |

### 相对上游新增与调整

- Windows 移除自动和手动更新、在线模型价格与汇率同步、实时上报，以及对应界面、菜单和外部网站入口。
  相关模块通过项目配置排除；保留原文件，未重写 macOS 实现。
- 保留本地日志监控、token 速度、用量历史和费用计算。模型价格使用内置表，汇率使用本地 JSON。
- 支持 `AGENT_SPEED_RATES_FILE` 和 PowerShell 更新脚本。汇率仅在每次启动时读取一次，agent 更新后需重启。
- 面板底部新增重启按钮，退出前保存设置，释放单实例锁后启动新进程。
- 新增启用 agent 流量选项，默认关闭，变更后重启生效；不影响整机网速。
  提示区分已关闭、需重启、需安装、连接中、已启用与连接异常。
- Helper 是可选的本机 Windows 服务，不负责实时上报；主程序可选择构建为不带 Helper 或内嵌 Helper 的版本。
- 从主程序安装 Helper 成功后，在当前用户桌面创建 `Agent Speed Traffic Helper.lnk`，
  打开 Windows 服务管理器，便于查找 `AgentSpeedTraffic`。不覆盖同名无关文件，不设置管理员运行标记。
- `.gitignore` 排除本机编辑器配置、临时脚本、测试报告和测试运行数据。

### Helper 1.0.1 相对 1.0.0

- TDH 元数据读取在 API 返回后再次核对实际使用长度与分配容量。
- 客户端查询权限设置失败时终止服务启动，不再继续进入客户端无法验证身份的运行状态。
- 版本增加用于识别此次防御性加固，不代表已经确认旧版存在可利用的高危漏洞。
- 没有增加外部网络请求，没有更改管道协议，也没有将所有旧版 Helper 判定为禁止运行。

### 配置与升级说明

- 旧配置中的 `network`、`updates`、`report` 和 `cost.syncPrices` 不再读取或保存。
- 新配置项为 `traffic.agentTrafficEnabled`，缺省为 false；升级后需主动启用各 agent 流量。
- 内嵌 Helper 不等于服务已经安装。从带 Helper 的新版主程序进入流量设置并手动安装，需要管理员授权。
- 服务与主程序是独立进程；服务没有客户端连接达到 120 秒后自动退出。
- 桌面快捷方式由主程序在成功安装后创建；直接运行独立 Helper 的安装命令不经过此步骤。
- 本 fork 尚未调整上游 GitHub Actions 发布流程。推送 `v*` 标签会触发原发布流程，正式发布前需检查平台和 Helper 打包选项。

### 验证与安全边界

本次验证：核心 1,616 项、表现层 3,613 项、Windows 17 项、Helper 164 项，合计 5,410 项通过。
内嵌 Helper 提取后的文件与独立产物、内嵌 SHA-256 一致。
安全审查、NuGet 查询和测试不能证明没有漏洞；完整的运行时权限与跨会话验证尚未完成。
二进制目前没有发布者数字签名。

使用、构建和手动安装步骤见 [Windows 本地版本说明](windows/OFFLINE.md)，
详细安全复核见 [Traffic Helper 安全复核](windows/TRAFFIC-HELPER-SECURITY.md)。
