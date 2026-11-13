using Shared.Domain.Images;

namespace Shared.Application;

public interface IImageStorage
{
    Task<string> SaveAsync(ImageFile image);

    // Deleting an address that no longer points to a file does nothing.
    Task DeleteAsync(string address);
}
