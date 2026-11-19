using System.Text;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;
using Xunit;

namespace Shared.Domain.Tests.Unit.Images;

public class ImageFileTests
{
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];

    private static readonly byte[] JpegStart = [0xFF, 0xD8];
    private static readonly byte[] JpegHeader = JpegSegment(0xE0, "JFIF\0");
    private static readonly byte[] JpegExif = JpegSegment(0xE1, "Exif\0\0GPS 45.25 19.86");
    private static readonly byte[] JpegXmp = JpegSegment(0xE1, "http://ns.adobe.com/xap/1.0/\0<exif:GPSLatitude>45,15N</exif:GPSLatitude>");
    private static readonly byte[] JpegComment = JpegSegment(0xFE, "Snimljeno kod kuce");
    private static readonly byte[] JpegImageData = [.. JpegSegment(0xDA, "scan"), 0x12, 0x34, 0xFF, 0x00, 0x56, 0xFF, 0xD9];

    // A little-endian EXIF, as phones write it: the orientation (6, turned right) and a pointer to the GPS data.
    private static readonly byte[] ExifTurnedRight =
    [
        .. "II"u8, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00,
        0x02, 0x00,
        0x12, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00,
        0x25, 0x88, 0x04, 0x00, 0x01, 0x00, 0x00, 0x00, 0x26, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        .. "GPS 45.25 19.86"u8
    ];
    // The same orientation alone, big-endian.
    private static readonly byte[] ExifOnlyTurnedRight =
    [
        .. "MM"u8, 0x00, 0x2A, 0x00, 0x00, 0x00, 0x08,
        0x00, 0x01,
        0x01, 0x12, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x06, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00
    ];
    private static readonly byte[] JpegExifTurnedRight = JpegSegment(0xE1, [.. "Exif\0\0"u8, .. ExifTurnedRight]);
    private static readonly byte[] JpegOrientationTurnedRight = JpegSegment(0xE1, [.. "Exif\0\0"u8, .. ExifOnlyTurnedRight]);

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] PngHeader = PngChunk("IHDR", "13-byte-head!");
    private static readonly byte[] PngImageData = PngChunk("IDAT", "pixels");
    private static readonly byte[] PngEnd = PngChunk("IEND", "");
    // The checksum was computed separately (Python zlib.crc32 over "eXIf" and the data), because a viewer ignores a chunk without a valid one.
    private static readonly byte[] PngOrientationTurnedRight = PngChunk("eXIf", ExifOnlyTurnedRight, [0xD6, 0x67, 0x4B, 0x69]);

    private static byte[] ContentStartingWith(byte[] signature, int length)
    {
        var content = new byte[length];
        signature.CopyTo(content, 0);
        return content;
    }

    private static byte[] JpegSegment(byte marker, string data) => JpegSegment(marker, Encoding.ASCII.GetBytes(data));

    private static byte[] JpegSegment(byte marker, byte[] data)
    {
        var length = data.Length + 2;
        return [0xFF, marker, (byte)(length >> 8), (byte)length, .. data];
    }

    // A little-endian EXIF with only the orientation, whose directory starts at the given offset.
    private static byte[] JpegExifWithOrientation(uint directoryOffset, int orientation) => JpegSegment(0xE1,
    [
        .. "Exif\0\0"u8, .. "II"u8, 0x2A, 0x00, .. BitConverter.GetBytes(directoryOffset),
        0x01, 0x00,
        0x12, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, (byte)orientation, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00
    ]);

    private static byte[] PngChunk(string type, string data) => PngChunk(type, Encoding.ASCII.GetBytes(data), [0, 0, 0, 0]);

    private static byte[] PngChunk(string type, byte[] data, byte[] checksum)
    {
        var length = data.Length;
        return [0, 0, (byte)(length >> 8), (byte)length, .. Encoding.ASCII.GetBytes(type), .. data, .. checksum];
    }

    [Theory]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF }, ImageFormat.Jpeg)]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ImageFormat.Png)]
    public void Content_with_a_jpeg_or_png_signature_becomes_an_image_of_that_format(byte[] signature, ImageFormat format)
    {
        var image = new ImageFile(ContentStartingWith(signature, 1024));

        image.Format.Should().Be(format);
    }

    [Theory]
    [InlineData("GIF89a")]
    [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>")]
    public void Content_whose_signature_is_neither_jpeg_nor_png_is_rejected(string start)
    {
        var content = ContentStartingWith(Encoding.ASCII.GetBytes(start), 1024);

        var creation = () => new ImageFile(content);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Image_of_exactly_5_mb_is_accepted()
    {
        var content = ContentStartingWith(JpegSignature, 5 * 1024 * 1024);

        var image = new ImageFile(content);

        image.Content.Should().HaveCount(5 * 1024 * 1024);
    }

    [Fact]
    public void Image_larger_than_5_mb_is_rejected()
    {
        var content = ContentStartingWith(JpegSignature, 5 * 1024 * 1024 + 1);

        var creation = () => new ImageFile(content);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Empty_file_is_rejected()
    {
        var creation = () => new ImageFile([]);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Exif_xmp_and_comments_of_a_jpeg_are_not_kept()
    {
        byte[] withMetadata = [.. JpegStart, .. JpegHeader, .. JpegExif, .. JpegXmp, .. JpegComment, .. JpegImageData];

        var image = new ImageFile(withMetadata);

        image.Content.Should().Equal([.. JpegStart, .. JpegHeader, .. JpegImageData]);
    }

    [Fact]
    public void Orientation_of_a_jpeg_is_kept_without_the_rest_of_its_exif()
    {
        byte[] turnedPhoto = [.. JpegStart, .. JpegHeader, .. JpegExifTurnedRight, .. JpegImageData];

        var image = new ImageFile(turnedPhoto);

        image.Content.Should().Equal([.. JpegStart, .. JpegHeader, .. JpegOrientationTurnedRight, .. JpegImageData]);
    }

    [Theory]
    [InlineData(0x08u, 1)]
    [InlineData(0xF0u, 6)]
    [InlineData(0xFFFFFFFFu, 6)]
    public void Exif_of_an_upright_jpeg_or_one_that_cannot_be_read_is_not_kept(uint directoryOffset, int orientation)
    {
        byte[] photo = [.. JpegStart, .. JpegHeader, .. JpegExifWithOrientation(directoryOffset, orientation), .. JpegImageData];

        var image = new ImageFile(photo);

        image.Content.Should().Equal([.. JpegStart, .. JpegHeader, .. JpegImageData]);
    }

    [Fact]
    public void Exif_after_fill_bytes_of_a_jpeg_is_not_kept()
    {
        byte[] withFillBytes = [.. JpegStart, .. JpegHeader, 0xFF, 0xFF, .. JpegExif, .. JpegImageData];

        var image = new ImageFile(withFillBytes);

        image.Content.Should().Equal([.. JpegStart, .. JpegHeader, .. JpegImageData]);
    }

    [Fact]
    public void Data_after_the_end_of_a_jpeg_is_not_kept()
    {
        byte[] withAppendedPicture = [.. JpegStart, .. JpegHeader, .. JpegImageData, .. JpegStart, .. JpegExif, .. JpegImageData];

        var image = new ImageFile(withAppendedPicture);

        image.Content.Should().Equal([.. JpegStart, .. JpegHeader, .. JpegImageData]);
    }

    [Fact]
    public void Exif_and_text_chunks_of_a_png_are_not_kept()
    {
        byte[] withMetadata = [.. PngSignature, .. PngHeader, .. PngChunk("tEXt", "Author\0Maja"), .. PngChunk("eXIf", "GPS 45.25 19.86"), .. PngImageData, .. PngEnd];

        var image = new ImageFile(withMetadata);

        image.Content.Should().Equal([.. PngSignature, .. PngHeader, .. PngImageData, .. PngEnd]);
    }

    [Fact]
    public void Orientation_of_a_png_is_kept_without_the_rest_of_its_exif()
    {
        byte[] turnedPhoto = [.. PngSignature, .. PngHeader, .. PngChunk("eXIf", ExifTurnedRight, [0, 0, 0, 0]), .. PngImageData, .. PngEnd];

        var image = new ImageFile(turnedPhoto);

        image.Content.Should().Equal([.. PngSignature, .. PngHeader, .. PngOrientationTurnedRight, .. PngImageData, .. PngEnd]);
    }
}
