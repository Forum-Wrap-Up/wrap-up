using System.ComponentModel.DataAnnotations;

namespace ForumWrapUp.Web.Models;

public sealed class UpdateWrapUpYearRequest
{
    [Required]
    public bool? IsEnabled { get; set; }
}
