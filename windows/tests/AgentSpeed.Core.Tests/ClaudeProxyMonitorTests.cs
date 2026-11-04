using AgentSpeed.Core.IO.Sqlite;
using AgentSpeed.Core.Monitoring;
using AgentSpeed.Core.Parsing;

namespace AgentSpeed.Core.Tests;

public sealed class ClaudeProxyMonitorTests : IDisposable
{
    private readonly TempDirectory _temp = new();
    private readonly long _start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000 * 1000 - 6000;
    private string ClaudeRoot => _temp.Combine("claude", "projects");
    private string DatabasePath => _temp.Combine("cc-switch.db");
    private long Today => _start - 3_600_000;

    public void Dispose() => _temp.Dispose();

    [Fact]
    public void SeedCompletesRoutedDesktopUsageWithoutAddingAnotherRequest()
    {
        WriteTranscript();
        using var database = CreateDatabase();
        Insert(database);
        using var core = CreateMonitor();
        core.Seed(_start + 4000, Today, CancellationToken.None);

        var (snapshot, events) = core.BuildSnapshot(_start + 4000, Today, MonitorState.Running, observeLive: false);

        Assert.Equal(1, snapshot.Today.Requests);
        Assert.Equal(60, snapshot.Today.Output);
        Assert.Equal(30.0, snapshot.AverageToday!.Value, 6);
        var desktop = snapshot.Sources.Single(s => s.Kind == SourceKind.ClaudeDesktop);
        Assert.Equal(60, desktop.Today.Output);
        Assert.Equal(0, snapshot.Sources.Single(s => s.Kind == SourceKind.Codex).Today.Requests);
        Assert.Empty(snapshot.Warnings.OfType<MonitorWarning.UnfinalizedUsage>());
        Assert.Empty(events);
    }

    [Fact]
    public void PollCompletesDelayedUsageAndStreamingReplayCannotEraseIt()
    {
        var path = WriteTranscript();
        using var database = CreateDatabase();
        using var core = CreateMonitor();
        core.Seed(_start + 1000, Today, CancellationToken.None);
        var (before, _) = core.BuildSnapshot(_start + 1000, Today, MonitorState.Running, observeLive: false);
        Assert.Equal(0, before.Today.Output);
        Insert(database);
        Assert.True(core.PollRecent(_start + 2500));

        var (completed, _) = core.BuildSnapshot(_start + 2500, Today, MonitorState.Running, observeLive: false);
        Assert.Equal(1, completed.Today.Requests);
        Assert.Equal(60, completed.Today.Output);
        Assert.True(completed.Live.Output > 0);
        Assert.Equal(30.0, completed.AverageToday!.Value, 6);

        File.AppendAllText(path, ClaudeLines.Assistant(_start + 1900, "resp_route", 0, "null", input: 0) + "\n");
        Assert.True(core.ProcessPaths([path], _start + 2600, Today));
        var (replayed, _) = core.BuildSnapshot(_start + 2600, Today, MonitorState.Running, observeLive: false);
        Assert.Equal(completed.Today, replayed.Today);
        Assert.Equal(completed.AverageToday, replayed.AverageToday);
    }

    [Fact]
    public void DisablingDesktopAlsoDisablesItsProxySupplement()
    {
        WriteTranscript();
        using var database = CreateDatabase();
        Insert(database);
        using var core = CreateMonitor(new HashSet<SourceKind> { SourceKind.ClaudeCode });
        core.Seed(_start + 4000, Today, CancellationToken.None);
        core.PollRecent(_start + 4500);

        var (snapshot, _) = core.BuildSnapshot(_start + 4500, Today, MonitorState.Running, observeLive: false);
        Assert.Equal(0, snapshot.Today.Requests);
        Assert.Empty(snapshot.Agents);
    }

    [Fact]
    public void ProxyRecordsWithoutTranscriptResponsesAreNeverCounted()
    {
        Directory.CreateDirectory(ClaudeRoot);
        using var database = CreateDatabase();
        Insert(database);
        using var core = CreateMonitor();
        core.Seed(_start + 4000, Today, CancellationToken.None);
        core.PollRecent(_start + 4500);

        var (snapshot, _) = core.BuildSnapshot(_start + 4500, Today, MonitorState.Running, observeLive: false);
        Assert.Equal(0, snapshot.Today.Requests);
        Assert.Empty(snapshot.Agents);
    }

    private MonitorCore CreateMonitor(IReadOnlySet<SourceKind>? enabled = null) => new(
        new MonitorConfiguration(enabled ?? new HashSet<SourceKind> { SourceKind.ClaudeDesktop, SourceKind.ClaudeCode }),
        new SourceLocations(_temp.Combine("codex"), ClaudeRoot, string.Empty, string.Empty, [],
            ClaudeUsageDatabasePath: DatabasePath), Today);

    private string WriteTranscript()
    {
        var relative = "claude/projects/project/session.jsonl";
        return _temp.WriteFile(relative, string.Join("\n", [
            ClaudeLines.User(_start, "\"go\"", extra: ",\"entrypoint\":\"claude-desktop-3p\""),
            ClaudeLines.ApiRequest(_start),
            ClaudeLines.Assistant(_start + 1000, "resp_route", 0, "null", input: 0),
        ]) + "\n");
    }

    private SqliteConnection CreateDatabase()
    {
        var writer = SqliteConnection.Open(DatabasePath, SqliteOpenMode.ReadWriteCreate);
        writer.Execute("""
            CREATE TABLE proxy_request_logs (
                request_id TEXT PRIMARY KEY, app_type TEXT, input_tokens INTEGER, output_tokens INTEGER,
                cache_read_tokens INTEGER, cache_creation_tokens INTEGER, latency_ms INTEGER,
                created_at INTEGER, input_token_semantics INTEGER, status_code INTEGER, data_source TEXT
            )
            """);
        return writer;
    }

    private void Insert(SqliteConnection writer)
    {
        using var insert = writer.Prepare("""
            INSERT INTO proxy_request_logs VALUES ('session:resp_route', 'claude-desktop', 100, 60, 800, 0, 2000, ?1, 2, 200, 'proxy')
            """);
        insert.Bind(1, (_start + 2000) / 1000).Run();
    }
}
