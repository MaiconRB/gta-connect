using GtaConnect.Application.Features.DevSeed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace GtaConnect.Api.Controllers;

// Dev-only: gate dentro da action (404, não 403 — nem revela que a rota existe fora de
// Development), mesmo espírito do "if (app.Environment.IsDevelopment())" já usado no
// Program.cs pro Swagger.
[ApiController]
[Route("api/dev")]
[AllowAnonymous]
public class DevController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IDevSeedService _seedService;

    public DevController(IWebHostEnvironment environment, IDevSeedService seedService)
    {
        _environment = environment;
        _seedService = seedService;
    }

    [HttpPost("seed")]
    public async Task<ActionResult<DevSeedResultDto>> Seed(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var result = await _seedService.SeedAsync(cancellationToken);
        return Ok(result);
    }
}
