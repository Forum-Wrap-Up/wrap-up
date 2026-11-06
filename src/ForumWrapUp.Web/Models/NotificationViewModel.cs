namespace ForumWrapUp.Web.Models;

public sealed class NotificationViewModel
{
    public NotificationViewModel(string message, int dismissAfterMilliseconds = 6000)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentOutOfRangeException.ThrowIfNegative(dismissAfterMilliseconds);

        Message = message;
        DismissAfterMilliseconds = dismissAfterMilliseconds;
    }

    public string Message { get; }

    public int DismissAfterMilliseconds { get; }
}
