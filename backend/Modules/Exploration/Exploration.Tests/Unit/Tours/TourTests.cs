using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourTests
{
    private static readonly string LongDescription = new('o', 100);
    private static readonly TourPricing FreePricing = new(TourPricingType.Free, null);

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
        AddKeyPoints(tour, 2);

        tour.Publish(FreePricing);

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Tour_with_a_short_description_cannot_be_published()
    {
        var tour = CreateTour("Kratak opis.");
        tour.AddTransportTime(TransportMode.Walking, 120);
        AddKeyPoints(tour, 2);

        var publishing = () => tour.Publish(FreePricing);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_without_a_transport_time_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);
        AddKeyPoints(tour, 2);

        var publishing = () => tour.Publish(FreePricing);

        publishing.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Tour_with_fewer_than_two_key_points_cannot_be_published(int keyPointCount)
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        AddKeyPoints(tour, keyPointCount);

        var publishing = () => tour.Publish(FreePricing);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_be_published_again()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Car, 30);
        AddKeyPoints(tour, 2);
        tour.Publish(FreePricing);

        var publishing = () => tour.Publish(FreePricing);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_cannot_be_published_again()
    {
        var tour = CreateArchivedTour();

        var publishing = () => tour.Publish(FreePricing);

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

    [Fact]
    public void Adding_a_key_point_to_a_draft_appends_it_and_records_the_new_length()
    {
        var tour = CreateTour(LongDescription);
        tour.AddKeyPoint(CreateKeyPoint("Železnička stanica"), 0);
        var keyPoint = CreateKeyPoint("Trg slobode");

        tour.AddKeyPoint(keyPoint, 2.4);

        tour.KeyPoints.Should().HaveCount(2);
        tour.KeyPoints[^1].Should().Be(keyPoint);
        tour.LengthInKm.Should().Be(2.4);
    }

    [Fact]
    public void Updating_a_key_point_keeps_its_identity_and_position_and_records_the_new_length()
    {
        var tour = CreateTour(LongDescription);
        var keyPoint = CreateKeyPoint("Trg slobode");
        tour.AddKeyPoint(keyPoint, 0);
        tour.AddKeyPoint(CreateKeyPoint("Petrovaradinska tvrđava"), 2.4);

        tour.UpdateKeyPoint(keyPoint.Id, new KeyPoint("Dunavski park", "Novi opis.", "/images/park.jpg", "Tajna parka.", 45.2541, 19.8510, false), 1.9);

        tour.KeyPoints[0].Id.Should().Be(keyPoint.Id);
        tour.KeyPoints[0].Name.Should().Be("Dunavski park");
        tour.KeyPoints[0].Secret.Should().Be("Tajna parka.");
        tour.KeyPoints[0].IsVisibleWithoutPurchase.Should().BeFalse();
        tour.LengthInKm.Should().Be(1.9);
    }

    [Fact]
    public void Removing_a_key_point_drops_it_and_records_the_new_length()
    {
        var tour = CreateTour(LongDescription);
        var keyPoint = CreateKeyPoint("Trg slobode");
        tour.AddKeyPoint(keyPoint, 0);
        tour.AddKeyPoint(CreateKeyPoint("Petrovaradinska tvrđava"), 2.4);

        tour.RemoveKeyPoint(keyPoint.Id, 0);

        tour.KeyPoints.Should().NotContain(keyPoint);
        tour.LengthInKm.Should().Be(0);
    }

    [Fact]
    public void Changing_a_missing_key_point_is_rejected()
    {
        var tour = CreateTour(LongDescription);
        tour.AddKeyPoint(CreateKeyPoint("Trg slobode"), 0);

        var removal = () => tour.RemoveKeyPoint(Guid.NewGuid(), 0);

        removal.Should().Throw<NotFoundException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Tour_length_must_be_a_finite_number_that_is_not_negative(double lengthInKm)
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddKeyPoint(CreateKeyPoint("Trg slobode"), lengthInKm);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Key_points_of_a_published_tour_cannot_be_changed()
    {
        var tour = CreatePublishedTour();

        var addition = () => tour.AddKeyPoint(CreateKeyPoint("Trg slobode"), 0);

        addition.Should().Throw<DomainException>();
    }

    private static void AddKeyPoints(Tour tour, int count)
    {
        for (var index = 0; index < count; index++)
        {
            tour.AddKeyPoint(CreateKeyPoint($"Tačka {index + 1}"), index);
        }
    }

    private static KeyPoint CreateKeyPoint(string name) =>
        new(name, "Opis tačke.", "/images/tacka.jpg", null, 45.2551, 19.8452, true);

    private static Tour CreatePublishedTour()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 60);
        AddKeyPoints(tour, 2);
        tour.Publish(FreePricing);
        return tour;
    }

    private static Tour CreateArchivedTour()
    {
        var tour = CreatePublishedTour();
        tour.Archive();
        return tour;
    }
}
