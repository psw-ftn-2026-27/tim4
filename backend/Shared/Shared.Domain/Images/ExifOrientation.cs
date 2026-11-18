using System.Buffers.Binary;

namespace Shared.Domain.Images;

// The orientation is the only EXIF value an image keeps, because phones save upright photos
// sideways and browsers turn them by it. JPEG and PNG store EXIF as the same TIFF structure:
// a header (byte order, offset of the first directory) and directories of 12-byte entries.
internal static class ExifOrientation
{
    private const int HeaderLength = 8;
    private const int EntryLength = 12;
    private const ushort OrientationTag = 0x0112;

    // Reads a turning orientation (2 to 8) from the first directory.
    // Returns null for an upright photo (1) and for anything it cannot read.
    public static ushort? ReadFrom(ReadOnlySpan<byte> tiff)
    {
        if (tiff.Length < HeaderLength)
        {
            return null;
        }
        var bigEndian = tiff[0] == 'M';
        var entry = OrientationEntryIn(tiff, bigEndian);
        if (entry is null)
        {
            return null;
        }

        var orientation = ReadUInt16(tiff[(entry.Value + 8)..], bigEndian);
        return orientation is >= 2 and <= 8 ? orientation : null;
    }

    // Returns where the orientation entry of the first directory starts, or null.
    private static int? OrientationEntryIn(ReadOnlySpan<byte> tiff, bool bigEndian)
    {
        var directory = ReadUInt32(tiff[4..], bigEndian);
        // Written without adding to the offset, so a huge offset cannot wrap around and pass.
        if (directory > tiff.Length - 2)
        {
            return null;
        }

        var entries = ReadUInt16(tiff[(int)directory..], bigEndian);
        for (var i = 0; i < entries; i++)
        {
            var entry = (int)directory + 2 + i * EntryLength;
            if (entry + EntryLength > tiff.Length)
            {
                return null;
            }
            if (ReadUInt16(tiff[entry..], bigEndian) == OrientationTag)
            {
                return entry;
            }
        }
        return null;
    }

    // The smallest TIFF structure that still tells the browser how to turn the photo:
    // a big-endian header and one directory with the orientation as its only entry.
    public static byte[] Only(ushort orientation) =>
    [
        (byte)'M', (byte)'M', 0x00, 0x2A, 0x00, 0x00, 0x00, 0x08,
        0x00, 0x01,
        0x01, 0x12, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, (byte)(orientation >> 8), (byte)orientation, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00
    ];

    private static ushort ReadUInt16(ReadOnlySpan<byte> bytes, bool bigEndian) =>
        bigEndian ? BinaryPrimitives.ReadUInt16BigEndian(bytes) : BinaryPrimitives.ReadUInt16LittleEndian(bytes);

    private static uint ReadUInt32(ReadOnlySpan<byte> bytes, bool bigEndian) =>
        bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(bytes) : BinaryPrimitives.ReadUInt32LittleEndian(bytes);
}
