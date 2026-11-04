using System.Security.Cryptography;
using AgentSpeed.Core.IO.Sqlite;
using AgentSpeed.Core.Monitoring;
using AgentSpeed.Core.Parsing;

namespace AgentSpeed.Core.Tests;

public sealed class ClaudeProxyUsageTests : IDisposable
{
    private readonly TempDirectory _temp = new();
    private const long T0 = 1_790_000_000_000;
    private string DatabasePath => _temp.Combine("cc-switch.db");

    public void Dispose() => _temp.Dispose();

    [Fact]
    public void ExactResponseIdCompletesDesktopUsageAndKeepsTheOriginalRequest()
    {
        using var writer = CreateDatabase();
        Insert(writer, "session:resp_a", input: 100, cached: 800, created: 20, output: 60);
        Insert(writer, "session:resp_unrelated", output: 999);
        var source = new ClaudeProxyUsage(DatabasePath);
        var original = Response();
        Assert.Same(original, source.Observe(original));

        var completed = Assert.Single(source.Refresh());

        Assert.Equal(original.Key, completed.Key);
        Assert.Equal(original.AgentId, completed.AgentId);
        Assert.Equal(SourceKind.ClaudeDesktop, completed.Source);
        Assert.Equal(new TokenCounts(920, 800, 20, 60), completed.Tokens);
        Assert.Equal(T0, completed.StartMs);
        Assert.Equal(T0 + 2000, completed.EndMs);
        Assert.True(completed.Final);
        Assert.Empty(source.Refresh());
        var repeated = source.Observe(original with { EndMs = T0 + 1500 });
        Assert.Equal(completed, repeated);
    }

    [Fact]
    public void DelayedProxyCommitIsReadEvenWithoutAnotherTranscriptLine()
    {
        using var writer = CreateDatabase();
        var source = new ClaudeProxyUsage(DatabasePath);
        source.Observe(Response());
        Assert.Empty(source.Refresh());

        Insert(writer, "session:resp_a", output: 75);
        File.SetLastWriteTimeUtc(DatabasePath, DateTime.UtcNow.AddSeconds(1));

        Assert.Equal(75, Assert.Single(source.Refresh()).Tokens.Output);
    }

    [Fact]
    public void ANewCandidateIsQueriedEvenWhenTheDatabaseDidNotChange()
    {
        using var writer = CreateDatabase();
        Insert(writer, "session:resp_b", output: 75);
        var source = new ClaudeProxyUsage(DatabasePath);
        source.Observe(Response());
        Assert.Empty(source.Refresh());

        source.Observe(Response() with { Key = "resp_b" });

        Assert.Equal("resp_b", Assert.Single(source.Refresh()).Key);
    }

    [Fact]
    public void InclusiveInputIsNotCountedTwice()
    {
        using var writer = CreateDatabase();
        Insert(writer, "session:resp_a", input: 1000, cached: 800, semantics: 1);
        var source = new ClaudeProxyUsage(DatabasePath);
        source.Observe(Response());

        Assert.Equal(1000, Assert.Single(source.Refresh()).Tokens.Input);
    }

    [Theory]
    [InlineData(SourceKind.ClaudeCode, 0, false, "resp_a")]
    [InlineData(SourceKind.Codex, 0, false, "resp_a")]
    [InlineData(SourceKind.ClaudeDesktop, 12, false, "resp_a")]
    [InlineData(SourceKind.ClaudeDesktop, 0, true, "resp_a")]
    [InlineData(SourceKind.ClaudeDesktop, 0, false, "msg_a")]
    public void NativeUsageAndOtherSourcesAreNotReplaced(SourceKind kind, long output, bool final, string key)
    {
        using var writer = CreateDatabase();
        Insert(writer, "session:" + key, output: 900);
        var source = new ClaudeProxyUsage(DatabasePath);
        var original = Response() with { Source = kind, Key = key, Tokens = new TokenCounts(Output: output), Final = final };

        Assert.Same(original, source.Observe(original));
        Assert.Empty(source.Refresh());
    }

    [Theory]
    [InlineData("claude", "proxy", 200, 100)]
    [InlineData("codex", "proxy", 200, 100)]
    [InlineData("claude-desktop", "session", 200, 100)]
    [InlineData("claude-desktop", "proxy", 500, 100)]
    [InlineData("claude-desktop", "proxy", 200, 0)]
    public void OnlySuccessfulDesktopProxyUsageCompletesAResponse(string app, string dataSource, long status, long output)
    {
        using var writer = CreateDatabase();
        Insert(writer, "session:resp_a", app: app, dataSource: dataSource, status: status, output: output);
        var source = new ClaudeProxyUsage(DatabasePath);
        source.Observe(Response());

        Assert.Empty(source.Refresh());
    }

    [Fact]
    public void CountsAreBoundedAndUnknownStartUsesTheRecordedLatency()
    {
        using var writer = CreateDatabase();
        Insert(writer, "session:resp_a", input: long.MaxValue, cached: -1, created: long.MaxValue, output: long.MaxValue);
        var source = new ClaudeProxyUsage(DatabasePath);
        source.Observe(Response() with { StartMs = null });

        var completed = Assert.Single(source.Refresh());
        Assert.Equal(new TokenCounts(TokenCounts.MaxValue, 0, TokenCounts.MaxValue, TokenCounts.MaxValue), completed.Tokens);
        Assert.Equal(T0, completed.StartMs);
    }

    [Fact]
    public void MissingDatabaseIsNotCreatedAndCanAppearLater()
    {
        var source = new ClaudeProxyUsage(DatabasePath);
        source.Observe(Response());
        Assert.Empty(source.Refresh());
        Assert.False(File.Exists(DatabasePath));

        using var writer = CreateDatabase();
        Insert(writer, "session:resp_a");
        Assert.Single(source.Refresh());
    }

    [Fact]
    public void CorruptOrUnsupportedDatabaseDoesNotBreakMonitoring()
    {
        File.WriteAllText(DatabasePath, "not SQLite");
        var source = new ClaudeProxyUsage(DatabasePath);
        source.Observe(Response());
        Assert.Empty(source.Refresh());
        var unsupported = _temp.Combine("unsupported.db");
        using var writer = SqliteConnection.Open(unsupported, SqliteOpenMode.ReadWriteCreate);
        writer.Execute("CREATE TABLE unrelated (x INTEGER)");
        var other = new ClaudeProxyUsage(unsupported);
        other.Observe(Response());
        Assert.Empty(other.Refresh());
    }

    [Fact]
    public void ClosedWalDatabaseIsNotModifiedAndNoSidecarsAreCreated()
    {
        using (var writer = CreateDatabase())
        {
            writer.Execute("PRAGMA journal_mode=WAL");
            Insert(writer, "session:resp_a");
        }

        var before = SHA256.HashData(File.ReadAllBytes(DatabasePath));
        var files = Directory.GetFiles(_temp.Path).Order().ToArray();
        var source = new ClaudeProxyUsage(DatabasePath);
        source.Observe(Response());
        Assert.Single(source.Refresh());

        Assert.Equal(before, SHA256.HashData(File.ReadAllBytes(DatabasePath)));
        Assert.Equal(files, Directory.GetFiles(_temp.Path).Order());
    }

    [Fact]
    public void BusyDatabaseDoesNotStallMonitoringAndIsRetriedAfterCommit()
    {
        using var writer = CreateDatabase();
        var source = new ClaudeProxyUsage(DatabasePath);
        source.Observe(Response());
        writer.Execute("BEGIN EXCLUSIVE");
        Insert(writer, "session:resp_a");
        var timer = System.Diagnostics.Stopwatch.StartNew();
        Assert.Empty(source.Refresh());
        Assert.True(timer.Elapsed < TimeSpan.FromSeconds(1));
        writer.Execute("COMMIT");
        File.SetLastWriteTimeUtc(DatabasePath, DateTime.UtcNow.AddSeconds(1));

        Assert.Single(source.Refresh());
    }

    [Fact]
    public void AnOpenWalWriterCanCommitNewUsage()
    {
        using var writer = CreateDatabase();
        writer.Execute("PRAGMA journal_mode=WAL");
        var source = new ClaudeProxyUsage(DatabasePath);
        source.Observe(Response());
        Assert.Empty(source.Refresh());
        Insert(writer, "session:resp_a");

        Assert.Single(source.Refresh());
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

    private static void Insert(SqliteConnection writer, string key, long input = 100, long cached = 0, long created = 0,
        long output = 60, long semantics = 2, string app = "claude-desktop", string dataSource = "proxy", long status = 200)
    {
        using var insert = writer.Prepare("INSERT INTO proxy_request_logs VALUES (?1, ?2, ?3, ?4, ?5, ?6, 2000, ?7, ?8, ?9, ?10)");
        insert.Bind(1, key).Bind(2, app).Bind(3, input).Bind(4, output).Bind(5, cached).Bind(6, created)
            .Bind(7, (T0 + 2000) / 1000).Bind(8, semantics).Bind(9, status).Bind(10, dataSource).Run();
    }

    private static ParsedResponse Response() => new("resp_a", SourceKind.ClaudeDesktop, "p/session.jsonl", "gpt-route",
        T0, T0 + 1000, default, Final: false);
}
