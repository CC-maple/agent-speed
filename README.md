Windows offline fork of [zoefix/agent-speed](https://github.com/zoefix/agent-speed), based on upstream 1.3.0. This fork is not yet released; Traffic Helper is 1.0.1. See [fork changes](CHANGELOG.md) and [Windows instructions](windows/OFFLINE.md). macOS source is unchanged.

<div align="center">

<img src="assets/icon/icon-1024.png" width="128" alt="Agent Speed icon">

# Agent Speed

**Feel your AI agents run.** A native racing instrument for the live token speed of Codex, Claude Code, Claude Desktop, DeepSeek Harness, OpenCode, Gemini CLI, omp, Antigravity, WorkBuddy and WorkBuddy AI — calm when idle, on fire at the redline.

[English](README.md) · [繁體中文](README.zh-TW.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md)

[![Upstream Release](https://img.shields.io/github/v/release/zoefix/agent-speed?style=flat-square)](https://github.com/zoefix/agent-speed/releases/latest)
![macOS 14+](https://img.shields.io/badge/macOS-14%2B-black?style=flat-square&logo=apple)
![Windows 10/11](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?style=flat-square&logo=windows)
[![License: MIT](https://img.shields.io/badge/license-MIT-green?style=flat-square)](LICENSE)

<img src="docs/images/hero.png" width="760" alt="Agent Speed floating orb and menu bar">

</div>

## Windows offline fork

- Windows removes automatic/manual updates, online price and exchange-rate synchronization, live reporting and their UI entries.
- Local logs, token speed, history and cost calculations remain. Prices are bundled; an agent can update a local rates file using the supplied PowerShell script. Restart to load new rates.
- The dashboard has a restart button and an opt-in agent-traffic checkbox, off by default. Changes require a restart; status distinguishes restart/install required, connecting, enabled and connection failure. Machine traffic remains independent.
- Optional Helper 1.0.1 runs as a local Windows service. Installing it from the app requires administrator approval and creates a desktop service-management shortcut. Embedding the helper does not install it.
- Root `VERSION` retains upstream 1.3.0; local Windows development builds show 0.0.0. Read [CHANGELOG.md](CHANGELOG.md) before publishing. The upstream release badge and macOS installer refer to the upstream project.

## Install

**macOS 14 or later** (Apple Silicon and Intel) — paste into Terminal:

```sh
curl -fsSL https://raw.githubusercontent.com/zoefix/agent-speed/main/install.sh | sh
```

**Windows offline fork** (.NET 10 SDK; x64 verified) — build the standalone app from this checkout:

```powershell
dotnet publish windows\src\AgentSpeed\AgentSpeed.csproj -c Release -r win-x64 --self-contained true -o dist\windows-offline-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false
```

For a build with the optional helper, its C++/Windows SDK requirements and manual installation steps, see [Windows instructions](windows/OFFLINE.md). The upstream `install.ps1` downloads the upstream version, not this fork. The macOS command above remains the upstream installer.

<details>
<summary>Uninstall</summary>

```sh
curl -fsSL https://raw.githubusercontent.com/zoefix/agent-speed/main/install.sh | sh -s -- --uninstall
```

For this Windows fork, remove the optional service from the app’s Network Traffic settings before removing a portable build. See [Windows instructions](windows/OFFLINE.md).

The upstream macOS uninstaller supports `--purge` to remove settings. Windows fork service management is described in the linked instructions.
</details>

## What you get

- **Floating orb** — a smoked-glass instrument with a 36-segment LED gauge. As speed climbs, so does its title — from *Walking* and *Bicycle* through *Bullet Train* and *Fighter Jet* to *Rocket* at the redline — while the gauge sweeps through the heat spectrum, a comet races around the ring, sparks fly, embers rise and every finished response sends out a shockwave. Beat your daily peak and it celebrates.
- **Five orb styles, as many orbs as you like** — *LED Gauge*, *Needle Tachometer*, *Liquid Energy*, *Reactor* and *Minimal Ring*. Give each orb its own mix of sources (all of them, one tool, or Claude Code + Codex) and a glass name plate that stays legible on any wallpaper.
- **Menu bar / taskbar** — a compact gauge and number that never jitters, right where you look: tok/s, money, or tok/s with the live money rate over today's total, plus optional network speed. On Windows it lives inside the taskbar, next to the clock.
- **Dashboard** — a glowing line drawing of your current speed title's vehicle moves inside its orb — a walker, a bicycle pushing off, all the way to a rocket at the redline — plus a live chart, per-source speed, every active agent (including sub-agents) with its phase, and today's totals. Click a source to open its history.
- **Cost** — API-equivalent live and daily costs, with subscription exclusions and 38 display currencies. Windows uses bundled model prices and a local exchange-rate file read once at startup; an agent can update the file, then restart the app. Unmodified macOS retains upstream online synchronization.
- **Network traffic** — upload and download speed per agent or for the whole computer, as two small lines on the orbs, in the menu bar / taskbar and in the dashboard, with each day's traffic in the history. On Windows, per-agent traffic needs a small optional helper service, installed once with administrator approval.
- **Title themes** — nine ladders of speed titles: *Vehicles*, *Animals*, *Beaufort Scale*, *Cosmos*, *Cultivation*, *Martial Arts*, *Arcane Ranks*, *Game Ranks* and *Engineer Career* — or up to eight sets of your own. Outside *Vehicles*, the dashboard orb shows a glowing title badge.
- **Usage history** — every day since your oldest log: output and input tokens, requests, average speed and peak, cost and network traffic, charted by source over today, yesterday or any date range, with per-model speed and CSV export. Only daily totals are kept, so your history survives even after your tools delete old logs.
- **Live reporting (macOS only, opt-in)** — off by default; if you like, share your live numbers with an API you pick, such as the [Codex Radar](https://codexradar.com/) speed ladder, under a username you choose. Never conversation content, files or project names — see [Privacy](#privacy).
- **Native everywhere** — SwiftUI + AppKit on macOS, WPF with Fluent/Mica on Windows. Light, dark or automatic theme. English, 繁體中文, 简体中文, 日本語.
- **Humane** — effects intensity from *Off* to *Max*, respects Reduce Motion and high contrast, click-through mode, magnetic edges, remembers its place on every display, launches at login.
- **Updates (macOS only)** — upstream GitHub Releases checks and update installation remain on macOS. The Windows fork has no update check or downloader.

<p align="center"><img src="docs/images/orb-styles.png" width="760" alt="The five orb styles: LED Gauge, Needle Tachometer, Liquid Energy, Reactor and Minimal Ring"></p>

<p align="center"><img src="docs/images/orbs.png" width="760" alt="Several orbs, each with its own sources and a glass name plate"></p>

<p align="center"><img src="docs/images/hero-vehicle.gif" width="480" alt="The vehicle inside the dashboard orb, from bicycle to rocket"></p>

<p align="center"><img src="docs/images/dashboards.png" width="560" alt="Dashboard in dark (tok/s) and light (cost per second), with today's cost and network traffic"></p>

<p align="center"><img src="docs/images/history.png" width="760" alt="Usage history window with daily output and cost"></p>

## How speed is measured

Agent Speed reads the transcripts your agents already write on your disk:

| Source | Folder |
| --- | --- |
| Codex (CLI, IDE, desktop) | `~/.codex/sessions` and `archived_sessions`, compressed logs included (or `$CODEX_HOME`) |
| Claude Code (CLI, IDE) | `~/.claude/projects` (or `$CLAUDE_CONFIG_DIR/projects`) |
| Claude Desktop (Code and agent sessions) | `~/.claude/projects`, told apart automatically |
| DeepSeek Harness (`dsh`, desktop and CLI) | `~/.dsh/sessions` (or `$DSH_HOME/sessions`) |
| OpenCode (CLI and desktop) | `~/.local/share/opencode/opencode.db` (or `$XDG_DATA_HOME/opencode`) |
| Gemini CLI | `~/.gemini/tmp` (or `$GEMINI_CLI_HOME/.gemini/tmp`) |
| omp (oh-my-pi) | `~/.omp/agent/sessions` and every profile in `~/.omp/profiles` (or `$PI_CODING_AGENT_DIR/sessions`) |
| Antigravity (`agy` CLI, desktop app, IDE and extensions) | `~/.gemini/antigravity-cli`, `~/.gemini/antigravity` and `~/.gemini/antigravity-ide` (their `conversations` folders) |
| WorkBuddy (Tencent, China edition) | `~/.workbuddy/projects` |
| WorkBuddy AI (international edition) | `~/.workbuddy-ai/projects` |

**Speed = output tokens ÷ the real time the model spent on them** — from the moment the request was sent until the response finished, including time to first token. Parallel agents add up. While a response is still streaming, it is estimated from that agent's recent measured speed and corrected with the measured value the instant it completes. Optionally you can switch to *total throughput* (input + output).

The full, testable specification lives in [`spec/METRICS.md`](spec/METRICS.md). Both apps are verified against the same fixtures and an independent [reference implementation](spec/tools/reference.py).

Ordinary Claude Desktop chat conversations are not stored on your computer, so they cannot be measured; WorkBuddy conversations that run in the cloud are not counted either.

## Privacy

### Windows fork

The Windows application does not make outgoing HTTP requests. Local traffic measurement remains optional. Helper reads event address fields only to exclude loopback traffic; addresses are not stored or sent through its local pipe. See the [security review](windows/TRAFFIC-HELPER-SECURITY.md) for trust boundaries and limitations.

### Upstream privacy description

The following original description applies to unmodified macOS and upstream Windows builds, including their optional network features. Those network features are removed from this Windows fork.

Read-only. Local-only. No telemetry, no accounts. Agent Speed makes only three kinds of automatic network requests, and each one sends nothing about you: the update check against GitHub Releases, a daily download of the public model price table from this repository (GitHub, with jsDelivr as a mirror), and — only when you show costs in a currency other than US dollars — a daily download of public exchange rates (open.er-api.com, with the fawazahmed0 currency API and Frankfurter as fallbacks). Every user requests the same files, so no server learns your currency, models or usage. One switch in Settings turns all network access off; costs then use the price table and rates shipped with the app. Agent Speed never stores or displays conversation content — only token counts, model names and project folder names. The usage history keeps only daily totals (including each day's API-equivalent cost), in a small database on your computer (`~/Library/Application Support/AgentSpeed`, `%LOCALAPPDATA%\AgentSpeed`), and can be cleared in Settings. Updates are verified against the release's SHA-256 checksums before they are installed; the checksums are published with the release, so they catch corrupted or tampered downloads, not a compromised repository. The price table is plain data checked against strict rules; it is not signed.

Network traffic: Agent Speed counts how many bytes programs send and receive, on this computer only. It never reads content, addresses or destinations; only live reporting (below), if you turn it on, sends your agents' byte counts to the service you chose. On Windows, per-agent traffic needs an optional helper service that runs as the restricted Local Service account and only counts bytes per process; it is installed and removed with one administrator prompt each ([details](spec/WINDOWS-SERVICE.md)).

Live reporting (optional, off by default; the dashboard offers it once): if you turn it on for an API — for example a public leaderboard service such as the built-in Codex Radar speed ladder (`deng.codexradar.com`) — Agent Speed sends that service your live speed, the tokens used in the last few seconds and, if you leave them on, the API-equivalent cost in US dollars, your agents' network traffic, models in use and a per-tool breakdown, together with the username you choose (it starts as your computer's name; one click picks a random one). It reports every few seconds while your agents work and stops when they rest. It never sends conversation content, prompts, file, folder or project names, or your usage history, and failed reports are never resent. Each report is signed with a key that stays on your computer; the service can tell that reports come from the same device, not whether the numbers are true. You can pause, turn it off, or ask the service to forget your device in Settings.

Every host Agent Speed may contact, and when, is listed in [`spec/NETWORK.md`](spec/NETWORK.md).

## Build from source

<details>
<summary>macOS (Xcode 26 or later)</summary>

```sh
cd macos
swift test
./scripts/build-app.sh          # → dist/AgentSpeed-macOS.zip
```
</details>

<details>
<summary>Windows (.NET 10 SDK; also cross-builds from macOS/Linux)</summary>

```sh
dotnet test windows/tests/AgentSpeed.Core.Tests
dotnet test windows/tests/AgentSpeed.Presentation.Tests
dotnet publish windows/src/AgentSpeed/AgentSpeed.csproj -c Release -r win-x64 --self-contained true -o dist/windows-offline-x64
```
</details>

## Project layout

```
spec/       measurement + design specification, shared fixtures, reference implementation
macos/      Swift package: AgentSpeedCore (engine) and AgentSpeed (SwiftUI app)
windows/    .NET solution: AgentSpeed.Core (engine), AgentSpeed.Presentation, AgentSpeed (WPF app)
assets/     icon artwork
install.sh  install.ps1
```

## Friends

[Codex Radar](https://codexradar.com/)

## License

[MIT](LICENSE) © 2026 zoefix
