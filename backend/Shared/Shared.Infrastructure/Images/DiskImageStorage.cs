using Shared.Application;
using Shared.Domain.Images;

namespace Shared.Infrastructure.Images;

internal sealed class DiskImageStorage : IImageStorage
{
    private readonly string _folder;
    private readonly string _requestPath;

    public DiskImageStorage(string folder, string requestPath)
    {
        _folder = folder;
        _requestPath = requestPath;
    }

    public async Task<string> SaveAsync(ImageFile image)
    {
        var name = $"{Guid.NewGuid()}{ExtensionOf(image.Format)}";
        await File.WriteAllBytesAsync(Path.Combine(_folder, name), image.Content);
        return $"{_requestPath}/{name}";
    }

    public Task DeleteAsync(string address)
    {
        // Only the file name is kept, so an address can never point outside the folder.
        var name = Path.GetFileName(address);
        // File.Delete does nothing when the file does not exist.
        File.Delete(Path.Combine(_folder, name));
        return Task.CompletedTask;
    }

    private static string ExtensionOf(ImageFormat format) => format switch
    {
        ImageFormat.Jpeg => ".jpg",
        ImageFormat.Png => ".png",
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}
