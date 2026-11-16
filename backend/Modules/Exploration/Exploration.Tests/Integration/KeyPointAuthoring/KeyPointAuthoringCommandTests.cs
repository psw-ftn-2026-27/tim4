using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeyPointAuthoring;

public class KeyPointAuthoringCommandTests : BaseIntegrationTest
{
    private static readonly byte[] PngImage = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];

    public KeyPointAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_adds_a_key_point_with_an_image_to_the_end_of_a_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.DraftWithKeyPoints;
        using var form = CreateForm("Petrovaradinska tvrđava", 3.2, PngImage);

        var response = await client.PostAsync($"/api/exploration/tours/{tour.Id}/key-points", form);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(t => t.Id == tour.Id);
        stored.KeyPoints.Should().HaveCount(tour.KeyPoints.Count + 1);
        stored.KeyPoints[^1].Name.Should().Be("Petrovaradinska tvrđava");
        stored.KeyPoints[^1].ImageAddress.Should().StartWith("/images/");
        stored.LengthInKm.Should().Be(3.2);
    }

    [Fact]
    public async Task Author_cannot_add_a_key_point_without_an_image()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.DraftWithKeyPoints;
        using var form = CreateForm("Petrovaradinska tvrđava", 3.2, null);

        var response = await client.PostAsync($"/api/exploration/tours/{tour.Id}/key-points", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(t => t.Id == tour.Id).KeyPoints.Should().HaveCount(tour.KeyPoints.Count);
    }

    [Fact]
    public async Task Author_cannot_add_a_key_point_to_a_published_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedVineyards;
        using var form = CreateForm("Podrum", 1.0, PngImage);

        var response = await client.PostAsync($"/api/exploration/tours/{tour.Id}/key-points", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(t => t.Id == tour.Id).KeyPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task Explorer_cannot_add_a_key_point_to_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var tour = TourSeed.DraftWithKeyPoints;
        using var form = CreateForm("Tuđa tačka", 3.2, PngImage);

        var response = await client.PostAsync($"/api/exploration/tours/{tour.Id}/key-points", form);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(t => t.Id == tour.Id).KeyPoints.Should().HaveCount(tour.KeyPoints.Count);
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_key_point()
    {
        var client = Factory.CreateClient();
        using var form = CreateForm("Petrovaradinska tvrđava", 3.2, PngImage);

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.DraftWithKeyPoints.Id}/key-points", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var form = CreateForm("Petrovaradinska tvrđava", 3.2, PngImage);

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.DraftWithKeyPoints.Id}/key-points", form);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_updates_a_key_point_and_keeps_its_image_when_no_new_image_is_sent()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.DraftWithKeyPoints;
        var keyPoint = tour.KeyPoints[0];
        using var form = CreateForm("Glavna železnička stanica", 2.1, null);

        var response = await client.PutAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}", form);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(t => t.Id == tour.Id);
        stored.KeyPoints[0].Id.Should().Be(keyPoint.Id);
        stored.KeyPoints[0].Name.Should().Be("Glavna železnička stanica");
        stored.KeyPoints[0].ImageAddress.Should().Be(keyPoint.ImageAddress);
        stored.LengthInKm.Should().Be(2.1);
    }

    [Fact]
    public async Task Author_removes_a_key_point_from_a_draft_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.DraftWithKeyPoints;
        var keyPoint = tour.KeyPoints[0];

        var response = await client.DeleteAsync($"/api/exploration/tours/{tour.Id}/key-points/{keyPoint.Id}?tourLengthInKm=0");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(t => t.Id == tour.Id);
        stored.KeyPoints.Should().NotContain(point => point.Id == keyPoint.Id);
        stored.LengthInKm.Should().Be(0);
    }

    private static MultipartFormDataContent CreateForm(string name, double tourLengthInKm, byte[]? image)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(name), "name" },
            { new StringContent("Opis ključne tačke."), "description" },
            { new StringContent("Tajna tačke."), "secret" },
            { new StringContent(45.2517.ToString(CultureInfo.InvariantCulture)), "latitude" },
            { new StringContent(19.8618.ToString(CultureInfo.InvariantCulture)), "longitude" },
            { new StringContent("true"), "isVisibleWithoutPurchase" },
            { new StringContent(tourLengthInKm.ToString(CultureInfo.InvariantCulture)), "tourLengthInKm" }
        };
        if (image is not null)
        {
            var file = new ByteArrayContent(image);
            file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(file, "image", "tacka.png");
        }
        return form;
    }
}
