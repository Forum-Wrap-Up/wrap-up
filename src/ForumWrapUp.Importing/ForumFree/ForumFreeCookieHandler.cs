namespace ForumWrapUp.Importing.ForumFree;

public sealed class ForumFreeCookieHandler : DelegatingHandler
{
    private readonly IForumApiCookieProvider _cookieProvider;

    public ForumFreeCookieHandler(IForumApiCookieProvider cookieProvider)
    {
        _cookieProvider = cookieProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string cookie = await _cookieProvider.GetCookieAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(cookie))
            throw new InvalidOperationException("The ForumFree API cookie has not been configured.");

        if (cookie.Contains('\r') || cookie.Contains('\n'))
            throw new InvalidOperationException("The ForumFree API cookie is invalid.");

        if (!request.Headers.TryAddWithoutValidation("Cookie", cookie))
            throw new InvalidOperationException("The ForumFree API cookie could not be added to the request.");

        return await base.SendAsync(request, cancellationToken);
    }
}
