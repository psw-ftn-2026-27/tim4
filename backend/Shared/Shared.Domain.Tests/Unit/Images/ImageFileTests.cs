using System.Text;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;
using Xunit;

namespace Shared.Domain.Tests.Unit.Images;

public class ImageFileTests
{
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];

    private static byte[] ContentStartingWith(byte[] signature, int length)
    {
        var content = new byte[length];
        signature.CopyTo(content, 0);
        return content;
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
}
