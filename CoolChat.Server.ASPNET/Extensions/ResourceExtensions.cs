using CoolChat.Domain.Models;

namespace CoolChat.Server.ASPNET.Extensions;

public static class ResourceExtensions
{
    public static string GetRealPath(this Resource resource, string basePath) =>
        Path.Join(basePath, resource.Id.ToString());

    public static Task<byte[]> Read(this Resource resource, string basePath) =>
        File.ReadAllBytesAsync(resource.GetRealPath(basePath));

    public static Task Write(this Resource resource, string basePath, byte[] data) =>
        File.WriteAllBytesAsync(resource.GetRealPath(basePath), data);
}