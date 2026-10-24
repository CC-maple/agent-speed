本專案基於 [zoefix/agent-speed](https://github.com/zoefix/agent-speed) 上游 1.3.0，提供尚未正式發布的 Windows 離線 fork，目前 Traffic Helper 為 1.0.1。請參閱 [版本變更](CHANGELOG.md)及 [Windows 使用說明](windows/OFFLINE.md)。macOS 原始碼未修改。

<div align="center">

<img src="assets/icon/icon-1024.png" width="128" alt="Agent Speed 圖示">

# Agent Speed

**看見你的 AI agent 全速奔跑。** 為 Codex、Claude Code、Claude Desktop、DeepSeek Harness、OpenCode、Gemini CLI、omp、Antigravity、WorkBuddy 與 WorkBuddy AI 打造的原生即時 token 速度儀表——閒置時安靜，衝到紅線時全面燃燒。

[English](README.md) · [繁體中文](README.zh-TW.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md)

<img src="docs/images/hero.png" width="760" alt="Agent Speed 懸浮窗與選單列">

</div>

## Windows 離線 fork

- Windows 移除自動及手動更新、線上價格與匯率同步、即時回報和相關介面入口。
- 保留本機記錄監控、token 速度、歷史與費用計算。價格使用內建表；agent 可透過 PowerShell 指令碼更新本機匯率檔案，重新啟動後生效。
- 面板新增重新啟動按鈕及預設關閉的啟用 agent 流量選項。變更後需重新啟動，狀態區分需安裝、連線中、已啟用與連線異常；整機網速獨立保留。
- 選用的 Helper 1.0.1 以本機 Windows 服務執行。從主程式安裝需管理員授權，成功後建立桌面服務管理捷徑；內嵌 Helper 不代表已安裝服務。
- 根目錄 `VERSION` 保留上游 1.3.0，本機 Windows 開發版本顯示 0.0.0。發布前請閱讀 [CHANGELOG.md](CHANGELOG.md)。下方 macOS 安裝指令仍對應上游。

## 安裝

**macOS 14 以上**（Apple Silicon 與 Intel）——貼到「終端機」：

```sh
curl -fsSL https://raw.githubusercontent.com/zoefix/agent-speed/main/install.sh | sh
```

**Windows 離線 fork**（.NET 10 SDK；已驗證 x64）——在本儲存庫目錄建置獨立程式：

```powershell
dotnet publish windows\src\AgentSpeed\AgentSpeed.csproj -c Release -r win-x64 --self-contained true -o dist\windows-offline-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false
```

內嵌 Helper、C++／Windows SDK 工具鏈及手動安裝服務步驟見 [Windows 使用說明](windows/OFFLINE.md)。上游 `install.ps1` 下載上游版本，不會安裝本 fork；上方 macOS 指令仍為上游安裝器。

<details>
<summary>解除安裝</summary>

```sh
curl -fsSL https://raw.githubusercontent.com/zoefix/agent-speed/main/install.sh | sh -s -- --uninstall
```

本 Windows fork 使用本機發布程式。清理可攜版本前，可在主程式的網路流量設定移除選用的輔助服務，詳見 [Windows 使用說明](windows/OFFLINE.md)。

上游 macOS 解除安裝程式可加上 `--purge` 同時移除設定。Windows fork 的服務管理方式見上述說明。
</details>

## 功能

- **懸浮窗**——煙燻玻璃質感的儀表，36 格 LED 錶盤。速度一路攀升時，稱號也跟著升級——從「步行」「自行車」到「高鐵」「戰鬥機」，衝到紅線就是「火箭」；錶盤沿著熱度光譜點亮、彗星繞圈疾馳、火花四濺、餘燼升騰，每完成一次回應就釋放一道衝擊波；打破今日紀錄還會慶祝。
- **五種懸浮窗樣式，想開幾個就開幾個**——「LED 儀表」「指針轉速錶」「能量液球」「反應爐」「極簡圓環」。每個懸浮窗都能自由組合來源（全部、單一工具，或 Claude Code＋Codex），下方的玻璃名牌在任何桌布上都清楚好讀。
- **選單列／工作列**——精簡的儀表與數字，寬度固定不跳動；可顯示 tok/s、金額，或 tok/s 加上即時花費與今日總額，也能加上網路速度。Windows 版直接嵌在工作列裡、時鐘旁邊。
- **面板**——圓形儀表裡有一台跟著速度稱號變化的發光線條載具：從步行、蹬著自行車出發，一路換到紅線時的火箭；另有即時曲線、各來源速度、所有正在工作的 agent（含子 agent）與其狀態、今日統計。點一下來源就能打開它的用量歷史。
- **費用**——保留 API 等值即時及每日費用、訂閱來源排除和 38 種顯示貨幣。Windows 使用內建模型價格及僅啟動時讀取的本機匯率檔案，agent 更新後需重新啟動；未修改的 macOS 保留上游線上同步方式。
- **網路流量**——各 agent 或整台電腦的上傳／下載速度，以兩行小字顯示在懸浮窗上，選單列／工作列與面板也看得到，用量歷史記錄每天的流量。Windows 若要看各 agent 的流量，需要安裝一次選用的小型輔助服務（需系統管理員同意）。
- **稱號主題**——九套速度稱號：「交通工具」「動物」「蒲福風級」「宇宙」「修真境界」「武學境界」「魔法位階」「遊戲段位」「工程師職涯」，也能自訂最多 8 組。選交通工具以外的主題時，面板圓圈會顯示發光的稱號徽章。
- **用量歷史**——從最早的紀錄開始，每天的輸出與輸入 token、請求數、平均速度與峰值、費用與網路流量，可依來源，以今天、昨天或任意日期區間繪成圖表，並看各模型的速度、匯出 CSV。只保存每日彙總，即使工具刪除了舊紀錄，歷史也不會消失。
- **即時上報（僅 macOS，選用）**——預設關閉；想參與的話，可以用你取的使用者名稱，把即時數字分享給你選的 API，例如 [Codex Radar](https://codexradar.com/) 的速蹬天梯。從不傳送對話內容、檔案或專案名稱，詳見[隱私](#隱私)。
- **全原生**——macOS 使用 SwiftUI + AppKit，Windows 使用 WPF 與 Fluent／Mica。淺色、深色、自動主題；繁體中文、简体中文、English、日本語。
- **人性化**——特效強度從「關閉」到「熱血全開」，遵循「減少動態效果」與高對比設定，支援滑鼠穿透、邊緣磁吸、記住每個螢幕上的位置、登入時自動啟動。
- **更新（僅 macOS）**——macOS 保留上游 GitHub Releases 檢查及更新安裝功能；Windows fork 不包含更新檢查或下載器。

<p align="center"><img src="docs/images/orb-styles.png" width="760" alt="五種懸浮窗樣式：LED 儀表、指針轉速錶、能量液球、反應爐、極簡圓環"></p>

<p align="center"><img src="docs/images/orbs.png" width="760" alt="多個懸浮窗，各自追蹤不同來源並附玻璃名牌"></p>

<p align="center"><img src="docs/images/hero-vehicle.gif" width="480" alt="面板圓圈裡的載具，從自行車到火箭"></p>

<p align="center"><img src="docs/images/dashboards.png" width="560" alt="深色（tok/s）與淺色（每秒花費）面板，含今日費用與網路流量"></p>

<p align="center"><img src="docs/images/history.png" width="760" alt="用量歷史視窗，含每日輸出與費用"></p>

## 速度怎麼算

Agent Speed 只讀取 agent 本來就寫在你電腦上的紀錄：

| 來源 | 資料夾 |
| --- | --- |
| Codex（CLI、IDE、桌面版） | `~/.codex/sessions` 與 `archived_sessions`，含壓縮紀錄（或 `$CODEX_HOME`） |
| Claude Code（CLI、IDE） | `~/.claude/projects`（或 `$CLAUDE_CONFIG_DIR/projects`） |
| Claude Desktop（Code 與 agent 工作階段） | `~/.claude/projects`，自動區分 |
| DeepSeek Harness（`dsh`，桌面版與命令列） | `~/.dsh/sessions`（或 `$DSH_HOME/sessions`） |
| OpenCode（命令列與桌面版） | `~/.local/share/opencode/opencode.db`（或 `$XDG_DATA_HOME/opencode`） |
| Gemini CLI | `~/.gemini/tmp`（或 `$GEMINI_CLI_HOME/.gemini/tmp`） |
| omp（oh-my-pi） | `~/.omp/agent/sessions`，以及 `~/.omp/profiles` 裡的每個設定檔（或 `$PI_CODING_AGENT_DIR/sessions`） |
| Antigravity（`agy` 命令列、桌面版、IDE 與擴充功能） | `~/.gemini/antigravity-cli`、`~/.gemini/antigravity`、`~/.gemini/antigravity-ide`（其中的 `conversations` 資料夾） |
| WorkBuddy（騰訊，中國版） | `~/.workbuddy/projects` |
| WorkBuddy AI（國際版） | `~/.workbuddy-ai/projects` |

**速度＝輸出 token ÷ 模型實際花在這些 token 上的時間**——從送出請求到回應完成，含等待第一個字。多個 agent 平行時會加總。回應還在進行中時，先以該 agent 近期的實測速度估算，完成的瞬間立即以實測值校正。也可以切換成「總吞吐量」（輸入＋輸出）。

完整且可測試的規格見 [`spec/METRICS.md`](spec/METRICS.md)。兩個平台的程式都以同一份測試資料，以及一份獨立的[參考實作](spec/tools/reference.py)交叉驗證。

Claude Desktop 一般聊天對話不會存在本機，因此無法統計；WorkBuddy 在雲端執行的對話也不會計入。

## 隱私

### Windows fork

Windows 應用程式不主動發出 HTTP 請求，選用的本機流量統計仍保留。Helper 只為排除回環流量而讀取事件位址欄位，不儲存位址，也不經本機管道傳送位址。權限與驗證限制見 [安全複核](windows/TRAFFIC-HELPER-SECURITY.md)。

### 上游隱私說明

以下原始說明適用於未修改的 macOS 及上游 Windows 版本，包括其選用聯網功能；本 Windows fork 已移除這些聯網功能。

唯讀、只在本機運作，沒有遙測、不需帳號。Agent Speed 只會自動發出三種網路請求，而且都不會傳送任何關於你的資料：向 GitHub Releases 檢查更新、每天從本專案下載公開的模型價格表（GitHub，以 jsDelivr 為備援），以及只有在你選擇以美元以外的貨幣顯示費用時，每天下載公開匯率（open.er-api.com，以 fawazahmed0 currency API 和 Frankfurter 為備援）。每位使用者下載的都是同一份檔案，所以伺服器無從得知你的貨幣、模型或用量。設定裡的一個開關就能關閉所有網路連線，此時費用改用 App 內建的價格表與匯率。Agent Speed 從不儲存或顯示對話內容，只使用 token 數量、模型名稱和專案資料夾名稱。用量歷史只保存每日總計（含每日的 API 等值費用），存在你電腦上的一個小型資料庫（`~/Library/Application Support/AgentSpeed`、`%LOCALAPPDATA%\AgentSpeed`），可在設定中清除。更新在安裝前會以該版本發布的 SHA-256 校驗碼驗證；校驗碼與版本一起發布，因此能攔下損壞或遭竄改的下載檔，但無法防範儲存庫本身遭入侵。價格表是依嚴格規則檢查的純資料，沒有數位簽章。

網路流量：Agent Speed 只在本機計算各程式收發的位元組數，從不讀取內容、網址或連線對象；只有在你開啟即時上報（見下文）時，才會把 agent 的位元組數傳送給你選擇的服務。在 Windows 上，要顯示各 agent 的流量需要安裝選用的輔助服務；它以權限受限的「本機服務」帳戶執行，只統計每個程式的位元組數，安裝和移除各需要一次系統管理員確認（[詳細說明](spec/WINDOWS-SERVICE.md)）。

即時上報（選用，預設關閉；面板會詢問一次）：為某個 API（例如公開排行榜服務，像是內建的 Codex Radar 速蹬天梯 `deng.codexradar.com`）開啟後，Agent Speed 會把你的即時速度、最近幾秒用掉的 token 數，以及你保留的項目——以美元計的 API 等值費用、agent 的網路流量、使用中的模型、各工具的明細——連同你選擇的使用者名稱（預設是電腦名稱，一鍵就能換成隨機名稱）傳送給該服務。agent 工作時每隔幾秒上報一次，閒下來就停止。它從不傳送對話內容、提示詞、檔案／資料夾／專案名稱或用量歷史，上報失敗的資料也不會補傳。每筆上報都以只存在你電腦上的金鑰簽章；服務能確認資料來自同一台裝置，但無法確認數字是否正確。可以在設定中暫停、關閉，或要求服務忘記你的裝置。

Agent Speed 可能連線的所有主機與時機，都列在 [`spec/NETWORK.md`](spec/NETWORK.md)。

## 從原始碼建置

<details>
<summary>macOS（Xcode 26 以上）</summary>

```sh
cd macos
swift test
./scripts/build-app.sh          # → dist/AgentSpeed-macOS.zip
```
</details>

<details>
<summary>Windows（.NET 10 SDK；也能在 macOS／Linux 交叉建置）</summary>

```sh
dotnet test windows/tests/AgentSpeed.Core.Tests
dotnet test windows/tests/AgentSpeed.Presentation.Tests
dotnet publish windows/src/AgentSpeed/AgentSpeed.csproj -c Release -r win-x64 --self-contained true -o dist/windows-offline-x64
```
</details>

## 友情連結

[Codex Radar](https://codexradar.com/)

## 授權

[MIT](LICENSE) © 2026 zoefix
