[zoefix/agent-speed](https://github.com/zoefix/agent-speed) 1.3.0 を基にした Windows オフライン fork です。この fork は正式リリース前で、Traffic Helper は 1.0.1 です。[変更履歴](CHANGELOG.md)と [Windows の手順](windows/OFFLINE.md)を参照してください。macOS のソースは変更していません。

<div align="center">

<img src="assets/icon/icon-1024.png" width="128" alt="Agent Speed アイコン">

# Agent Speed

**AI agent が全力で走る瞬間を、目で感じる。** Codex・Claude Code・Claude Desktop・DeepSeek Harness・OpenCode・Gemini CLI・omp・Antigravity・WorkBuddy・WorkBuddy AI のトークン速度をリアルタイムに映し出すネイティブ計器。待機中は静かに、レッドラインでは燃え上がる。

[English](README.md) · [繁體中文](README.zh-TW.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md)

<img src="docs/images/hero.png" width="760" alt="Agent Speed のフローティングオーブとメニューバー">

</div>

## Windows オフライン fork

- Windows では自動・手動更新、オンライン価格・為替同期、ライブ送信と関連 UI を削除しています。
- ローカルログ、トークン速度、履歴、費用計算は維持します。価格表は内蔵、為替はローカルファイルです。agent が PowerShell スクリプトで更新した後は再起動してください。
- ダッシュボードに再起動ボタンと、初期状態で無効の agent 通信量チェックボックスを追加しました。変更は再起動後に反映し、接続状態を表示します。コンピューター全体の通信量は独立しています。
- 任意の Helper 1.0.1 は Windows サービスとして動作します。アプリからのインストールには管理者の承認が必要で、成功後にデスクトップへサービス管理用ショートカットを作成します。内蔵だけではサービスはインストールされません。
- ルートの `VERSION` は上流の 1.3.0、ローカル Windows 開発ビルドは 0.0.0 のままです。公開前に [CHANGELOG.md](CHANGELOG.md) を確認してください。以下の macOS コマンドは上流用です。

## インストール

**macOS 14 以降**（Apple Silicon・Intel）——ターミナルに貼り付け：

```sh
curl -fsSL https://raw.githubusercontent.com/zoefix/agent-speed/main/install.sh | sh
```

**Windows オフライン fork**（.NET 10 SDK、x64 を検証済み）——このリポジトリからビルド：

```powershell
dotnet publish windows\src\AgentSpeed\AgentSpeed.csproj -c Release -r win-x64 --self-contained true -o dist\windows-offline-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false
```

Helper の内蔵、C++／Windows SDK の要件と手動インストール手順は [Windows の手順](windows/OFFLINE.md)に記載しています。上流の `install.ps1` は上流版をダウンロードし、この fork はインストールしません。macOS のコマンドは上流のままです。

<details>
<summary>アンインストール</summary>

```sh
curl -fsSL https://raw.githubusercontent.com/zoefix/agent-speed/main/install.sh | sh -s -- --uninstall
```

この Windows fork の任意サービスは、アプリの通信量設定から削除できます。ポータブルビルドを削除する前に [Windows の手順](windows/OFFLINE.md)を確認してください。

上流 macOS のアンインストーラーは `--purge` で設定も削除できます。Windows fork のサービス管理は上記手順を参照してください。
</details>

## 特長

- **フローティングオーブ**——スモークガラスの計器と 36 セグメントの LED ゲージ。速度が上がるにつれて称号もランクアップ——「徒歩」「自転車」から「新幹線」「戦闘機」へ、レッドラインに達すれば「ロケット」。ゲージは熱のスペクトルに沿って点灯し、彗星がリングを駆け、火花が散り、残り火が舞い上がり、応答が完了するたびに衝撃波が広がります。今日のピークを更新するとお祝いします。
- **5 つのオーブスタイル、オーブはいくつでも**——「LED ゲージ」「針式タコメーター」「リキッドエナジー」「リアクター」「ミニマルリング」。オーブごとにソースを自由に組み合わせられ（すべて、ひとつのツール、Claude Code＋Codex など）、どんな壁紙の上でも読みやすいガラスのネームプレートが付きます。
- **メニューバー／タスクバー**——幅が揺れないコンパクトなゲージと数値。tok/s、金額、または tok/s に現在の金額と今日の合計を添えた表示から選べ、ネットワーク速度も追加できます。Windows ではタスクバーの時計の隣に組み込まれます。
- **ダッシュボード**——円形メーターの中で、スピード称号に合わせた光る線画の乗り物が走ります。徒歩から自転車で走り出し、レッドラインではロケットへ。さらにライブグラフ、ソース別の速度、作業中のすべての agent（サブ agent を含む）とその状態、今日の集計。ソースをクリックすると使用履歴が開きます。
- **費用**——API 換算の現在・日別費用、定額プランの除外、38 通貨表示を維持します。Windows は内蔵価格表と起動時に一度だけ読むローカル為替ファイルを使用し、agent による更新後は再起動が必要です。未変更の macOS は上流のオンライン同期を維持します。
- **通信量**——agent ごと、またはコンピューター全体のアップロード／ダウンロード速度を、オーブに小さな 2 行で表示。メニューバー／タスクバーとダッシュボードにも表示でき、使用履歴に日ごとの通信量を記録します。Windows で agent ごとの通信量を見るには、任意の小さなヘルパーサービスを一度だけインストールします（管理者の承認が必要）。
- **称号テーマ**——9 種類のスピード称号：「乗り物」「動物」「風力階級」「宇宙」「仙道修行」「武芸の道」「魔法の位階」「ゲームランク」「エンジニアの道」。自分だけの称号も最大 8 個まで作れます。乗り物以外のテーマでは、ダッシュボードのメーターに光る称号バッジが表示されます。
- **使用履歴**——最も古い記録から毎日の出力・入力トークン、リクエスト数、平均速度とピーク、費用と通信量を、ソース別に今日・昨日・任意の期間でグラフ表示。モデルごとの速度も確認でき、CSV に書き出せます。保存するのは日ごとの集計だけなので、ツールが古い記録を削除しても履歴は残ります。
- **ライブ送信（macOS のみ、任意）**——初期設定はオフ。参加したいときは、選んだユーザー名で、[Codex Radar](https://codexradar.com/) のスピードランキングなど好きな API にライブの数値を共有できます。会話の内容、ファイル、プロジェクト名は一切送りません。詳しくは[プライバシー](#プライバシー)へ。
- **完全ネイティブ**——macOS は SwiftUI + AppKit、Windows は WPF と Fluent／Mica。ライト・ダーク・自動テーマ。English、繁體中文、简体中文、日本語。
- **人にやさしい**——エフェクトの強さは「オフ」から「全開」まで。「視差効果を減らす」やハイコントラストに対応し、クリック透過、画面端へのスナップ、ディスプレイごとの位置記憶、ログイン時の自動起動を備えています。
- **更新（macOS のみ）**——macOS は上流の GitHub Releases 確認と更新を維持します。Windows fork に更新確認・ダウンロード機能はありません。

<p align="center"><img src="docs/images/orb-styles.png" width="760" alt="5 つのオーブスタイル：LED ゲージ、針式タコメーター、リキッドエナジー、リアクター、ミニマルリング"></p>

<p align="center"><img src="docs/images/orbs.png" width="760" alt="それぞれ別のソースを表示し、ガラスのネームプレートが付いた複数のオーブ"></p>

<p align="center"><img src="docs/images/hero-vehicle.gif" width="480" alt="ダッシュボードのメーター内の乗り物（自転車からロケットまで）"></p>

<p align="center"><img src="docs/images/dashboards.png" width="560" alt="ダーク（tok/s）とライト（1 秒あたりの金額）のダッシュボード。今日の費用と通信量を表示"></p>

<p align="center"><img src="docs/images/history.png" width="760" alt="日ごとの出力と費用を表示した使用履歴ウィンドウ"></p>

## 速度の測り方

Agent Speed は、agent がすでにローカルに書き出している記録だけを読み取ります。

| ソース | フォルダ |
| --- | --- |
| Codex（CLI・IDE・デスクトップ） | `~/.codex/sessions` と `archived_sessions`、圧縮された記録を含む（または `$CODEX_HOME`） |
| Claude Code（CLI・IDE） | `~/.claude/projects`（または `$CLAUDE_CONFIG_DIR/projects`） |
| Claude Desktop（Code と agent セッション） | `~/.claude/projects`（自動で区別） |
| DeepSeek Harness（`dsh`、デスクトップと CLI） | `~/.dsh/sessions`（または `$DSH_HOME/sessions`） |
| OpenCode（CLI とデスクトップ） | `~/.local/share/opencode/opencode.db`（または `$XDG_DATA_HOME/opencode`） |
| Gemini CLI | `~/.gemini/tmp`（または `$GEMINI_CLI_HOME/.gemini/tmp`） |
| omp（oh-my-pi） | `~/.omp/agent/sessions` と `~/.omp/profiles` 内の各プロファイル（または `$PI_CODING_AGENT_DIR/sessions`） |
| Antigravity（`agy` CLI・デスクトップアプリ・IDE・拡張機能） | `~/.gemini/antigravity-cli`、`~/.gemini/antigravity`、`~/.gemini/antigravity-ide`（それぞれの `conversations` フォルダ） |
| WorkBuddy（Tencent、中国版） | `~/.workbuddy/projects` |
| WorkBuddy AI（国際版） | `~/.workbuddy-ai/projects` |

**速度＝出力トークン ÷ モデルが実際にそれらに費やした時間**——リクエスト送信から応答完了まで、最初のトークンを待つ時間を含みます。複数の agent は合算されます。ストリーミング中の応答はその agent の最近の実測速度で推定し、完了した瞬間に実測値で補正します。「総スループット」（入力＋出力）にも切り替えられます。

テスト可能な完全な仕様は [`spec/METRICS.md`](spec/METRICS.md) にあります。両プラットフォームのアプリは同じテストデータと独立した[リファレンス実装](spec/tools/reference.py)で相互検証されています。

Claude Desktop の通常のチャット会話はローカルに保存されないため、計測できません。WorkBuddy でクラウド実行された会話も集計されません。

## プライバシー

### Windows fork

Windows アプリは外部 HTTP リクエストを行いません。任意のローカル通信量測定は残ります。Helper はループバックを除外するためだけにイベントのアドレスを読み、保存やローカルパイプへの送信は行いません。権限と検証の範囲は [安全性レビュー](windows/TRAFFIC-HELPER-SECURITY.md)を参照してください。

### 上流のプライバシー説明

以下は未変更の macOS と上流 Windows 版の説明です。そこに含まれる任意のネットワーク機能は、この Windows fork から削除されています。

読み取り専用・ローカルのみで動作し、テレメトリもアカウントもありません。Agent Speed が自動で行うネットワーク通信は次の 3 種類だけで、どれもあなたに関する情報を送りません：GitHub Releases でのアップデート確認、このリポジトリで公開しているモデル価格表の 1 日 1 回のダウンロード（GitHub、予備に jsDelivr）、そして米ドル以外の通貨で費用を表示するときだけ行う公開為替レートの 1 日 1 回のダウンロード（open.er-api.com、予備に fawazahmed0 currency API と Frankfurter）。全員が同じファイルを取得するため、サーバーがあなたの通貨・モデル・使用量を知ることはありません。設定のスイッチ 1 つですべての通信をオフにでき、その場合はアプリ内蔵の価格表とレートで費用を計算します。Agent Speed は会話の内容を保存・表示せず、使うのはトークン数・モデル名・プロジェクトフォルダー名だけです。使用履歴は日別の集計（各日の API 換算費用を含む）だけを、お使いのコンピューター上の小さなデータベース（`~/Library/Application Support/AgentSpeed`、`%LOCALAPPDATA%\AgentSpeed`）に保存し、設定から消去できます。アップデートはインストール前にリリースの SHA-256 チェックサムで検証します。チェックサムはリリースと一緒に公開されるため、破損や改ざんされたダウンロードは検出できますが、リポジトリ自体の侵害は防げません。価格表は厳格なルールで検証される単なるデータで、署名はされていません。

通信量：Agent Speed はこのコンピューター上で、プログラムが送受信したバイト数だけを数えます。内容・アドレス・接続先は一切読みません。このうち agent のバイト数を送信するのは、オンにしたライブ送信（下記）だけで、送信先は選んだサービスです。Windows で agent ごとの通信量を表示するには、任意のヘルパーサービスが必要です。権限の少ない「ローカル サービス」アカウントで動作し、プロセスごとのバイト数だけを数えます。インストールと削除のときに、それぞれ一度だけ管理者の承認が必要です（[詳細](spec/WINDOWS-SERVICE.md)）。

ライブ送信（任意・初期設定はオフ。ダッシュボードで一度だけ案内します）：API（公開ランキングサービスなど。組み込みの選択肢は Codex Radar のスピードランキング `deng.codexradar.com`）に対してオンにすると、Agent Speed は現在の速度、直近数秒に使った token 数、そしてオンのままにした項目——米ドル建ての API 換算費用、agent の通信量、使用中のモデル、ツールごとの内訳——を、選んだユーザー名（初期値はコンピューター名。ワンクリックでランダムな名前にできます）と一緒にそのサービスへ送信します。agent が作業している間は数秒ごとに送信し、休んでいる間は止まります。会話の内容、プロンプト、ファイル・フォルダー・プロジェクトの名前、使用履歴は一切送信せず、送信に失敗したデータを後から送り直すこともありません。各送信はこのコンピューターから出ない鍵で署名されます。サービスは同じデバイスからの送信であることは確認できますが、数値が正しいかどうかは確認できません。設定から一時停止・オフにしたり、サービスにデバイスの削除を依頼したりできます。

Agent Speed が接続する可能性のあるホストとそのタイミングは、すべて [`spec/NETWORK.md`](spec/NETWORK.md) に記載しています。

## ソースからビルド

<details>
<summary>macOS（Xcode 26 以降）</summary>

```sh
cd macos
swift test
./scripts/build-app.sh          # → dist/AgentSpeed-macOS.zip
```
</details>

<details>
<summary>Windows（.NET 10 SDK。macOS／Linux からのクロスビルドも可）</summary>

```sh
dotnet test windows/tests/AgentSpeed.Core.Tests
dotnet test windows/tests/AgentSpeed.Presentation.Tests
dotnet publish windows/src/AgentSpeed/AgentSpeed.csproj -c Release -r win-x64 --self-contained true -o dist/windows-offline-x64
```
</details>

## リンク

[Codex Radar](https://codexradar.com/)

## ライセンス

[MIT](LICENSE) © 2026 zoefix
