using System.Net;
using System.Net.Http;

namespace AgentSpeed.Core.Network;

/// <summary>One HTTPS <c>GET</c> of NETWORK 1: the request line and only these headers, against a feature's host list.</summary>
/// <param name="AllowedHosts">Hosts the request and every redirect hop may use (HTTPS only).</param>
public sealed record HttpFetchRequest(Uri Url, string UserAgent, int MaxBytes, IReadOnlySet<string> AllowedHosts)
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

    public string Accept { get; init; } = "application/json";

    /// <summary>The <c>ETag</c> the same URL returned before; null sends no <c>If-None-Match</c>.</summary>
    public string? IfNoneMatch { get; init; }

    public TimeSpan Timeout { get; init; } = DefaultTimeout;

    /// <summary><c>AgentSpeed/&lt;version&gt; (Windows)</c>, the only identification a request carries (NETWORK 1).</summary>
    public static string UserAgentFor(string version) => $"AgentSpeed/{version} (Windows)";

    /// <summary>HTTPS on the allowed hosts, default port, no user information.</summary>
    public bool Permits(Uri? url) =>
        url is { IsAbsoluteUri: true } && url.Scheme == Uri.UriSchemeHttps && url.Port == 443 && url.UserInfo.Length == 0
        && url.IdnHost.Length > 0 && AllowedHosts.Contains(url.IdnHost.ToLowerInvariant());
}

/// <summary>A response of any status; a <c>304</c> has an empty body. Bodies of other non-2xx statuses are not read.</summary>
public sealed class HttpFetchResponse
{
    public HttpFetchResponse(int status, byte[]? body = null, string? etag = null, Uri? finalUrl = null)
    {
        Status = status;
        Body = body ?? [];
        ETag = etag;
        FinalUrl = finalUrl;
    }

    public int Status { get; }

    public byte[] Body { get; }

    public string? ETag { get; }

    /// <summary>The URL that answered, after redirects.</summary>
    public Uri? FinalUrl { get; }

    public bool IsSuccess => Status is >= 200 and <= 299;
}

public enum HttpFetchFailure
{
    /// <summary>Not HTTPS, or a host (or redirect hop) outside the feature's list.</summary>
    DisallowedUrl,

    /// <summary>Larger than the feature's cap, by <c>Content-Length</c> or while streaming.</summary>
    TooLarge,

    /// <summary>Timeouts, offline, TLS and other transport failures.</summary>
    Transport,
}

/// <summary>A failed fetch; <see cref="Reason"/> is short and never contains a URL or anything personal.</summary>
public sealed class HttpFetchException : Exception
{
    public HttpFetchException(HttpFetchFailure failure, string reason)
        : base(reason)
    {
        Failure = failure;
    }

    public HttpFetchFailure Failure { get; }

    /// <summary>A short reason for logs and the inline status.</summary>
    public string Reason => Message;

    public static HttpFetchException DisallowedUrl() => new(HttpFetchFailure.DisallowedUrl, "disallowed URL");

    public static HttpFetchException TooLarge() => new(HttpFetchFailure.TooLarge, "response too large");
}

/// <summary>The network seam: the app uses a caller-supplied transport, tests a scripted transport.</summary>
public interface IHttpTransport
{
    /// <exception cref="HttpFetchException">Disallowed URL, too large, or a transport failure.</exception>
    /// <exception cref="OperationCanceledException">The caller cancelled.</exception>
    Task<HttpFetchResponse> GetAsync(HttpFetchRequest request, CancellationToken cancellationToken = default);
}
