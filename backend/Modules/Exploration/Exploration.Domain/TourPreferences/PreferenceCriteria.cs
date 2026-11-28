using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourPreferences;

public sealed record PreferenceCriteria
{
    public List<TourDifficulty> Difficulties { get; private init; }
    public TransportMode? Transport { get; private init; }
    public List<string> Tags { get; private init; }

    private PreferenceCriteria()
    {
        Difficulties = null!;
        Tags = null!;
    }

    public PreferenceCriteria(List<TourDifficulty>? difficulties, TransportMode? transport, List<string>? tags)
    {
        difficulties ??= [];
        tags ??= [];

        if (difficulties.Count == 0 && transport is null && tags.Count == 0)
        {
            throw new DomainException("A preference requires at least one difficulty, transport or tag.");
        }
        if (difficulties.Distinct().Count() != difficulties.Count)
        {
            throw new DomainException("A preference cannot repeat a difficulty.");
        }
        if (tags.Any(string.IsNullOrWhiteSpace))
        {
            throw new DomainException("A preference cannot have an empty tag.");
        }
        if (tags.Distinct().Count() != tags.Count)
        {
            throw new DomainException("A preference cannot repeat a tag.");
        }

        Difficulties = difficulties;
        Transport = transport;
        Tags = tags;
    }
}
