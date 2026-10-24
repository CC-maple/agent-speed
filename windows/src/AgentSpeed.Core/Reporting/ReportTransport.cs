using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;

namespace AgentSpeed.Core.Reporting;

/// <summary>A response of any status with its (capped) body; header names lower-case.</summary>
public sealed record ReportHttpResponse(int Status, IReadOnlyDictionary<string, string> Headers, byte[] Body);

public enum ReportTransportErrorKind
{
    /// <summary>A redirect to a non-https URL, or more than three (discovery).</summary>
    DisallowedRedirect,
    TooLarge,
    Timeout,
    Offline,
    Tls,
    Other,
}

/// <summary>Why the user is told a request failed (DESIGN 11): each a localized <c>report.error.*</c> sentence.</summary>
public enum ReportFailure
{
    /// <summary>No connection, the host not found or refusing, a timeout.</summary>
    Unreachable,

    /// <summary>A TLS failure, or a redirect to a non-<c>https</c> URL.</summary>
    Insecure,

    /// <summary>The service answered with a status the client cannot act on (<c>5xx</c>, <c>429</c>, an unexpected <c>3xx</c> …).</summary>
    Unavailable,

    /// <summary>The response could not be read (too large).</summary>
    Response,

    /// <summary>The device key could not be created or used on this computer.</summary>
    Key,
}

/// <summary>A transport failure; <see cref="Reason"/> is short, for the log only, and never contains a URL.</summary>
public sealed class ReportTransportException(ReportTransportErrorKind kind, string reason) : Exception(reason)
{
    public ReportTransportErrorKind Kind { get; } = kind;

    public string Reason => Message;

    /// <summary>What the user is told (DESIGN 11, No internals).</summary>
    public ReportFailure Failure => Kind switch
    {
        ReportTransportErrorKind.DisallowedRedirect or ReportTransportErrorKind.Tls => ReportFailure.Insecure,
        ReportTransportErrorKind.TooLarge => ReportFailure.Response,
        _ => ReportFailure.Unreachable,
    };
}

public static class ReportFailures
{
    public static string Key(ReportFailure failure) => failure switch
    {
        ReportFailure.Unreachable => "report.error.unreachable",
        ReportFailure.Insecure => "report.error.insecure",
        ReportFailure.Unavailable => "report.error.unavailable",
        ReportFailure.Response => "report.error.response",
        _ => "report.error.key",
    };
}

/// <summary>The network seam of live reporting; tests use a scripted transport or the reference server.</summary>
public interface IReportTransport
{
    /// <summary>Discovery <c>GET</c> (REPORTING 3.1): <c>Accept</c>, <c>User-Agent</c>, <c>If-None-Match</c>; up to three <c>https</c> redirects.</summary>
    /// <exception cref="ReportTransportException">The request failed.</exception>
    Task<ReportHttpResponse> GetAsync(Uri url, string? etag, string userAgent, int maxBytes, TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>An API request: never redirected (a <c>3xx</c> comes back as the response).</summary>
    /// <exception cref="ReportTransportException">The request failed.</exception>
    Task<ReportHttpResponse> SendAsync(ReportRequest request, int maxBytes, TimeSpan timeout, CancellationToken cancellationToken = default);
}
