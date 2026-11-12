using Shared.Domain.Exceptions;

namespace Shared.Domain.Images;

public sealed record ImageFile
{
    public const int MaxSizeInBytes = 5 * 1024 * 1024;

    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public byte[] Content { get; }
    public ImageFormat Format { get; }

    public ImageFile(byte[] content)
    {
        if (content.Length == 0)
        {
            throw new DomainException("An image cannot be empty.");
        }
        if (content.Length > MaxSizeInBytes)
        {
            throw new DomainException("An image cannot be larger than 5 MB.");
        }

        Content = content;
        Format = FormatOf(content);
    }

    private static ImageFormat FormatOf(byte[] content)
    {
        if (content.AsSpan().StartsWith(JpegSignature))
        {
            return ImageFormat.Jpeg;
        }
        if (content.AsSpan().StartsWith(PngSignature))
        {
            return ImageFormat.Png;
        }

        throw new DomainException("An image must be a JPEG or PNG file.");
    }
}
