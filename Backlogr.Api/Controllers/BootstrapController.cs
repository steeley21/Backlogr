using Backlogr.Api.DTOs.Admin;
using Backlogr.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backlogr.Api.Controllers;

[ApiController]
[Route("api/bootstrap")]
public sealed class BootstrapController : ControllerBase
{
    private const string BootstrapSecretHeaderName = "X-Bootstrap-Secret";
    private readonly ISuperAdminBootstrapService _bootstrapService;

    public BootstrapController(ISuperAdminBootstrapService bootstrapService)
    {
        _bootstrapService = bootstrapService;
    }

    [AllowAnonymous]
    [HttpPost("superadmin")]
    public async Task<ActionResult<AdminUserSummaryDto>> BootstrapSuperAdmin(CancellationToken cancellationToken)
    {
        var providedSecret = Request.Headers[BootstrapSecretHeaderName].FirstOrDefault() ?? string.Empty;

        try
        {
            var elevatedUser = await _bootstrapService.BootstrapAsync(providedSecret, cancellationToken);
            return Ok(elevatedUser);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
