using System.Security.Claims;
using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoolChat.Server.ASPNET.Controllers;

class GroupCreateResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public Resource Icon { get; set; } = null!;
}

class MyGroupsResponse
{
    public class Item
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public Resource Icon { get; set; } = null!;
    }

    public List<Item> Items { get; set; } = null!;
}

class GetChannelsResponse
{
    public class Item
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string Name { get; set; } = null!;
        public int Icon { get; set; }
    }

    public List<Item> Items { get; set; } = null!;
}

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ITokenService _tokenService;
    private readonly IResourceService _resourceService;
    private readonly IAccountService _accountService;

    public GroupController(IGroupService groupService, IResourceService resourceService, ITokenService tokenService, IAccountService accountService)
    {
        _groupService = groupService;
        _tokenService = tokenService;
        _accountService = accountService;
        _resourceService = resourceService;
    }

    [HttpPost("Create"), Authorize]
    public async Task<IActionResult> Create()
    {
        if (!Request.Form.ContainsKey("title"))
            return BadRequest("Group has no title");
        
        string? title = Request.Form["title"][0];

        if (title == null || title.Length < 2)
            return BadRequest("Invalid title");

        if (Request.Form.Files.Count != 1)
            return BadRequest("No file attached");
        
        // Upload group icon
        IFormFile file = Request.Form.Files[0];

        byte[] content = new byte[file.Length];
        await file.OpenReadStream().ReadExactlyAsync(content, 0, (int)file.Length);
        
        using Image image = await Image.LoadAsync(file.OpenReadStream());
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(128, 128),
            Mode = ResizeMode.Crop,
        }));

        MemoryStream stream = new();
        await image.SaveAsWebpAsync(stream);

        Resource icon = await _resourceService.Upload(file.FileName, stream.ToArray());

        // Get current user
        ClaimsPrincipal principal = _tokenService.GetPrincipalFromExpiredToken(Request.Headers.Authorization![0]!.Substring("Bearer ".Length));
        string name = principal.Identity!.Name!;

        Account account = _accountService.GetByUsername(name)!;

        // Create group and add user
        Group group = _groupService.CreateGroup(title, icon);
        _groupService.AddMember(group, account);

        return Ok(new GroupCreateResponse
        {
            Id = group.Id,
            Title = group.Name,
            Icon = group.Icon,
        });
    }

    [HttpGet("MyGroups"), Authorize]
    public IActionResult MyGroups()
    {
        ClaimsPrincipal principal = _tokenService.GetPrincipalFromExpiredToken(Request.Headers.Authorization![0]!.Substring("Bearer ".Length));
        string name = principal.Identity!.Name!;

        Account account = _accountService.GetByUsername(name)!;
        
        return Ok(new MyGroupsResponse
        {
            Items = account.Groups.Select(group => new MyGroupsResponse.Item
            {
                Id = group.Id,
                Title = group.Name,
                Icon = group.Icon,
            }).ToList(),
        });
    }

    [HttpGet("GetChannels"), Authorize]
    public IActionResult GetChannels([FromQuery] int id)
    {
        Group? group = _groupService.GetById(id);

        if (group == null)
            return BadRequest("Invalid group id");
        
        return Ok(new GetChannelsResponse
        {
            Items = group.Channels.Select(channel => new GetChannelsResponse.Item
            {
                Id = channel.Id,
                ChatId = channel.Chat.Id,
                Name = channel.Name,
                Icon = channel.Icon,
            }).ToList(),
        });
    }
}
