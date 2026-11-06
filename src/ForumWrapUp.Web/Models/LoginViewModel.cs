using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ForumWrapUp.Web.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Inserisci il tuo MID.")]
    [RegularExpression("[1-9][0-9]{0,9}", ErrorMessage = "Inserisci un MID valido: un numero intero maggiore di zero.")]
    public string Mid { get; set; } = string.Empty;

    [Required(ErrorMessage = "Inserisci la password del wrap-up.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    [BindNever]
    public int ClientTimeoutMs { get; set; }
}
