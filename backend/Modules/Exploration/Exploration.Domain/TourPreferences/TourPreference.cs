using Shared.Domain;

namespace Exploration.Domain.TourPreferences;

public sealed class TourPreference : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public PreferenceCriteria Criteria { get; private set; }
    public bool OpenSearchWithPreferences { get; private set; }

    private TourPreference()
    {
        Criteria = null!;
    }

    public TourPreference(Guid touristId, PreferenceCriteria criteria, bool openSearchWithPreferences)
        : base(Guid.NewGuid())
    {
        TouristId = touristId;
        Criteria = criteria;
        OpenSearchWithPreferences = openSearchWithPreferences;
    }

    public void Change(PreferenceCriteria criteria, bool openSearchWithPreferences)
    {
        Criteria = criteria;
        OpenSearchWithPreferences = openSearchWithPreferences;
    }
}
