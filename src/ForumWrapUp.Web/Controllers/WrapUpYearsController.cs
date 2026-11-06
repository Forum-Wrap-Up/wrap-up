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
[Route("api/admin/wrap-up-years")]
public sealed class WrapUpYearsController : ControllerBase
{
    private readonly WrapUpYearService _wrapUpYearService;

    public WrapUpYearsController(WrapUpYearService wrapUpYearService)
    {
        _wrapUpYearService = wrapUpYearService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WrapUpYearResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WrapUpYearResponse>>> GetAll(CancellationToken cancellationToken)
    {
        List<WrapUpYear> years = await _wrapUpYearService.GetAllAsync(cancellationToken);
        List<WrapUpYearResponse> response = years
            .Select(WrapUpYearResponse.FromYear)
            .ToList();

        return Ok(response);
    }

    [HttpPut("{year:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(WrapUpYearResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WrapUpYearResponse>> SetEnabled(int year, UpdateWrapUpYearRequest request, CancellationToken cancellationToken)
    {
        if (year < 1 || year > 9999)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid wrap-up year."
            });

        if (request.IsEnabled is not bool isEnabled)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "The isEnabled field is required."
            });

        WrapUpYear configuredYear = await _wrapUpYearService.SetEnabledAsync(year, isEnabled, cancellationToken);
        WrapUpYearResponse response = WrapUpYearResponse.FromYear(configuredYear);

        return Ok(response);
    }
}
