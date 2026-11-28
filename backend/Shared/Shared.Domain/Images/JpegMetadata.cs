using System.Buffers.Binary;

namespace Shared.Domain.Images;

// A JPEG is a list of segments (marker, length, data) up to the start of the image data.
internal static class JpegMetadata
{
    private const int StartLength = 2;
    private const int SegmentHeaderLength = 4;
    private const byte Marker = 0xFF;
    private const byte StartOfScan = 0xDA;
    private const byte EndOfImage = 0xD9;
    // APP1 holds EXIF and XMP.
    private const byte App1 = 0xE1;
    // APP13 holds IPTC, COM holds a free-text comment.
    private static readonly byte[] RemovedSegments = [0xED, 0xFE];
    private static readonly byte[] ExifHeader = "Exif\0\0"u8.ToArray();

    public static byte[] RemoveFrom(byte[] content)
    {
        using var kept = new MemoryStream(content.Length);
        kept.Write(content, 0, StartLength);
        var position = StartLength;

        while (SegmentAt(content, position) is (var marker, var end))
        {
            if (marker == StartOfScan)
            {
                kept.Write(ImageDataFrom(content, position));
                return kept.ToArray();
            }
            kept.Write(KeptSegment(marker, content.AsSpan(position, end - position)));
            position = end;
        }

        kept.Write(content, position, content.Length - position);
        return kept.ToArray();
    }

    // Returns the marker and the end of the segment that starts at the position, or null when
    // the file cannot be read there. An extra FF fill byte before a marker counts as a segment
    // of one byte, so it is skipped like any other removed segment.
    private static (byte Marker, int End)? SegmentAt(byte[] content, int position)
    {
        if (position + SegmentHeaderLength > content.Length || content[position] != Marker)
        {
            return null;
        }
        var marker = content[position + 1];
        if (marker == Marker)
        {
            return (marker, position + 1);
        }

        var length = BinaryPrimitives.ReadUInt16BigEndian(content.AsSpan(position + 2));
        var end = position + 2 + length;
        return length < 2 || end > content.Length ? null : (marker, end);
    }

    private static byte[] KeptSegment(byte marker, ReadOnlySpan<byte> segment) => marker switch
    {
        Marker => [],
        App1 => KeptApp1(segment),
        _ when RemovedSegments.Contains(marker) => [],
        _ => segment.ToArray()
    };

    // An APP1 segment holds either EXIF or XMP. Of EXIF only a new segment with the orientation
    // is kept; XMP, an upright photo, and anything unreadable leave nothing.
    private static byte[] KeptApp1(ReadOnlySpan<byte> segment)
    {
        var data = segment[SegmentHeaderLength..];
        var orientation = data.StartsWith(ExifHeader) ? ExifOrientation.ReadFrom(data[ExifHeader.Length..]) : null;
        if (orientation is null)
        {
            return [];
        }

        byte[] newData = [.. ExifHeader, .. ExifOrientation.Only(orientation.Value)];
        var length = newData.Length + 2;
        return [Marker, App1, (byte)(length >> 8), (byte)length, .. newData];
    }

    // The image data ends with the end-of-image marker; anything after it (for example a second
    // picture that phones append, with its own EXIF) is not part of the image and is dropped.
    private static ReadOnlySpan<byte> ImageDataFrom(byte[] content, int startOfScan)
    {
        var imageData = content.AsSpan(startOfScan);
        var endOfImage = imageData.IndexOf([Marker, EndOfImage]);
        return endOfImage < 0 ? imageData : imageData[..(endOfImage + 2)];
    }
}
