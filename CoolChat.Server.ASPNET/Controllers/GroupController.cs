using System.ComponentModel.DataAnnotations;
using System.Net;
using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using static CoolChat.Server.ASPNET.Validation;

namespace CoolChat.Server.ASPNET.Controllers;

public class InviteDto
{
    [Required] public int InviteId { get; set; }

    [Required] public int GroupId { get; set; }

    [Required] public string GroupName { get; set; }

    [Required] public int MemberCount { get; set; }

    [Required] public Resource GroupIcon { get; set; }

    [Required] public string SenderName { get; set; }

    [Required] public int SenderId { get; set; }
}

public class ChannelDto
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public string Name { get; set; } = null!;
    public int Icon { get; set; }

    public static ChannelDto FromModel(Channel channel)
    {
        return new()
        {
            Id = channel.Id,
            ChatId = channel.Chat.Id,
            Icon = channel.Icon,
            Name = channel.Name
        };
    }
}

public class GroupDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public Resource Icon { get; set; } = null!;


    public List<ChannelDto> Channels { get; set; } = null!;

    public static GroupDto FromModel(Group group)
    {
        return new()
        {
            Id = group.Id,
            Title = group.Name,
            Icon = group.Icon,
            Channels = group.Channels.ToList().Select(ChannelDto.FromModel).ToList()
        };
    }
}

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IGroupService _groupService;
    private readonly IResourceService _resourceService;
    private readonly ITokenService _tokenService;

    public GroupController(IGroupService groupService, IResourceService resourceService, ITokenService tokenService,
        IAccountService accountService)
    {
        _groupService = groupService;
        _tokenService = tokenService;
        _accountService = accountService;
        _resourceService = resourceService;
    }

    [HttpPost("Create")]
    [Authorize]
    public async Task<IActionResult> Create()
    {
        ValidationBuilder validation = new();
        string? title;

        validation.Guard(nameof(title),
            (() => !Request.Form.ContainsKey("title"), "Group has no title"));

        title = Request.Form["title"][0]?.Trim();

        validation.Guard(nameof(title),
            (() => title == null, "Please add a title"),
            (() => title!.Length < 2,
                $"Your group must have a name that's at least 2 characters long, get a little more creative. What about: 'wowie amazing {title} server'"));

        validation.Guard("icon",
            (() => Request.Form.Files.Count != 1,
                "Please select an icon for your group, you can find some good ones <a href=\"https://randompicturegenerator.com/cat.php\">here</a>"));

        if (validation.Build() is IInvalid invalid)
            return Ok(invalid);

        title = WebUtility.HtmlEncode(title);
        
        // Upload group icon
        var file = Request.Form.Files[0];

        var content = new byte[file.Length];
        await file.OpenReadStream().ReadExactlyAsync(content, 0, (int)file.Length);

        using var image = await Image.LoadAsync(file.OpenReadStream());
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(128, 128),
            Mode = ResizeMode.Crop
        }));

        MemoryStream stream = new();
        await image.SaveAsWebpAsync(stream);

        var icon = await _resourceService.Upload(file.FileName, stream.ToArray());

        var account = (await _accountService.GetByUsernameAsync(User.Identity!.Name!))!;

        // Create group and add user
        var group = await _groupService.CreateGroupAsync(account, title!, icon);

        return Ok(GroupDto.FromModel(group));
    }

    [HttpPost("AddChannel/{groupId}"), Authorize]
    public async Task<IActionResult> AddChannel(int groupId)
    {
        var group = await _groupService.GetByIdAsync(groupId);

        if (group == null)
            return NotFound();
        
        ValidationBuilder validation = new();

        string? name;
        int? icon;
        bool? isPrivate;

        validation.Guard(nameof(name),
            (() => !Request.Form.ContainsKey("name"), "You must provide a name for your cool new channel"));
        
        validation.Guard(nameof(icon),
            (() => !Request.Form.ContainsKey("icon"), "Pick a nice icon for your channel"));
        
        validation.Guard(nameof(isPrivate),
            (() => !Request.Form.ContainsKey("private"), "Please specify if the group will be public or private"));

        name = Request.Form["name"][0]!.Trim();

        if (!int.TryParse(Request.Form["icon"][0]!, out int parsedIcon))
            return Ok(Invalid("Invalid icon format"));
        icon = parsedIcon;

        if (!bool.TryParse(Request.Form["private"][0]!, out bool parsedPrivate))
            return Ok(Invalid("Invalid access format"));
        isPrivate = parsedPrivate;
        
        validation.Guard(nameof(name),
            (() => name.Length < 2, "Channel must have at least 2 characters in its name"),
            (() => name.Length > 20, "Channel name may not be longer than 20 characters"));
        
        validation.Guard(nameof(icon),
            (() => icon < 0, "Invalid icon"),
            (() => icon > 20, "Invalid icon"));

        if (validation.Build() is IInvalid invalid)
            return Ok(invalid);

        name = WebUtility.HtmlEncode(name);
        
        var account = (await _accountService.GetByUsernameAsync(User.Identity!.Name!))!;
        var result = await _groupService.AddChannelAsync(group, account, name, icon.Value, isPrivate.Value);

        if (result is IInvalid<Channel> invalidResult)
            return Ok(invalidResult);

        var channel = result.Valid()!.Value;
        
        return Ok(Valid(new ChannelDto
        {
            Id = channel.Id,
            Icon = channel.Icon,
            Name = channel.Name,
            ChatId = channel.Chat.Id,
        }));
    }

    [HttpGet("MyGroups")]
    [Authorize]
    public async Task<IActionResult> MyGroups()
    {
        var account = (await _accountService.GetByUsernameAsync(User.Identity!.Name!))!;

        return Ok((await _accountService.GetGroupsAsync(account.Id)).ToList().Select(group => new GroupDto
        {
            Id = group.Id,
            Title = group.Name,
            Icon = group.Icon,
            Channels = group.Channels.ToList().Select(channel => new ChannelDto
            {
                Id = channel.Id,
                ChatId = channel.Chat.Id,
                Icon = channel.Icon,
                Name = channel.Name,
            }).ToList()
        }));
    }

    [HttpGet("MyInvites")]
    [Authorize]
    public async Task<IActionResult> MyInvitesAsync()
    {
        var account = (await _accountService.GetByUsernameAsync(User.Identity!.Name!))!;

        return Ok(account.ReceivedInvites.Where(invite => invite.Type == InviteType.Group)
            .Select(async invite =>
            {
                var group = await _groupService.GetByIdAsync(invite.InvitedId);
                var sender = invite.From;

                if (group == null)
                    return null;

                return new InviteDto
                {
                    InviteId = invite.Id,
                    GroupId = group.Id,
                    GroupName = group.Name,
                    GroupIcon = group.Icon,
                    MemberCount = group.Members.Count,
                    SenderId = sender.Id,
                    SenderName = sender.Name
                };
            }).Where(i => i != null));
    }
}