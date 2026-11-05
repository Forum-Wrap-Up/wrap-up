using System.ComponentModel.DataAnnotations;
using ForumWrapUp.Web.Validation;

namespace ForumWrapUp.Web.Models;

public sealed class CreateUserRequest
{
    [Range(1, int.MaxValue)]
    public required int Mid { get; init; }

    [Required]
    [MinLength(PasswordPolicyValidator.MinimumLength)]
    public required string Password { get; init; }
}
