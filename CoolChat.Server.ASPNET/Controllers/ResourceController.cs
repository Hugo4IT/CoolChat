using System.Net.Http.Headers;
using CoolChat.Domain.Interfaces;
using CoolChat.Server.ASPNET.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CoolChat.Server.ASPNET.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourceController : ControllerBase
{
    private readonly IResourceService _resourceService;

    public ResourceController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [HttpGet("Icon")]
    public async Task<IActionResult> Icon([FromQuery] int id)
    {
        var resource = await _resourceService.GetById(id);

        if (resource == null)
            return NotFound();

        var bytes = await resource.Read(_resourceService.BasePath);
        // Response.Headers.ContentType = "image/webp";
        // Response.Headers.ContentLength = bytes.Length;
        ContentDispositionHeaderValue cd = new("attachment")
        {
            FileName = resource.OriginalFileName + ".webp"
        };
        Response.Headers.ContentDisposition = cd.ToString();

        return File(bytes, "image/webp");
    }
}