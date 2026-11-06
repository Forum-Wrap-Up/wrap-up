namespace ForumWrapUp.Web.Models;

public sealed class LoginFormOptions
{
    public TimeSpan ClientTimeout { get; set; } = TimeSpan.FromSeconds(20);
}
