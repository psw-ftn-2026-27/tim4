using Shared.Domain.Images;

namespace Shared.Application;

public interface IImageStorage
{
    Task<string> SaveAsync(ImageFile image);
}
