namespace Shared.Domain.Images;

// Removes the parts of a JPEG or PNG file that describe the photo instead of showing it:
// EXIF (which carries the GPS position where the photo was taken), XMP, IPTC, comments and
// text chunks. Only the EXIF orientation is kept, see ExifOrientation. A part the code cannot
// read is kept unchanged, because the goal is that an ordinary photo from a phone reveals
// nothing, not to defend against a file built to fool the check.
internal static class ImageMetadata
{
    public static byte[] RemoveFrom(byte[] content, ImageFormat format) => format switch
    {
        ImageFormat.Jpeg => JpegMetadata.RemoveFrom(content),
        ImageFormat.Png => PngMetadata.RemoveFrom(content),
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}
