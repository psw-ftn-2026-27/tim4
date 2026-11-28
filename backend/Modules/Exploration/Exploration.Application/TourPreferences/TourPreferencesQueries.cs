namespace Exploration.Application.TourPreferences;

public sealed class TourPreferencesQueries
{
    private readonly ITourPreferenceReadRepository _preferenceReadRepository;

    public TourPreferencesQueries(ITourPreferenceReadRepository preferenceReadRepository)
    {
        _preferenceReadRepository = preferenceReadRepository;
    }

    public Task<TourPreferenceDto?> GetMineAsync(Guid touristId)
    {
        return _preferenceReadRepository.GetByTouristIdAsync(touristId);
    }
}
