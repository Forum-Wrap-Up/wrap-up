using System.ComponentModel.DataAnnotations;
using ForumWrapUp.Web.Validation;

namespace ForumWrapUp.Web.Models;

public sealed class SetUserPasswordRequest
{
    [Required]
    [MinLength(PasswordPolicyValidator.MinimumLength)]
    public required string Password { get; init; }
}
