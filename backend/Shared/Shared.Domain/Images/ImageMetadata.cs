using System.Buffers.Binary;
using System.Text;

namespace Shared.Domain.Images;

// Removes the parts of a JPEG or PNG file that describe the photo instead of showing it:
// EXIF (which carries the GPS position where the photo was taken), XMP, IPTC, comments and
// text chunks. Only the EXIF orientation is kept, see ExifOrientation. A part the code cannot
// read is kept unchanged, because the goal is that an ordinary photo from a phone reveals
// nothing, not to defend against a file built to fool the check.
internal static class ImageMetadata
{
    private const int JpegStartLength = 2;
    private const byte JpegMarker = 0xFF;
    private const byte JpegStartOfScan = 0xDA;
    private const byte JpegEndOfImage = 0xD9;
    // APP1 holds EXIF and XMP.
    private const byte JpegApp1 = 0xE1;
    // APP13 holds IPTC, COM holds a free-text comment.
    private static readonly byte[] RemovedJpegSegments = [0xED, 0xFE];
    private static readonly byte[] JpegExifHeader = "Exif\0\0"u8.ToArray();
    private const int JpegSegmentHeaderLength = 4;

    private const int PngSignatureLength = 8;
    private const int PngChunkFrameLength = 12;
    private const string PngExif = "eXIf";
    private static readonly string[] RemovedPngChunks = ["tEXt", "zTXt", "iTXt"];

    public static byte[] RemoveFrom(byte[] content, ImageFormat format) => format switch
    {
        ImageFormat.Jpeg => RemoveFromJpeg(content),
        ImageFormat.Png => RemoveFromPng(content),
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };

    // A JPEG is a list of segments (marker, length, data) up to the start of the image data.
    private static byte[] RemoveFromJpeg(byte[] content)
    {
        using var kept = new MemoryStream(content.Length);
        kept.Write(content, 0, JpegStartLength);
        var position = JpegStartLength;

        while (position + JpegSegmentHeaderLength <= content.Length && content[position] == JpegMarker)
        {
            var marker = content[position + 1];
            // A marker may be preceded by extra FF fill bytes.
            if (marker == JpegMarker)
            {
                position++;
                continue;
            }
            if (marker == JpegStartOfScan)
            {
                kept.Write(ImageDataFrom(content, position));
                return kept.ToArray();
            }

            var length = BinaryPrimitives.ReadUInt16BigEndian(content.AsSpan(position + 2));
            var end = position + 2 + length;
            if (length < 2 || end > content.Length)
            {
                break;
            }
            var segment = content.AsSpan(position, end - position);
            if (marker == JpegApp1)
            {
                kept.Write(KeptJpegApp1(segment));
            }
            else if (!RemovedJpegSegments.Contains(marker))
            {
                kept.Write(segment);
            }
            position = end;
        }

        kept.Write(content, position, content.Length - position);
        return kept.ToArray();
    }

    // An APP1 segment holds either EXIF or XMP. Of EXIF only a new segment with the orientation
    // is kept; XMP, an upright photo, and anything unreadable leave nothing.
    private static byte[] KeptJpegApp1(ReadOnlySpan<byte> segment)
    {
        var data = segment[JpegSegmentHeaderLength..];
        if (!data.StartsWith(JpegExifHeader))
        {
            return [];
        }
        var orientation = ExifOrientation.ReadFrom(data[JpegExifHeader.Length..]);
        if (orientation is null)
        {
            return [];
        }

        byte[] newData = [.. JpegExifHeader, .. ExifOrientation.Only(orientation.Value)];
        var length = newData.Length + 2;
        return [JpegMarker, JpegApp1, (byte)(length >> 8), (byte)length, .. newData];
    }

    // The image data ends with the end-of-image marker; anything after it (for example a second
    // picture that phones append, with its own EXIF) is not part of the image and is dropped.
    private static ReadOnlySpan<byte> ImageDataFrom(byte[] content, int startOfScan)
    {
        var imageData = content.AsSpan(startOfScan);
        var endOfImage = imageData.IndexOf([JpegMarker, JpegEndOfImage]);
        return endOfImage < 0 ? imageData : imageData[..(endOfImage + 2)];
    }

    // A PNG is a signature followed by chunks (length, type, data, checksum) up to IEND.
    private static byte[] RemoveFromPng(byte[] content)
    {
        using var kept = new MemoryStream(content.Length);
        kept.Write(content, 0, PngSignatureLength);
        var position = PngSignatureLength;

        while (position + PngChunkFrameLength <= content.Length)
        {
            var length = BinaryPrimitives.ReadUInt32BigEndian(content.AsSpan(position));
            var type = Encoding.ASCII.GetString(content, position + 4, 4);
            var end = position + PngChunkFrameLength + (long)length;
            if (end > content.Length)
            {
                break;
            }
            if (type == PngExif)
            {
                kept.Write(KeptPngExif(content.AsSpan(position + 8, (int)length)));
            }
            else if (!RemovedPngChunks.Contains(type))
            {
                kept.Write(content, position, (int)(end - position));
            }
            position = (int)end;
            if (type == "IEND")
            {
                return kept.ToArray();
            }
        }

        kept.Write(content, position, content.Length - position);
        return kept.ToArray();
    }

    // Of an eXIf chunk only a new chunk with the orientation is kept; an upright photo and
    // anything unreadable leave nothing.
    private static byte[] KeptPngExif(ReadOnlySpan<byte> data)
    {
        var orientation = ExifOrientation.ReadFrom(data);
        if (orientation is null)
        {
            return [];
        }

        byte[] typeAndData = [.. Encoding.ASCII.GetBytes(PngExif), .. ExifOrientation.Only(orientation.Value)];
        var chunk = new byte[typeAndData.Length + 8];
        BinaryPrimitives.WriteUInt32BigEndian(chunk, (uint)(typeAndData.Length - 4));
        typeAndData.CopyTo(chunk, 4);
        BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(chunk.Length - 4), Crc32(typeAndData));
        return chunk;
    }

    // The checksum every PNG chunk ends with, computed over its type and data;
    // a viewer ignores a chunk whose checksum does not match.
    private static uint Crc32(ReadOnlySpan<byte> bytes)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
        }
        return ~crc;
    }
}
