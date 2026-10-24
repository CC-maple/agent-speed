# Traffic Helper 安全复核

复核日期：2026-10-24。范围为当前 Windows 源码、安装流程、进程间通信、数据采集与新桌面快捷方式。
本次没有安装或启动系统服务，没有触发 UAC，没有向外部扫描服务上传源码或程序。

## 结论与边界

在本次检查范围内未确认高危漏洞，但源码复核和自动化测试不能证明没有漏洞。
安装后真实的服务账户、文件和注册表 ACL、ETW 权限、管道远程拒绝及跨会话行为，
仍需要在手动安装后的环境中验证。原生内存代码没有经过完整模糊测试或独立渗透测试。

辅助服务版本由 1.0.0 更新为 1.0.1，以便界面识别此次加固的二进制。
没有将旧版判定为已确认存在高危问题，最低兼容版本保持不变。

## 已检查的安全边界

| 项目 | 源码行为与证据 |
| --- | --- |
| 安装授权 | `AgentSpeed.TrafficHelper/Install/Installer.cs` 检查管理员令牌、用户 SID 与请求者身份，再验证自身 SHA-256。 |
| 文件安装 | `InstallFolder.cs` 拒绝重解析点和不受信任的目录所有者、可被非管理员写入的目录；安装在 Program Files 下。 |
| 提权前暂存 | `AgentSpeed.Core/Traffic/Windows/HelperInstaller.cs` 创建仅当前用户、SYSTEM、管理员可访问的暂存目录，核验哈希并持有禁止写入和删除共享的文件句柄。 |
| 服务权限 | `ServiceConfigurator.cs` 设置按需启动、LocalService、加引号的固定二进制路径；保留 SeChangeNotifyPrivilege，允许用户不获得更改服务配置的权限。 |
| DLL 加载 | Helper 的默认 DLL 搜索目录限制为 System32；NativeAOT 发布配置也限制本机 DLL 搜索。 |
| 管道访问 | 显式 DACL、拒绝远程客户端、首个管道实例保护；客户端权限不包含创建管道实例的权限。 |
| 身份校验 | `HelperClient.cs` 将管道服务端 PID 与 SCM 查询结果和安装路径比对，并在校验后再次查询服务 PID。 |
| 数据隔离 | 服务端根据系统返回的客户端会话 ID 筛选数据，拒绝会话 0；每会话最多两条连接，总共四个管道实例。 |
| 资源边界 | 固定请求格式、请求限速、读写超时；进程表、孤立事件表和布局缓存均有容量限制。 |
| 空闲退出 | 没有客户端连接达到 120 秒后，服务自动退出。 |
| 桌面入口 | 安装成功后由未提权的主程序创建普通 Shell Link，目标固定为 System32 中的 mmc.exe，参数固定为 services.msc。不会执行脚本、Helper 安装指令或主程序更新逻辑。 |

## 数据与信任边界

- 服务采集进程 ID、创建时间、父进程与会话信息以及收发字节计数。
- Kernel-Network 事件本身包含地址字段。`TrafficEvents.cs` 读取这些字段仅用于排除回环地址，
  不保存地址，也不在管道协议中传输地址。这与完全不接触地址字段不同。
- 未发现 HTTP 客户端或主动建立外部网络连接的业务代码。通信接口是本机命名管道。
- 被允许的用户身份下运行的其他普通进程，也属于该 SID 的信任范围；管道没有为每个应用另设密钥。
  这不是与同用户恶意软件之间的完整隔离机制。
- 内嵌 SHA-256 能验证 Helper 与主程序打包的数据一致，不能替代发布者数字签名。
  程序未添加 Authenticode 签名，UAC 的发布者身份不能靠内嵌哈希证明。

## 本次加固与改动

1. TDH 原生缓冲区保留分配容量，在第二次 API 调用成功后再次验证返回长度，
   拒绝小于结构头或大于分配容量的结果。属于防御性边界检查，未证明存在可触发的越界漏洞。
2. `ServiceRuntime` 不再忽略 `GrantQueryLimited` 的返回值。权限设置失败时返回错误并停止启动，
   避免服务处于运行状态但所有客户端都无法完成身份验证。此前客户端会拒绝连接，未发现权限绕过。
3. 安装成功后创建桌面服务管理快捷方式；不覆盖同名无关文件，不设置 RunAsUser 标志。
   创建失败会明确提示服务已安装但快捷方式失败，不伪报整个安装失败。

## 验证

- Helper 测试覆盖 ACL、SID、畸形请求、限速、会话隔离、进程列表与事件解析、计数和空闲退出。
- 新 Windows 测试通过真实 Shell Link COM 接口创建和重新读取临时目录中的快捷方式，
  核对固定系统目标、参数、服务名和未提权标志，确认重复创建和同名文件冲突不会覆盖已有文件。
- NuGet 漏洞查询针对 Helper 和主程序，包含传递依赖；本次查询源为 api.nuget.org，未报告已知易受攻击的包。
  该结果不包含所有 Windows 组件或 NativeAOT 静态运行时的漏洞审计。
- 发布后应提取内嵌 Helper，核对源文件、提取文件和内嵌 SHA-256 三者一致。

后续清理检查中，只读确认了当前运行的是复核版主程序，`AgentSpeedTraffic` 服务正在运行，
已安装 Helper 的版本为 1.0.1.0，其 SHA-256 与保留的编译产物一致，桌面快捷方式已存在。
该检查没有安装、启动、停止或修改服务，也未替代完整的运行时权限和跨会话验证。

参考：[Windows 命名管道安全](https://learn.microsoft.com/en-us/windows/win32/ipc/named-pipe-security-and-access-rights)、
[Shell Links](https://learn.microsoft.com/en-us/windows/win32/shell/links)、
[TdhGetEventInformation](https://learn.microsoft.com/en-us/windows/win32/api/tdh/nf-tdh-tdhgeteventinformation)。
