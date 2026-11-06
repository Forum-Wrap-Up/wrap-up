using ForumWrapUp.Data.Entities;
using ForumWrapUp.Web.Models;
using ForumWrapUp.Web.Security;
using ForumWrapUp.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumWrapUp.Web.Controllers;

[ApiController]
[Authorize(Policy = AdminApiDefaults.AuthorizationPolicy)]
[Produces("application/json")]
[Route("api/admin/wrap-ups")]
public sealed class AnnualWrapUpsController : ControllerBase
{
    private readonly AnnualWrapUpGenerationService _generationService;

    public AnnualWrapUpsController(AnnualWrapUpGenerationService generationService)
    {
        _generationService = generationService;
    }

    [HttpPost("{mid:int}/{year:int}/generate")]
    [ProducesResponseType(typeof(AnnualWrapUpGenerationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AnnualWrapUpGenerationResponse>> Generate(int mid, int year, CancellationToken cancellationToken)
    {
        if (mid <= 0 || year < 1 || year >= DateOnly.MaxValue.Year)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid MID or wrap-up year."
            });

        AnnualWrapUp summary;
        try
        {
            summary = await _generationService.GenerateAsync(mid, year, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (AnnualWrapUpGenerationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "The annual wrap-up cannot be generated.",
                Detail = exception.Message
            });
        }

        AnnualWrapUpGenerationResponse response = AnnualWrapUpGenerationResponse.FromSummary(summary);
        return Ok(response);
    }
}
