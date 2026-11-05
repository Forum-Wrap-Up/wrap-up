using ForumWrapUp.Data.Entities.Raw;
using ForumWrapUp.Importing;
using ForumWrapUp.Web.Models;
using ForumWrapUp.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumWrapUp.Web.Controllers;

[ApiController]
[Authorize(Policy = AdminApiDefaults.AuthorizationPolicy)]
[Produces("application/json")]
[Route("api/admin/forum-profiles")]
public sealed class ForumProfilesController : ControllerBase
{
    private readonly ForumProfileService _profileService;

    public ForumProfilesController(ForumProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpPost("{mid:int}/refresh")]
    [ProducesResponseType(typeof(ForumProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ForumProfileResponse>> Refresh(int mid, CancellationToken cancellationToken)
    {
        ForumProfile profile;

        try
        {
            profile = await _profileService.AddOrRefreshAsync(mid, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Unable to retrieve the ForumFree profile.");
        }
        catch (InvalidDataException)
        {
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "ForumFree returned an invalid profile response.");
        }

        return ForumProfileResponse.FromProfile(profile);
    }
}
