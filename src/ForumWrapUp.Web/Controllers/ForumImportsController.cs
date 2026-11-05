using ForumWrapUp.Data.Entities;
using ForumWrapUp.Importing;
using ForumWrapUp.Web.Models;
using ForumWrapUp.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumWrapUp.Web.Controllers;

[ApiController]
[Authorize(Policy = AdminApiDefaults.AuthorizationPolicy)]
[Produces("application/json")]
[Route("api/admin/imports")]
public sealed class ForumImportsController : ControllerBase
{
    private const string StatusRouteName = "GetAdminForumImportOperation";

    private readonly IForumDataImportService _importService;

    public ForumImportsController(IForumDataImportService importService)
    {
        _importService = importService;
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ForumImportOperationResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ForumImportOperationResponse>> QueueImport(QueueForumImportRequest request, CancellationToken cancellationToken)
    {
        ImportOperation operation;

        try
        {
            operation = await _importService.QueueUserDataImportAsync(request.Mid, request.FromDate, request.ToDate, cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid import request.",
                Detail = exception.Message
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        string statusUrl = Url.RouteUrl(StatusRouteName, new { operationId = operation.Id }) 
            ?? throw new InvalidOperationException("The import status route could not be generated.");

        ForumImportOperationResponse response = ForumImportOperationResponse.FromOperation(operation, statusUrl);

        return AcceptedAtRoute(StatusRouteName, new { operationId = operation.Id }, response);
    }

    [HttpGet("{operationId:guid}", Name = StatusRouteName)]
    [ProducesResponseType(typeof(ForumImportOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ForumImportOperationResponse>> GetOperation(Guid operationId, CancellationToken cancellationToken)
    {
        ImportOperation? operation = await _importService.GetOperationAsync(operationId, cancellationToken);
        if (operation is null)
            return NotFound();

        string statusUrl = Url.RouteUrl(StatusRouteName, new { operationId })
            ?? throw new InvalidOperationException("The import status route could not be generated.");

        return ForumImportOperationResponse.FromOperation(operation, statusUrl);
    }
}
