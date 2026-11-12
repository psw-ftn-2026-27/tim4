using Microsoft.Extensions.DependencyInjection;
using Shared.Application;

namespace Shared.Infrastructure.Images;

public static class ImageStorageExtensions
{
    public static IServiceCollection AddImageStorage(this IServiceCollection services, string folder, string requestPath)
    {
        return services.AddSingleton<IImageStorage>(new DiskImageStorage(folder, requestPath));
    }
}
