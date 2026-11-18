using System.Buffers.Binary;
using System.Text;

namespace Shared.Domain.Images;

// A PNG is a signature followed by chunks (length, type, data, checksum) up to IEND.
internal static class PngMetadata
{
    private const int SignatureLength = 8;
    private const int ChunkFrameLength = 12;
    private const string Exif = "eXIf";
    private const string End = "IEND";
    private static readonly string[] RemovedChunks = ["tEXt", "zTXt", "iTXt"];

    public static byte[] RemoveFrom(byte[] content)
    {
        using var kept = new MemoryStream(content.Length);
        kept.Write(content, 0, SignatureLength);
        var position = SignatureLength;

        while (ChunkAt(content, position) is (var type, var end))
        {
            kept.Write(KeptChunk(type, content.AsSpan(position, end - position)));
            position = end;
            if (type == End)
            {
                return kept.ToArray();
            }
        }

        kept.Write(content, position, content.Length - position);
        return kept.ToArray();
    }

    // Returns the type and the end of the chunk that starts at the position, or null when the
    // file cannot be read there.
    private static (string Type, int End)? ChunkAt(byte[] content, int position)
    {
        if (position + ChunkFrameLength > content.Length)
        {
            return null;
        }
        var length = BinaryPrimitives.ReadUInt32BigEndian(content.AsSpan(position));
        var end = position + ChunkFrameLength + (long)length;
        return end > content.Length ? null : (Encoding.ASCII.GetString(content, position + 4, 4), (int)end);
    }

    // The data of a chunk sits between its length and type (8 bytes) and its checksum (4 bytes).
    private static byte[] KeptChunk(string type, ReadOnlySpan<byte> chunk) => type switch
    {
        Exif => KeptExif(chunk[8..^4]),
        _ when RemovedChunks.Contains(type) => [],
        _ => chunk.ToArray()
    };

    // Of an eXIf chunk only a new chunk with the orientation is kept; an upright photo and
    // anything unreadable leave nothing.
    private static byte[] KeptExif(ReadOnlySpan<byte> data)
    {
        var orientation = ExifOrientation.ReadFrom(data);
        if (orientation is null)
        {
            return [];
        }

        byte[] typeAndData = [.. Encoding.ASCII.GetBytes(Exif), .. ExifOrientation.Only(orientation.Value)];
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
