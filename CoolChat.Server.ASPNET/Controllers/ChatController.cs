using CoolChat.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoolChat.Server.ASPNET.Controllers;

public class MessageDto
{
    public string Author { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime Date { get; set; }
}

internal class MyChatsResponse
{
    public List<int> Ids { get; set; } = null!;
}

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IChatService _chatService;
    private readonly ITokenService _tokenService;

    public ChatController(IChatService chatService, IAccountService accountService, ITokenService tokenService)
    {
        _chatService = chatService;
        _tokenService = tokenService;
        _accountService = accountService;
    }

    [HttpGet("GetMessages")]
    [Authorize]
    public async Task<IActionResult> GetMessages([FromQuery] int id, [FromQuery] int start, [FromQuery] int count)
    {
        if (count > 100)
            return BadRequest("Cannot request over 100 messages at once");

        if (await _chatService.GetByIdAsync(id) == null)
            return BadRequest("Invalid chat ID");

        return Ok(
            _chatService
                .GetMessagesAsync(id, start, count)
                .Select(message => new MessageDto
                {
                    Author = message.Author.Name,
                    Content = message.Content,
                    Date = message.Date
                })
        );
    }

    // [HttpGet("MyChats"), Authorize]
    // public IActionResult MyChats()
    // {
    //     Account account = _accountService.GetByUsername(User.Identity!.Name!)!;

    //     return Ok(new MyChatsResponse
    //     {
    //         Ids = account.Chats.Select(chat => chat.Id).ToList(),
    //     });
    // }

    // [HttpPost("AddMessage"), Authorize]
    // public async Task<IActionResult> AddMessage([FromQuery] int id)
    // {
    //     if (Request.ContentLength == null)
    //         return BadRequest("No content length");

    //     byte[] bytes = new byte[(int)Request.ContentLength!];
    //     await Request.Body.ReadExactlyAsync(bytes, 0, (int)Request.ContentLength!);

    //     string messageContent = Encoding.UTF8.GetString(bytes);
    //     messageContent = Markdown.ToHtml(new HtmlSanitizer().Sanitize(messageContent)).Trim();

    //     if (messageContent.Length == 0)
    //         return BadRequest("Empty Message");

    //     Chat? chat = _chatService.GetById(id);

    //     if (chat == null)
    //         return BadRequest("Invalid chat id");

    //     ClaimsPrincipal principal = _tokenService.GetPrincipalFromExpiredToken(Request.Headers.Authorization![0]!.Substring("Bearer ".Length));
    //     string name = principal.Identity!.Name!;

    //     Account account = _accountService.GetByUsername(name)!;

    //     if (!chat.Members.Any(m => m.Id == account.Id))
    //         return Unauthorized();

    //     Message message = new Message
    //     {
    //         Author = account,
    //         Content = messageContent,
    //         Date = DateTime.Now,
    //     };

    //     _chatService.AddMessage(chat, message);

    //     return Ok( new GetMessagesResponse.Item
    //     {
    //         Author = name,
    //         Content = message.Content,
    //         Date = message.Date,
    //     });
    // }
}