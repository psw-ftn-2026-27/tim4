using Exploration.Domain.TourPreferences;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourPreferences;

public sealed class TourPreferencesService
{
    private readonly ITourPreferenceRepository _preferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TourPreferencesService(ITourPreferenceRepository preferenceRepository, IUnitOfWork unitOfWork)
    {
        _preferenceRepository = preferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SaveAsync(Guid touristId, TourPreferenceDto dto)
    {
        var criteria = new PreferenceCriteria(dto.Difficulties, dto.Transport, dto.Tags);
        var preference = await _preferenceRepository.GetByTouristIdAsync(touristId);

        if (preference is null)
        {
            _preferenceRepository.Add(new TourPreference(touristId, criteria, dto.OpenSearchWithPreferences));
        }
        else
        {
            preference.Change(criteria, dto.OpenSearchWithPreferences);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid touristId)
    {
        var preference = await _preferenceRepository.GetByTouristIdAsync(touristId);
        if (preference is null)
        {
            throw new NotFoundException($"Tourist {touristId} has no tour preference.");
        }

        _preferenceRepository.Remove(preference);
        await _unitOfWork.SaveChangesAsync();
    }
}
