using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourTests
{
    private static readonly string LongDescription = new('o', 100);

    private static Tour CreateTour(string description) =>
        new(WellKnownUsers.Explorer, "Šetnja tvrđavom", description, TourDifficulty.Easy, ["istorija"], new RecommendedGroupSize(1, 10));

    [Fact]
    public void New_tour_starts_as_a_draft()
    {
        var tour = CreateTour(LongDescription);

        tour.Status.Should().Be(TourStatus.Draft);
        tour.PublishedAt.Should().BeNull();
        tour.TransportTimes.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_name(string name)
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, name, "Opis ture.", TourDifficulty.Easy, ["istorija"], new RecommendedGroupSize(1, 10));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_description(string description)
    {
        var creation = () => CreateTour(description);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_tour_requires_tags()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, [], new RecommendedGroupSize(1, 10));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_has_one_time_per_transport_mode()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, 90);

        addition.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Transport_time_must_be_positive(int minutes)
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, minutes);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_is_published_when_all_rules_are_met()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Bicycle, 45);

        tour.Publish();

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Tour_with_a_short_description_cannot_be_published()
    {
        var tour = CreateTour("Kratak opis.");
        tour.AddTransportTime(TransportMode.Walking, 120);

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_without_a_transport_time_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_be_published_again()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Car, 30);
        tour.Publish();

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_cannot_be_published_again()
    {
        var tour = CreateArchivedTour();

        var publishing = () => tour.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archiving_a_published_tour_records_when_it_was_archived_and_keeps_its_first_publication_time()
    {
        var tour = CreatePublishedTour();
        var publishedAt = tour.PublishedAt;

        tour.Archive();

        tour.Status.Should().Be(TourStatus.Archived);
        tour.ArchivedAt.Should().NotBeNull();
        tour.PublishedAt.Should().Be(publishedAt);
    }

    [Fact]
    public void Draft_tour_cannot_be_archived()
    {
        var tour = CreateTour(LongDescription);

        var archiving = () => tour.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_cannot_be_archived_again()
    {
        var tour = CreateArchivedTour();

        var archiving = () => tour.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reactivating_an_archived_tour_publishes_it_again_clears_the_archive_time_and_keeps_its_first_publication_time()
    {
        var tour = CreateArchivedTour();
        var publishedAt = tour.PublishedAt;

        tour.Reactivate();

        tour.Status.Should().Be(TourStatus.Published);
        tour.ArchivedAt.Should().BeNull();
        tour.PublishedAt.Should().Be(publishedAt);
    }

    [Fact]
    public void Draft_tour_cannot_be_reactivated()
    {
        var tour = CreateTour(LongDescription);

        var reactivation = () => tour.Reactivate();

        reactivation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_be_reactivated()
    {
        var tour = CreatePublishedTour();

        var reactivation = () => tour.Reactivate();

        reactivation.Should().Throw<DomainException>();
    }

    private static Tour CreatePublishedTour()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 60);
        tour.Publish();
        return tour;
    }

    private static Tour CreateArchivedTour()
    {
        var tour = CreatePublishedTour();
        tour.Archive();
        return tour;
    }
}
