namespace ForumWrapUp.Importing.ForumFree;

public interface IForumApiCookieProvider
{
    ValueTask<string> GetCookieAsync(CancellationToken cancellationToken);
}
