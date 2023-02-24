using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CoolChat.Data.EntityFramework;

public class DataContext : IdentityDbContext<IdentityUser>
{

}