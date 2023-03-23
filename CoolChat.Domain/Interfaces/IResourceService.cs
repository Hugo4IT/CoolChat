using CoolChat.Domain.Models;

namespace CoolChat.Domain.Interfaces;

public interface IResourceService
{
    public string BasePath { get; }

    public Task<Resource> Upload(string filename, byte[] data);
    public Task<Resource?> GetById(int id);
}