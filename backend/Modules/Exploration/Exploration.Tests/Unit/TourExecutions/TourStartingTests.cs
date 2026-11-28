using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourStartingTests
{
    private static readonly Guid Tourist = Guid.NewGuid();
    private static readonly Location Position = new(45.25, 19.85);

    [Fact]
    public void Draft_tour_cannot_be_started_even_when_bought()
    {
        var draft = CreateDraftTour();

        var start = () => new TourStarting().Start(draft, Tourist, true, null, Position, DateTime.UtcNow);

        start.Should().Throw<DomainException>();
    }

    [Fact]
    public void Bought_archived_tour_can_be_started()
    {
        var archived = CreatePublishedTour();
        archived.Archive();

        var execution = new TourStarting().Start(archived, Tourist, true, null, Position, DateTime.UtcNow);

        execution.TourId.Should().Be(archived.Id);
    }

    [Fact]
    public void Tour_the_tourist_has_not_bought_cannot_be_started()
    {
        var tour = CreatePublishedTour();

        var start = () => new TourStarting().Start(tour, Tourist, false, null, Position, DateTime.UtcNow);

        start.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_the_tourist_has_not_bought_cannot_be_started()
    {
        var archived = CreatePublishedTour();
        archived.Archive();

        var start = () => new TourStarting().Start(archived, Tourist, false, null, Position, DateTime.UtcNow);

        start.Should().Throw<DomainException>();
    }

    [Fact]
    public void Starting_a_tour_whose_session_is_in_progress_continues_that_session_instead_of_making_a_new_one()
    {
        var tour = CreatePublishedTour();
        var inProgress = new TourExecution(tour.Id, Tourist, Position, DateTime.UtcNow.AddHours(-1));
        var now = DateTime.UtcNow;

        var execution = new TourStarting().Start(tour, Tourist, true, inProgress, Position, now);

        execution.Should().BeSameAs(inProgress);
        execution.LastActivityAt.Should().Be(now);
    }

    [Fact]
    public void Tourist_cannot_start_a_tour_while_a_session_of_another_tour_is_in_progress()
    {
        var tour = CreatePublishedTour();
        var otherTour = CreatePublishedTour();
        var inProgress = new TourExecution(otherTour.Id, Tourist, Position, DateTime.UtcNow);

        var start = () => new TourStarting().Start(tour, Tourist, true, inProgress, Position, DateTime.UtcNow);

        start.Should().Throw<DomainException>();
    }

    private static Tour CreateDraftTour()
    {
        return new Tour(Guid.NewGuid(), "Šetnja", new string('a', 100), TourDifficulty.Easy, ["grad"], new RecommendedGroupSize(1, 10));
    }

    private static Tour CreatePublishedTour()
    {
        var tour = CreateDraftTour();
        tour.AddTransportTime(TransportMode.Walking, 60);
        tour.AddKeyPoint(new KeyPoint("Prva", "Prva tačka.", "/images/prva.jpg", null, new Location(45.25, 19.85), true), 0);
        tour.AddKeyPoint(new KeyPoint("Druga", "Druga tačka.", "/images/druga.jpg", null, new Location(45.26, 19.86), true), 1);
        tour.Publish(new TourPricing(TourPricingType.Free, null));
        return tour;
    }
}
