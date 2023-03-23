using Microsoft.AspNetCore.Mvc;

namespace CoolChat.Server.ASPNET.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    private readonly ILogger<PingController> _logger;

    public PingController(ILogger<PingController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "Ping")]
    public string Ping()
    {
        return "Pong";
    }
}