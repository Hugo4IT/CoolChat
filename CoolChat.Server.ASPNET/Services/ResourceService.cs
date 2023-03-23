using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using CoolChat.Server.ASPNET.Extensions;

namespace CoolChat.Server.ASPNET.Services;

public class ResourceService : IResourceService
{
    private readonly IConfiguration _configuration;
    private readonly DataContext _dataContext;

    public ResourceService(IConfiguration configuration, DataContext dataContext)
    {
        _configuration = configuration;
        _dataContext = dataContext;
    }

    public string BasePath => Path.Join(Directory.GetCurrentDirectory(), _configuration["ResourcePath"]);

    public async Task<Resource> Upload(string filename, byte[] data)
    {
        Directory.CreateDirectory(BasePath);

        var resource = new Resource
        {
            OriginalFileName = filename
        };

        _dataContext.Resources.Add(resource);
        await _dataContext.SaveChangesAsync();

        await resource.Write(BasePath, data);

        return resource;
    }

    public Task<Resource?> GetById(int id)
    {
        return Task.FromResult(_dataContext.Resources.FirstOrDefault(r => r.Id == id));
    }
}