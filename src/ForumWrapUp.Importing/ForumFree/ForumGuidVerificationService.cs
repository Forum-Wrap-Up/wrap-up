using System.Net;
using System.Text.RegularExpressions;

namespace ForumWrapUp.Importing.ForumFree;

public sealed class ForumGuidVerificationService : IForumGuidVerificationService
{
    private static readonly Regex ProfileContactsRegex = new(@"<dl\b[^>]*\bclass\s*=\s*['""][^'""]*\bprofile-contacts\b[^'""]*['""][^>]*>(?<content>.*?)</dl>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex HtmlTagsRegex = new(@"<[^>]+>", RegexOptions.Compiled);

    private readonly HttpClient _httpClient;

    public ForumGuidVerificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> VerifyAsync(int mid, Guid guid, CancellationToken cancellationToken = default)
    {
        if (mid <= 0)
            throw new ArgumentOutOfRangeException(nameof(mid), "MID must be positive.");

        string requestPath = $"?act=Profile&MID={mid}&tab=contact";
        using HttpResponseMessage response = await _httpClient.GetAsync(requestPath, cancellationToken);
        response.EnsureSuccessStatusCode();

        string html = await response.Content.ReadAsStringAsync(cancellationToken);
        Match contactsMatch = ProfileContactsRegex.Match(html);

        if (!contactsMatch.Success)
            return false;

        string contactText = HtmlTagsRegex.Replace(contactsMatch.Groups["content"].Value, " ");
        contactText = WebUtility.HtmlDecode(contactText);

        return contactText.Contains(guid.ToString("D"), StringComparison.OrdinalIgnoreCase);
    }
}
