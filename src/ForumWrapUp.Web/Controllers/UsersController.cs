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
[Route("api/admin/users")]
public sealed class UsersController : ControllerBase
{
    private readonly UserAccountService _userAccountService;

    public UsersController(UserAccountService userAccountService)
    {
        _userAccountService = userAccountService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        List<UserAccount> accounts = await _userAccountService.GetAllAsync(cancellationToken);
        List<UserResponse> response = accounts
            .Select(UserResponse.FromAccount)
            .ToList();

        return Ok(response);
    }

    [HttpGet("{mid:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetByMid(int mid, CancellationToken cancellationToken)
    {
        UserAccount? account = await _userAccountService.GetByMidAsync(mid, cancellationToken);

        if (account is null)
            return NotFound();

        return UserResponse.FromAccount(account);
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        UserAccount? account;

        try
        {
            account = await _userAccountService.CreateAsync(request.Mid, request.Password, cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid user data.",
                Detail = exception.Message
            });
        }

        if (account is null)
            return Conflict();

        UserResponse response = UserResponse.FromAccount(account);

        return CreatedAtAction(nameof(GetByMid), new { mid = account.Mid }, response);
    }

    [HttpPut("{mid:int}/password")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPassword(int mid, SetUserPasswordRequest request, CancellationToken cancellationToken)
    {
        UserAccount? account;

        try
        {
            account = await _userAccountService.SetPasswordAsync(mid, request.Password, cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid password.",
                Detail = exception.Message
            });
        }

        if (account is null)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{mid:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int mid, CancellationToken cancellationToken)
    {
        bool accountFound = await _userAccountService.DeactivateAsync(mid, cancellationToken);

        if (!accountFound)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{mid:int}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(int mid, CancellationToken cancellationToken)
    {
        bool accountFound = await _userAccountService.ReactivateAsync(mid, cancellationToken);

        if (!accountFound)
            return NotFound();

        return NoContent();
    }
}
