using AgentSpeed.Core.IO.Sqlite;
using AgentSpeed.Core.Parsing;

namespace AgentSpeed.Core.Monitoring;

/// <summary>
/// Completes zero-usage Claude Desktop Responses snapshots from CC Switch's local request log. Only exact response
/// ids are joined; the proxy log is never a separate source of requests, agents or estimated token counts.
/// </summary>
internal sealed class ClaudeProxyUsage(string databasePath)
{
    private const int Capacity = 65_536;
    private const long MaxTimestampSeconds = 253_402_300_799;
    private readonly Dictionary<string, ParsedResponse> _pending = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Usage> _resolved = new(StringComparer.Ordinal);
    private readonly Queue<string> _order = new();
    private Stamp? _lastStamp;
    private bool _dirty;

    public ParsedResponse Observe(ParsedResponse response)
    {
        if (response.Source != SourceKind.ClaudeDesktop || response.Final || response.Tokens.Output != 0
            || !response.Key.StartsWith("resp_", StringComparison.Ordinal))
        {
            _pending.Remove(response.Key);
            return response;
        }

        if (_resolved.TryGetValue(response.Key, out var usage))
        {
            return usage.Apply(response);
        }

        if (!_pending.ContainsKey(response.Key))
        {
            if (_order.Count >= Capacity && _order.TryDequeue(out var oldest))
            {
                _pending.Remove(oldest);
                _resolved.Remove(oldest);
            }

            _order.Enqueue(response.Key);
        }

        _pending[response.Key] = response;
        _dirty = true;
        return response;
    }

    /// <summary>Called after seeding and on the monitor's 1-second poll, including when transcripts stopped changing.</summary>
    public IReadOnlyList<ParsedResponse> Refresh()
    {
        if (_pending.Count == 0 || ReadStamp() is not { } stamp || (!_dirty && stamp == _lastStamp))
        {
            return [];
        }

        var updates = new List<ParsedResponse>();
        try
        {
            using var database = SqliteReadOnly.Open(databasePath, out _, busyTimeoutMs: 20);
            var columns = new HashSet<string>(StringComparer.Ordinal);
            using (var schema = database.Prepare("PRAGMA table_info(proxy_request_logs)"))
            {
                while (schema.Step())
                {
                    if (schema.GetString(1) is { } name)
                    {
                        columns.Add(name);
                    }
                }
            }

            // Older CC Switch versions did not label input semantics; the desktop bridge uses Anthropic's
            // non-cached input counter. Newer logs explicitly label inclusive input as 1 and exclusive input as 2.
            var semantics = columns.Contains("input_token_semantics") ? "input_token_semantics" : "2";
            var proxyOnly = columns.Contains("data_source") ? " AND data_source = 'proxy'" : string.Empty;
            using var query = database.Prepare($"""
                SELECT input_tokens, output_tokens, cache_read_tokens, cache_creation_tokens, latency_ms, created_at,
                       {semantics}
                FROM proxy_request_logs
                WHERE request_id = ?1 AND app_type = 'claude-desktop' AND status_code BETWEEN 200 AND 299
                      AND output_tokens > 0{proxyOnly}
                """);
            foreach (var response in _pending.Values)
            {
                query.Bind(1, "session:" + response.Key);
                try
                {
                    if (!query.Step())
                    {
                        continue;
                    }

                    var seconds = query.GetInt64(5);
                    if (seconds < 0 || seconds > MaxTimestampSeconds)
                    {
                        continue;
                    }

                    var input = TokenCounts.Clamp(query.GetInt64(0));
                    var cached = TokenCounts.Clamp(query.GetInt64(2));
                    var created = TokenCounts.Clamp(query.GetInt64(3));
                    if (query.GetInt64(6) != 1)
                    {
                        input = Math.Min(TokenCounts.MaxValue, input + cached + created);
                    }

                    var usage = new Usage(new TokenCounts(input, cached, created, TokenCounts.Clamp(query.GetInt64(1))),
                        seconds * 1000, query.GetInt64(4));
                    _resolved[response.Key] = usage;
                    updates.Add(usage.Apply(response));
                }
                finally
                {
                    query.Reset();
                }
            }

            _lastStamp = stamp;
            _dirty = false;
        }
        catch (Exception e) when (e is SqliteException or IOException or UnauthorizedAccessException)
        {
            // Optional metadata: a missing schema, a busy writer or a damaged database must not stop monitoring.
        }

        foreach (var response in updates)
        {
            _pending.Remove(response.Key);
        }

        return updates;
    }

    private Stamp? ReadStamp()
    {
        try
        {
            var file = new FileInfo(databasePath);
            if (!file.Exists)
            {
                return null;
            }

            var wal = new FileInfo(databasePath + "-wal");
            return new Stamp(file.Length, file.LastWriteTimeUtc, wal.Exists ? wal.Length : -1,
                wal.Exists ? wal.LastWriteTimeUtc : default);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return null;
        }
    }

    private readonly record struct Stamp(long Length, DateTime WriteUtc, long WalLength, DateTime WalWriteUtc);

    private sealed record Usage(TokenCounts Tokens, long EndMs, long LatencyMs)
    {
        public ParsedResponse Apply(ParsedResponse response)
        {
            var end = Math.Max(response.EndMs, EndMs);
            return response with
            {
                Tokens = TokenCounts.Max(response.Tokens, Tokens),
                Final = true,
                EndMs = end,
                StartMs = response.StartMs ?? (LatencyMs > 0 && LatencyMs <= end ? end - LatencyMs : null),
            };
        }
    }
}
