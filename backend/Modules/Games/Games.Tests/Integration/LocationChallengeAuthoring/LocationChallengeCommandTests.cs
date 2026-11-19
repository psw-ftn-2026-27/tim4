using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Infrastructure.Persistence;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeAuthoring;

public sealed class LocationChallengeCommandTests : BaseIntegrationTest
{
    private const string Address = "/api/games/location-challenges";
    private const string CleanPng =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGN40CAAAAO0AXHtB2phAAAAAElFTkSuQmCC";
    private const string PngWithGps =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAAgGVYSWZJSSoACAAAAAEAJYgEAAEAAAAaAAAAAAAAAAQAAQACAAIAAABOAAAAAgAFAAMAAABQAAAAAwACAAIAAABFAAAABAAFAAMAAABoAAAAAAAAAC0AAAABAAAADwAAAAEAAAAAAAAAAQAAABMAAAABAAAAMwAAAAEAAAAAAAAAAQAAANT7QE0AAAAMSURBVHicY3jQIAAAA7QBce0HamEAAAAASUVORK5CYII=";

    public LocationChallengeCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_creates_a_challenge_with_multiple_hidden_points()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var form = CreateForm(2);

        using var response = await client.PostAsync(Address, form);

        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        var id = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var context = Factory.CreateContext<GamesDbContext>();
        var challenge = context.LocationChallenges.Single(item => item.Id == id);
        challenge.StartLatitude.Should().Be(45.25);
        challenge.StartLongitude.Should().Be(19.85);
        challenge.XpReward.Should().Be(25);
        challenge.HiddenLocations.Should().HaveCount(2);
        challenge.HiddenLocations.Select(point => point.Longitude).Should().BeEquivalentTo([19.86, 19.87]);
        challenge.HiddenLocations.Select(point => point.ImageAddress).Should().OnlyContain(path => path.StartsWith("/images/"));
    }

    [Fact]
    public async Task The_public_clue_contains_the_image_without_exif_gps()
    {
        using var administrator = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var form = CreateForm(1, Convert.FromBase64String(PngWithGps));

        using var response = await administrator.PostAsync(Address, form);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var context = Factory.CreateContext<GamesDbContext>();
        var challenge = context.LocationChallenges.Single(item => item.Id == id);
        var imageAddress = challenge.HiddenLocations.Single().ImageAddress;
        using var visitor = Factory.CreateClient();
        using var photo = await visitor.GetAsync(imageAddress);
        photo.StatusCode.Should().Be(HttpStatusCode.OK);
        photo.Content.Headers.ContentType?.MediaType.Should().Be("image/png");
        var image = await photo.Content.ReadAsByteArrayAsync();
        image.Should().Equal(Convert.FromBase64String(CleanPng));
    }

    [Fact]
    public async Task Challenge_without_hidden_points_cannot_be_created()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var form = CreateForm(0);

        using var response = await client.PostAsync(Address, form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Challenge_with_an_invalid_photo_cannot_be_created()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var form = CreateForm(1, [1, 2, 3]);

        using var response = await client.PostAsync(Address, form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Challenge_with_a_hidden_point_without_a_photo_cannot_be_created()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var form = CreateForm(2, includeLastPhoto: false);

        using var response = await client.PostAsync(Address, form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.LocationChallenges.Should().BeEmpty();
    }

    [Fact]
    public async Task Challenge_with_a_missing_coordinate_cannot_be_created()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var form = CreateForm(1, includePointLatitude: false);

        using var response = await client.PostAsync(Address, form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task Only_an_administrator_can_create_a_location_challenge(bool signedIn, HttpStatusCode expected)
    {
        using var client = signedIn ? Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer") : Factory.CreateClient();
        using var form = CreateForm(1);

        using var response = await client.PostAsync(Address, form);

        response.StatusCode.Should().Be(expected);
    }

    private static MultipartFormDataContent CreateForm(
        int count, byte[]? photo = null, bool includePointLatitude = true, bool includeLastPhoto = true)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent("Fotografski izazov"), "name" },
            { new StringContent("45.25"), "startLatitude" },
            { new StringContent("19.85"), "startLongitude" },
            { new StringContent("25"), "xpReward" }
        };
        for (var index = 0; index < count; index++)
        {
            if (includePointLatitude)
            {
                form.Add(new StringContent("45.26"), $"points[{index}].latitude");
            }

            form.Add(new StringContent(index == 0 ? "19.86" : "19.87"), $"points[{index}].longitude");
            if (!includeLastPhoto && index == count - 1)
            {
                continue;
            }

            var file = new ByteArrayContent(photo ?? Convert.FromBase64String(CleanPng));
            file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(file, $"points[{index}].image", "tacka.png");
        }

        return form;
    }
}
